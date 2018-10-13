using UnityEngine;

public class Teleport : Power
{
    private const string m_throwTeleportParamNameString = "ThrowTeleport";

    private int m_throwTeleportParamHashId = Animator.StringToHash(m_throwTeleportParamNameString);

    [Header("Projectile")]
    [SerializeField]
    private GameObject m_teleportProjectile;
    [SerializeField]
    private float m_throwRange = 20.0f;
    [SerializeField]
    private LayerMask m_detectionLayer;
    [SerializeField]
    private float m_throwForce = 20.0f;

    private bool m_startedThrowing = false;
    private bool m_projectileThrowed = false;

    private Rigidbody m_teleportProjectileRigidbody;

    [Header("Character")]
    [SerializeField]
    private Collider m_characterCollider;
    [SerializeField]
    private Camera m_characterCamera;

    protected override void Awake()
    {
        base.Awake();

        Physics.IgnoreCollision(m_characterCollider, m_teleportProjectile.GetComponent<SphereCollider>());

        m_teleportProjectileRigidbody = m_teleportProjectile.GetComponent<Rigidbody>();
        m_teleportProjectileRigidbody.isKinematic = true;

        IsChargeable = false;
    }

    protected override void SubscribeToEvents()
    {
        m_armsEventsManager.Subscribe(ArmsEventsManager.THROW_TELEPORT_SPHERE_EVENT, this);
    }

    public override void Show(bool show)
    {
        m_teleportProjectile.SetActive(show);
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
            // TODO: Activate teleport
            Debug.Log("Activate");

            return true;
        }

        return false;
    }

    private void ThrowTeleportSphere()
    {
        m_teleportProjectile.transform.SetParent(null);
        m_teleportProjectileRigidbody.isKinematic = false;

        RaycastHit hit;
        
        if (Physics.Raycast(m_characterCamera.transform.position, m_characterCamera.transform.forward, out hit, m_throwRange, m_detectionLayer))
        {
            m_teleportProjectileRigidbody.AddForce((hit.point - m_characterCamera.transform.position).normalized * m_throwForce, ForceMode.Impulse);
        }
        else
        {
            Vector3 throwDirection = ((m_characterCamera.transform.position + m_characterCamera.transform.forward * m_throwRange) - m_characterCamera.transform.position).normalized;
            m_teleportProjectileRigidbody.AddForce(throwDirection * m_throwForce, ForceMode.Impulse);
        }

        m_projectileThrowed = true;
    }

    // Methods of the IAnimatorEventSubscriber interface used by the parent class
    public override void NotifyEvent(string eventName)
    {
        switch (eventName)
        {
            case ArmsEventsManager.THROW_TELEPORT_SPHERE_EVENT:
                ThrowTeleportSphere();
                break;
            default:
                Debug.LogWarning("Unpredicted event: " + eventName);
                break;
        }
    }
}
