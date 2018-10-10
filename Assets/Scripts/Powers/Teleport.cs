using UnityEngine;

public class Teleport : Power
{
    private const string m_throwTeleportParamNameString = "ThrowTeleport";

    private int m_throwTeleportParamHashId = Animator.StringToHash(m_throwTeleportParamNameString);

    [Header("Projectile")]
    [SerializeField]
    private GameObject m_teleportProjectile;

    protected override void Awake()
    {
        base.Awake();

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
        m_armsAnimator.SetTrigger(m_throwTeleportParamHashId);
        IsBusy = true;

        Debug.Log(Time.frameCount + " Use");

        return IsBusy;
    }

    // Methods of the IAnimatorEventSubscriber interface used by the parent class
    public override void NotifyEvent(string eventName)
    {
        switch (eventName)
        {
            case ArmsEventsManager.THROW_TELEPORT_SPHERE_EVENT:
                Debug.Log(eventName);
                // TODO: Throw teleport sphere
                break;
            default:
                Debug.LogWarning("Unpredicted event: " + eventName);
                break;
        }
    }
}
