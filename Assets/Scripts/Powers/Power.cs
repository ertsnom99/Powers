using UnityEngine;

public abstract class Power : MonoBehaviour
{
    public bool IsChargeable { get; protected set; }
    public bool IsCharging { get; protected set; }

    [Header("Animation")]
    [SerializeField]
    private string m_isIdleParamNameString;

    public int IsIdleParamHashId { get; private set; }

    protected virtual void Awake()
    {
        IsChargeable = true;
        IsCharging = false;

        IsIdleParamHashId = Animator.StringToHash(m_isIdleParamNameString);
    }

    public virtual void StartCharging()
    {
        IsCharging = true;

    }

    public virtual void StopCharging()
    {
        IsCharging = false;

    }

    public virtual void Use()
    {

    }

    public abstract void ShowPower(bool show);
}
