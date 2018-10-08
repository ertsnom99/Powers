using UnityEngine;

public class Teleport : Power
{
    [Header("Projectile")]
    [SerializeField]
    private GameObject m_teleportProjectile;

    protected override void Awake()
    {
        base.Awake();

        IsChargeable = false;
    }

    public override void ShowPower(bool show)
    {
        m_teleportProjectile.SetActive(show);
    }
}
