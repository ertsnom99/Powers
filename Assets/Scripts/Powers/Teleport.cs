using UnityEngine;

public class Teleport : Power
{
    [Header("Projectile")]
    [SerializeField]
    private GameObject m_teleportProjectile;

    public override void ShowPower(bool show)
    {
        m_teleportProjectile.SetActive(show);
    }
}
