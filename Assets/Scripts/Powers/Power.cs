using UnityEngine;

public abstract class Power : MonoBehaviour, IAnimatorEventSubscriber
{
    public bool IsChargeable { get; protected set; }
    public bool IsCharging { get; protected set; }
    public bool IsBusy { get; protected set; }

    [Header("Animation")]
    [SerializeField]
    private string m_isIdleParamNameString;

    public int IsIdleParamHashId { get; private set; }

    protected Animator m_armsAnimator;
    protected ArmsEventsManager m_armsEventsManager;

    protected virtual void Awake()
    {
        IsChargeable = true;
        IsCharging = false;
        IsBusy = false;

        IsIdleParamHashId = Animator.StringToHash(m_isIdleParamNameString);
    }

    public void Initialise(Animator armsAnimator, ArmsEventsManager armsEventsManager, bool show)
    {
        m_armsAnimator = armsAnimator;
        m_armsEventsManager = armsEventsManager;
        SubscribeToEvents();
        Show(show);
    }

    protected abstract void SubscribeToEvents();

    public abstract void Show(bool show);

    public virtual bool StartCharging()
    {
        IsCharging = true;
        Debug.Log(Time.frameCount + " Charge");

        return IsCharging;
    }

    public virtual bool StopCharging()
    {
        IsCharging = false;
        Debug.Log(Time.frameCount + " Stop");

        return IsCharging == false;
    }

    public abstract bool Use();

    // Methods of the IAnimatorEventSubscriber interface
    public abstract void NotifyEvent(string eventName);
}
