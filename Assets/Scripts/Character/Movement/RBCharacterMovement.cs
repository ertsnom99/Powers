using System.Collections.Generic;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]

public class RBCharacterMovement : MonoBehaviour
{
    [Header("Kinematics")]
    [SerializeField]
    private float m_rotationSpeed = 115.0f;
    private float m_lastRotationInput = .0f;
    [SerializeField]
    private float m_lockOnRotationSpeed = 10.0f;
    private Transform m_lockOnTarget = null;

    [SerializeField]
    private float m_maxXWalkSpeed = 4.5f;
    [SerializeField]
    private float m_maxZWalkSpeed = 4.5f;

    [SerializeField]
    private float m_acceleration = 20.0f;
    [SerializeField]
    private float m_decceleration = 32.0f;
    
    private Vector2 m_movementInputs = Vector3.zero;

    private Vector3 m_localVelocity = Vector3.zero;
    private Vector3 m_globalVelocity = Vector3.zero;
    private Vector3 m_globalMovement = Vector3.zero;

    private Vector3 m_lastPosition;

    [Header("Running")]
    [SerializeField]
    private bool m_runningAllowed = true;
    [SerializeField]
    private float m_minMovInputForRun = 0.5f;

    public bool IsRunning { get; private set; }

    [Header("Jumping")]
    [SerializeField]
    private float m_jumpHeight = 2.0f;
    [SerializeField]
    private bool m_controlWhileNotGrounded = true;

    [Header("Grounded")]
    [SerializeField]
    private Vector3 m_groundedCheckPosition = new Vector3(.0f, 0.495f, .0f);
    [SerializeField]
    private float m_groundedCheckRange = 0.505f;
    [SerializeField]
    private LayerMask m_groundedDetectionLayer;

    public bool IsGrounded { get; private set; }

    // Modifiers are a percentage of how much of the movement must be executed, NOT how much ISN'T executed
    [Header("Modifiers")]
    [SerializeField]
    private float m_sideStepModifier = 1.0f;
    [SerializeField]
    private float m_backwardModifier = 1.0f;
    [SerializeField]
    private float m_airborneModifier = 0.2f;
    [SerializeField]
    private float m_runModifier = 2.4f;
    private float m_globalModifier = 1.0f;

    private Dictionary<string, float> m_currentModifiers;

    [Header("Debug")]
    [SerializeField]
    private bool m_debugGroundDetectionSphere = false;
    [SerializeField]
    private bool m_drawVelocity = false;

    private Rigidbody m_rigidbody;

    private void Start()
    {
        IsGrounded = false;
        m_lastPosition = transform.position;

        m_rigidbody = GetComponent<Rigidbody>();
    }

    public void UpdateMovement(Inputs inputs, Transform lockOnTarget = null)
    {
        m_lockOnTarget = lockOnTarget;

        if (!m_lockOnTarget)
        {
            RegisterRotationInput(inputs);
        }

        MoveCharacter(inputs);
    }

    private void RegisterRotationInput(Inputs inputs)
    {
        m_lastRotationInput = inputs.xAxis;
    }

    // This method actually set the values that the FixedUpdate needes to move the character  
    private void MoveCharacter(Inputs inputs)
    {
        m_movementInputs = new Vector2(inputs.horizontal, inputs.vertical);
        if (m_movementInputs.magnitude > 1.0f) m_movementInputs.Normalize();
        
        IsGrounded = Physics.CheckSphere(transform.position + m_groundedCheckPosition, m_groundedCheckRange, m_groundedDetectionLayer, QueryTriggerInteraction.Ignore);

        TransferVelocityOnLandingAndLeavingGround();
        
        UpdateRunningState(CanRun() && inputs.running);

        if (inputs.jump && IsGrounded) Jump();

        /*if (Input.GetButtonDown("Dash"))
        {
            https://medium.com/ironequal/unity-character-controller-vs-rigidbody-a1e243591483
            Vector3 dashVelocity = Vector3.Scale(transform.forward, m_dashDistance * new Vector3((Mathf.Log(1f / (Time.deltaTime * m_rigidbody.drag + 1)) / -Time.deltaTime), 0, (Mathf.Log(1f / (Time.deltaTime * m_rigidbody.drag + 1)) / -Time.deltaTime)));
            m_rigidbody.AddForce(dashVelocity, ForceMode.VelocityChange);
        }*/

        // Do all the calculation to determine the global movement to be used in next FixedUpdate
        CalculateCurrentModifiers();

        CalculateLocalXVelocity();
        CalculateLocalZVelocity();

        CalculateGlobalMovement();

        // Update the position informations
        Vector3 lastMovement = transform.position - m_lastPosition;
        m_lastPosition = transform.position;

        // TODO: animate

        // Debugs
        if(m_drawVelocity)
        {
            Debug.DrawLine(transform.position, transform.position + m_rigidbody.velocity, Color.blue);
        }
    }

    private void TransferVelocityOnLandingAndLeavingGround()
    {
        if (IsGrounded && m_globalVelocity != Vector3.zero)
        {
            m_localVelocity += transform.InverseTransformDirection(m_globalVelocity);
            m_globalVelocity = Vector3.zero;
        }
        else if (!IsGrounded && m_localVelocity != Vector3.zero)
        {
            m_globalVelocity += transform.TransformDirection(m_localVelocity);
            m_localVelocity = Vector3.zero;
        }
    }

    private void UpdateRunningState(bool running)
    {
        if (!IsRunning && running)
        {
            IsRunning = true;
        }
        else if (IsRunning && !running)
        {
            IsRunning = false;
        }
    }

    private bool CanRun()
    {
        return (m_runningAllowed
                && IsGrounded
                && m_movementInputs.magnitude >= m_minMovInputForRun);
    }

    private void CalculateCurrentModifiers()
    {
        Dictionary<string, float> modifiers = new Dictionary<string, float>();

        modifiers.Add("backwardModifier", Mathf.Sign(m_movementInputs.y) == -1 ? m_backwardModifier : 1);
        modifiers.Add("sideStepModifier", m_movementInputs.x != 0 ? m_sideStepModifier : 1);
        modifiers.Add("airborneModifier", !IsGrounded ? m_airborneModifier : 1);
        modifiers.Add("runningModifier", IsRunning ? m_runModifier : 1);

        m_currentModifiers = modifiers;
    }
    
    private void CalculateLocalXVelocity()
    {
        float previousVelocityX = m_localVelocity.x;
        float currentMaxVelocityX = m_maxXWalkSpeed * m_movementInputs.x * m_currentModifiers["sideStepModifier"] * m_currentModifiers["runningModifier"] * m_globalModifier;

        // If the character moves without having achieved is maximum horizontal velocity
        if (m_movementInputs.x != 0 && previousVelocityX != currentMaxVelocityX && (IsGrounded || m_controlWhileNotGrounded))
        {
            m_localVelocity.x += Mathf.Abs(m_movementInputs.x) * m_acceleration * Mathf.Sign(currentMaxVelocityX - m_localVelocity.x) * m_currentModifiers["sideStepModifier"] * m_currentModifiers["airborneModifier"] * m_currentModifiers["runningModifier"] * Time.deltaTime;

            if (VelocityExceedMax(previousVelocityX, m_localVelocity.x, currentMaxVelocityX))
            {
                m_localVelocity.x = currentMaxVelocityX;
            }
        }
        // If the character doesn't want to move, but didn't loose all is horizontal velocity
        else if (m_movementInputs.x == 0 && IsGrounded && previousVelocityX != 0)
        {
            m_localVelocity.x -= m_decceleration * Mathf.Sign(previousVelocityX) * m_currentModifiers["sideStepModifier"] * m_currentModifiers["airborneModifier"] * m_currentModifiers["runningModifier"] * Time.deltaTime;

            if (Mathf.Sign(previousVelocityX) * m_localVelocity.x < 0)
            {
                m_localVelocity.x = 0;
            }
        }
    }

    private void CalculateLocalZVelocity()
    {
        float previousVelocityZ = m_localVelocity.z;
        float currentMaxVelocityZ = m_maxZWalkSpeed * m_movementInputs.y * m_currentModifiers["backwardModifier"] * m_currentModifiers["runningModifier"] * m_globalModifier;

        // If the character moves without having achived is maximum vertical velocity
        if (m_movementInputs.y != 0 && previousVelocityZ != currentMaxVelocityZ && (IsGrounded || m_controlWhileNotGrounded))
        {
            m_localVelocity.z += Mathf.Abs(m_movementInputs.y) * m_acceleration * Mathf.Sign(currentMaxVelocityZ - m_localVelocity.z) * m_currentModifiers["backwardModifier"] * m_currentModifiers["airborneModifier"] * m_currentModifiers["runningModifier"] * Time.deltaTime;

            if (VelocityExceedMax(previousVelocityZ, m_localVelocity.z, currentMaxVelocityZ))
            {
                m_localVelocity.z = currentMaxVelocityZ;
            }
        }
        // If the character doesn't want to move, but didn't loose all is vertical velocity
        else if (m_movementInputs.y == 0 && IsGrounded && previousVelocityZ != 0)
        {
            m_localVelocity.z -= m_decceleration * Mathf.Sign(previousVelocityZ) * m_currentModifiers["backwardModifier"] * m_currentModifiers["airborneModifier"] * m_currentModifiers["runningModifier"] * Time.deltaTime;

            if (Mathf.Sign(previousVelocityZ) * m_localVelocity.z < 0)
            {
                m_localVelocity.z = 0;
            }
        }
    }

    private bool VelocityExceedMax(float previousVelocity, float velocity, float maxVelocity)
    {
        return ((previousVelocity < maxVelocity && velocity > maxVelocity)
             || (previousVelocity > maxVelocity && velocity < maxVelocity));
    }

    // Add current local velocity to the current global velocity and return the result
    private void CalculateGlobalMovement()
    {
        Vector3 convertedLocalVelocity = transform.TransformDirection(m_localVelocity);

        if (!IsGrounded)
        {
            m_globalVelocity += convertedLocalVelocity;
            m_localVelocity = convertedLocalVelocity = Vector3.zero;

            // Adjusts the global velocity if necessary
            Vector3 globalToLocalVelocity = transform.InverseTransformDirection(m_globalVelocity);

            float maxAirborneXVelocity = m_maxXWalkSpeed * m_sideStepModifier * m_globalModifier;
            float maxAirborneZVelocity = m_maxZWalkSpeed * m_globalModifier;

            if (m_runningAllowed)
            {
                maxAirborneXVelocity *= m_runModifier;
                maxAirborneZVelocity *= m_runModifier;
            }

            if (Mathf.Abs(globalToLocalVelocity.x) > maxAirborneXVelocity)
            {
                globalToLocalVelocity.x = Mathf.Sign(globalToLocalVelocity.x) * maxAirborneXVelocity;
            }

            if (Mathf.Abs(globalToLocalVelocity.z) > maxAirborneZVelocity)
            {
                globalToLocalVelocity.z = Mathf.Sign(globalToLocalVelocity.z) * maxAirborneZVelocity;
            }

            m_globalVelocity = transform.TransformDirection(globalToLocalVelocity);
        }

        // Calculate the final movement for this frame
        m_globalMovement = m_globalVelocity + convertedLocalVelocity;
    }

    private void Jump()
    {
        Vector3 currentVelocity = new Vector3(.0f, m_rigidbody.velocity.y, .0f);
    
        // See https://math.stackexchange.com/questions/785375/calculate-initial-velocity-to-reach-height-y  for math explained (check porglezomp answer)
        m_rigidbody.AddForce(Vector3.up * Mathf.Sqrt(m_jumpHeight * -2.0f * Physics.gravity.y) - currentVelocity, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        Vector3 currentVelocity = new Vector3(m_rigidbody.velocity.x, 0.0f, m_rigidbody.velocity.z);
        
        m_rigidbody.AddForce(m_globalMovement - currentVelocity, ForceMode.VelocityChange);
        
        if (m_lockOnTarget)
        {
            // The rotation is done over time
            Vector3 characterToTarget = m_lockOnTarget.position - transform.position;
            characterToTarget.y = 0;

            float rotationDirection = Mathf.Sign(Vector3.Cross(transform.forward, characterToTarget.normalized).y);
            float rotation = Vector3.Angle(transform.forward, Vector3.Slerp(transform.forward, characterToTarget.normalized, m_lockOnRotationSpeed * m_globalModifier * Time.fixedDeltaTime));

            m_rigidbody.AddTorque(rotationDirection * transform.up * rotation - m_rigidbody.angularVelocity, ForceMode.VelocityChange);
        }
        else
        {
            // The rotation is instantaneous
            float rotation = m_lastRotationInput * m_rotationSpeed * m_globalModifier * Time.fixedDeltaTime;

            //m_rigidbody.MoveRotation(m_rigidbody.rotation * Quaternion.AngleAxis(rotation, Vector3.up));
            m_rigidbody.AddTorque(rotation * transform.up - m_rigidbody.angularVelocity, ForceMode.VelocityChange);
        }
    }

    private void OnDrawGizmos()
    {
        if (m_debugGroundDetectionSphere)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + m_groundedCheckPosition, m_groundedCheckRange);
        }
    }
}
