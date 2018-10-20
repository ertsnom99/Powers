using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    [Header("Pull")]
    [SerializeField]
    private SphereCollider m_area;
    [SerializeField]
    private float m_maxProximity = 2;
    [SerializeField]
    private float m_pullForce = 32000;
    [SerializeField]
    private float m_tangentForce = 20000;

    private Dictionary<Rigidbody, Vector3> m_affectedRigidbody = new Dictionary<Rigidbody, Vector3>();

    [Header("Explosion")]
    [SerializeField]
    private float m_explosionForce = 3000;

    [Header("Debug")]
    [SerializeField]
    private bool m_showInitialVecteursLine = false;
    [SerializeField]
    private bool m_showMovementLine = false;
    
    public void EnableGravity(bool enable)
    {
        m_area.enabled = enable;

        if (!enable)
        {
            foreach (KeyValuePair<Rigidbody, Vector3> entry in m_affectedRigidbody)
            {
                entry.Key.useGravity = true;
            }

            m_affectedRigidbody.Clear();
        }
    }

    public void Explode()
    {
        foreach (KeyValuePair<Rigidbody, Vector3> entry in m_affectedRigidbody)
        {
            Vector3 repulsionDirection = (entry.Key.transform.position - transform.position).normalized;

            RemoveRigidbodyForces(entry.Key);
            entry.Key.useGravity = true;

            entry.Key.AddExplosionForce(m_explosionForce, transform.position, m_area.radius);
        }

        m_affectedRigidbody.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: put this string in a static var
        if (!other.CompareTag("Player"))
        {
            Rigidbody rigidbody = other.GetComponent<Rigidbody>();

            if (rigidbody && !rigidbody.isKinematic && !m_affectedRigidbody.ContainsKey(rigidbody))
            {
                m_affectedRigidbody.Remove(rigidbody);
                rigidbody.useGravity = false;

                Vector3 colliderToCenter = transform.position - other.transform.position;
                Vector3 tangent = Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f), colliderToCenter) * Vector3.Cross(colliderToCenter, Vector3.one);
                Vector3 gravitationAxe = Vector3.Cross(colliderToCenter, tangent).normalized;

                m_affectedRigidbody.Add(rigidbody, gravitationAxe);

                if (m_showInitialVecteursLine)
                {
                    Debug.DrawLine(transform.position, other.transform.position, Color.red, 35);
                    Debug.DrawLine(other.transform.position, other.transform.position + tangent, Color.blue, 35);
                    Debug.DrawLine(transform.position, transform.position + gravitationAxe, Color.green, 35);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // TODO: put this string in a static var
        if (!other.CompareTag("Player"))
        {
            Rigidbody rigidbody = other.GetComponent<Rigidbody>();

            if (rigidbody && !rigidbody.isKinematic && m_affectedRigidbody.ContainsKey(rigidbody))
            {
                rigidbody.useGravity = true;

                m_affectedRigidbody.Remove(rigidbody);
            }
        }
    }

    public void FixedUpdate()
    {
        if (m_area.enabled)
        {
            foreach (KeyValuePair<Rigidbody, Vector3> entry in m_affectedRigidbody)
            {
                Vector3 colliderToCenter = transform.position - entry.Key.transform.position;

                // Calculate pull force
                Vector3 pull = colliderToCenter.normalized * m_pullForce;

                // Calculate repulsif force
                float repulseIntencity = 1 - (colliderToCenter.magnitude - m_maxProximity) / (m_area.radius - m_maxProximity);
                Vector3 repulse = Vector3.Lerp(Vector3.zero, -pull, repulseIntencity);

                // Calculate the tangent
                Vector3 tangent = Vector3.Cross(colliderToCenter, entry.Value).normalized;

                RemoveRigidbodyForces(entry.Key);

                // Pull if enable
                entry.Key.AddForce(pull * Time.fixedDeltaTime, ForceMode.VelocityChange);
                entry.Key.AddForce(repulse * Time.fixedDeltaTime, ForceMode.VelocityChange);

                // Move if enable
                entry.Key.AddForce(tangent * m_tangentForce * Time.fixedDeltaTime, ForceMode.Force);

                if (m_showMovementLine && entry.Key.gameObject.activeSelf)
                {
                    Debug.DrawLine(transform.position, transform.position + entry.Value, Color.red);
                    Debug.DrawLine(entry.Key.transform.position, entry.Key.transform.position + tangent, Color.blue);
                }
            }
        }
    }

    private void RemoveRigidbodyForces(Rigidbody rigidbody)
    {
        // Cancel any accumulated force
        rigidbody.AddForce(-rigidbody.velocity, ForceMode.VelocityChange);
        rigidbody.AddTorque(-rigidbody.angularVelocity, ForceMode.VelocityChange);
    }
}
