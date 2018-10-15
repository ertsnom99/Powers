using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(RBCharacterMovement))]

public class PlayerControls : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField]
    private bool m_useKeyboard;

    private Inputs noControlInputs;

    private bool m_wasLeftTriggerInputDown = false;
    private bool m_wasRightTriggerInputDown = false;

    public bool ControlsEnabled { get; private set; }

    private FPSCameraMovement m_cameraControl;
    private RBCharacterMovement m_movementScript;
    private PowerManager m_powerManager;
    
    private void Awake()
    {
        noControlInputs = new Inputs();

        ControlsEnabled = true;

        m_cameraControl = GetComponentInChildren<FPSCameraMovement>();
        m_movementScript = GetComponent<RBCharacterMovement>();
        m_powerManager = GetComponentInChildren<PowerManager>();

        if (!m_cameraControl)
        {
            Debug.LogError("No FPSCameraMovement found on childrens");
        }

        if (!m_powerManager)
        {
            Debug.LogError("No PowerManager found on childrens");
        }
    }

    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > 0.0f)
        {
            // Get the inputs used during this frame
            Inputs inputs = FetchInputs();

            if (ControlsCharacter())
            {
                // Movement and camera update
                if (!OnPreventMovementControlCheck())
                {
                    UpdateCamera(inputs, inputs.lockOn);
                    UpdateMovement(inputs, inputs.lockOn);
                    UpdatePower(inputs);
                }
                else
                {
                    UpdateCamera(noControlInputs, noControlInputs.lockOn);
                    UpdateMovement(noControlInputs, noControlInputs.lockOn);
                    UpdatePower(noControlInputs);
                }
            }
            else
            {
                UpdateCamera(noControlInputs, noControlInputs.lockOn);
                UpdateMovement(noControlInputs, noControlInputs.lockOn);
                UpdatePower(noControlInputs);
            }
        }
    }

    private Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();
        
	    if (m_useKeyboard)
	    {
            // Inputs from the keyboard
            inputs.vertical = Input.GetAxisRaw("Vertical");
            inputs.horizontal = Input.GetAxisRaw("Horizontal");
            inputs.running = Input.GetButton("Run");
            inputs.jump = Input.GetButtonDown("Jump");
            inputs.xAxis = Input.GetAxis("Mouse X");
            inputs.yAxis = Input.GetAxis("Mouse Y");

            inputs.lockOn = Input.GetButton("Lock");

            inputs.previousPower = Input.GetKeyDown("z");
            inputs.nextPower = Input.GetKeyDown("c");

            inputs.leftTriggerDown = Input.GetKeyDown("q");
            inputs.rightTriggerDown = Input.GetKeyDown("e");

            inputs.leftTriggerUp = Input.GetKeyUp("q");
            inputs.rightTriggerUp = Input.GetKeyUp("e");

            inputs.holdLeftTrigger = Input.GetKey("q");
            inputs.holdRightTrigger = Input.GetKey("e");

            inputs.cancelPower = Input.GetKey("x");
        }
	    else
	    {
            // Inputs grom the
            inputs.vertical = Input.GetAxisRaw("Controller Vertical");
            inputs.horizontal = Input.GetAxisRaw("Controller Horizontal");
            inputs.running = Input.GetButton("Controller Run");
            inputs.jump = Input.GetButtonDown("Controller Jump");
            inputs.xAxis = Input.GetAxis("Right Stick X");
            inputs.yAxis = Input.GetAxis("Right Stick Y");

            inputs.lockOn = Input.GetButton("Controller Lock");

            inputs.previousPower = Input.GetButtonDown("Left Bumper");
            inputs.nextPower = Input.GetButtonDown("Right Bumper");

            inputs.leftTriggerDown = !m_wasLeftTriggerInputDown && Input.GetAxis("Left Trigger") == 1.0f;
            inputs.rightTriggerDown = !m_wasRightTriggerInputDown && Input.GetAxis("Right Trigger") == 1.0f;
            
            inputs.leftTriggerUp = m_wasLeftTriggerInputDown && Input.GetAxis("Left Trigger") != 1.0f;
            inputs.rightTriggerUp = m_wasRightTriggerInputDown && Input.GetAxis("Right Trigger") != 1.0f;
            
            inputs.holdLeftTrigger = m_wasLeftTriggerInputDown = Input.GetAxis("Left Trigger") == 1.0f;
            inputs.holdRightTrigger = m_wasRightTriggerInputDown = Input.GetAxis("Right Trigger") == 1.0f;

            inputs.cancelPower = Input.GetButtonDown("Fire2");
        }

        return inputs;
	}

    public void SetKeyboardUse(bool useKeyboard)
    {
        m_useKeyboard = useKeyboard;
    }

    private bool ControlsCharacter()
    {
        return ControlsEnabled;
    }

    private bool OnPreventMovementControlCheck()
    {
        return false;
    }

    private void UpdateCamera(Inputs inputs, bool lockOn)
    {
        Transform[] lockableTargets = null;

        if (lockOn)
        {
            // TODO: Get all targets that can be locked on
        }

        m_cameraControl.RotateCamera(inputs, lockableTargets);
    }

    private void UpdateMovement(Inputs inputs, bool lockOn)
    {
        m_movementScript.MoveCharacter(inputs, lockOn ? m_cameraControl.TargetLockedOn : null);
    }

    private void UpdatePower(Inputs inputs)
    {
        if (inputs.previousPower && m_powerManager.SelectPreviousPower())
        {
            return;
        }

        if (inputs.nextPower && m_powerManager.SelectNextPower())
        {
            return;
        }

        if (inputs.holdLeftTrigger && m_powerManager.StartChargingPower())
        {
            return;
        }

        if (inputs.leftTriggerDown && m_powerManager.UsePower())
        {
            return;
        }

        if (inputs.leftTriggerUp && m_powerManager.StopChargingPower())
        {
            m_powerManager.UsePower();
            return;
        }

        if (inputs.cancelPower && m_powerManager.CancelPower())
        {
            return;
        }
    }
    
    public void EnableControl(bool enable)
    {
        ControlsEnabled = enable;
    }
}
