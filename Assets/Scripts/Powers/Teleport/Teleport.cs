using UnityEngine;

public class Teleport : Power
{
    private const string m_throwTeleportParamNameString = "ThrowTeleport";

    private int m_throwTeleportParamHashId = Animator.StringToHash(m_throwTeleportParamNameString);

    [Header("Projectile")]
    [SerializeField]
    private GameObject m_projectile;
    [SerializeField]
    private float m_projectileMass = 0.4f;
    [SerializeField]
    private float m_projectileDrag = 1.0f;
    [SerializeField]
    private float m_throwRange = 20.0f;
    [SerializeField]
    private LayerMask m_throwDetectionLayer;
    [SerializeField]
    private float m_throwForce = 20.0f;

    [Header("Projectile Container")]
    [SerializeField]
    private Transform m_projectileHoldContainer;
    [SerializeField]
    private Vector3 m_projectilePositionOffset = Vector3.zero;
    [SerializeField]
    private Vector3 m_projectileRotationOffset = Vector3.zero;

    private bool m_startedThrowing = false;
    private bool m_projectileThrowed = false;

    private Rigidbody m_projectileRigidbody;

    [Header("Character")]
    [SerializeField]
    private Transform m_character;
    private CapsuleCollider m_characterCollider;
    private Rigidbody m_characterRigidbody;
    [SerializeField]
    private Camera m_characterCamera;

    protected override void Awake()
    {
        base.Awake();

        IsChargeable = false;

        m_characterCollider = m_character.GetComponent<CapsuleCollider>();
        m_characterRigidbody = m_character.GetComponent<Rigidbody>();
    }

    public override void Initialise(Animator armsAnimator, ArmsEventsManager armsEventsManager, bool show)
    {
        base.Initialise(armsAnimator, armsEventsManager, show);

        // Prevent the projectile from colliding with the character SphereCollider
        Physics.IgnoreCollision(m_characterCollider, m_projectile.GetComponent<SphereCollider>());

        // Make sure that the projectile has no rigidbody
        Rigidbody projectileRgidbody = m_projectile.GetComponent<Rigidbody>();

        if (projectileRgidbody)
        {
            Destroy(projectileRgidbody);
        }

        ReplaceProjectileInHand();
    }

    protected override void SubscribeToEvents()
    {
        m_armsEventsManager.Subscribe(ArmsEventsManager.THROW_TELEPORT_SPHERE_EVENT, this);
    }

    public override void Show(bool show)
    {
        m_projectile.SetActive(show);
    }

    public override bool Use()
    {
        // If the the projectile hasn't started being thrown
        if(!m_startedThrowing)
        {
            m_startedThrowing = true;
            CanBeStop = false;

            m_armsAnimator.SetTrigger(m_throwTeleportParamHashId);

            return true;
        }
        // If the projectile was throwed
        else if (m_projectileThrowed)
        {
            // TODO: Check if there is enough space
            m_characterRigidbody.position = m_projectile.transform.position - m_characterCollider.center;
            
            RecoverProjectile();

            return true;
        }

        return false;
    }

    public override bool Cancel()
    {
        if (m_startedThrowing && m_projectileThrowed)
        {
            RecoverProjectile();

            return true;
        }

        return false;
    }

    private void ThrowProjectile()
    {
        m_projectile.transform.SetParent(null);

        AddRigidbodyToProjectile();

        RaycastHit hit;
        
        if (Physics.Raycast(m_characterCamera.transform.position, m_characterCamera.transform.forward, out hit, m_throwRange, m_throwDetectionLayer))
        {
            m_projectileRigidbody.AddForce((hit.point - m_characterCamera.transform.position).normalized * m_throwForce, ForceMode.Impulse);
        }
        else
        {
            Vector3 throwDirection = ((m_characterCamera.transform.position + m_characterCamera.transform.forward * m_throwRange) - m_characterCamera.transform.position).normalized;
            m_projectileRigidbody.AddForce(throwDirection * m_throwForce, ForceMode.Impulse);
        }

        m_projectileThrowed = true;
    }

    private void AddRigidbodyToProjectile()
    {
        if (!m_projectileRigidbody)
        {
            m_projectileRigidbody = m_projectile.AddComponent<Rigidbody>();
            m_projectileRigidbody.isKinematic = false;
            m_projectileRigidbody.useGravity = true;
            m_projectileRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            m_projectileRigidbody.mass = m_projectileMass;
            m_projectileRigidbody.drag = m_projectileDrag;
        }
    }

    private void RecoverProjectile()
    {
        // Destroy the rigidbody if it still exist
        if (m_projectileRigidbody)
        {
            Destroy(m_projectileRigidbody);
        }

        ReplaceProjectileInHand();

        m_startedThrowing = false;
        m_projectileThrowed = false;
        CanBeStop = true;
    }

    private void ReplaceProjectileInHand()
    {
        m_projectile.transform.SetParent(m_projectileHoldContainer);
        m_projectile.transform.localPosition = m_projectilePositionOffset;
        m_projectile.transform.rotation = Quaternion.Euler(m_projectileRotationOffset);
    }

    // Methods of the IAnimatorEventSubscriber interface used by the parent class
    public override void NotifyEvent(string eventName)
    {
        switch (eventName)
        {
            case ArmsEventsManager.THROW_TELEPORT_SPHERE_EVENT:
                ThrowProjectile();
                break;
            default:
                Debug.LogWarning("Unpredicted event: " + eventName);
                break;
        }
    }
}
