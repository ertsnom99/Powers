using UnityEngine;

public class ObjectPuller : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_pullLayer;

    public float pullRadius = 2;
    public float maxProximity = 1;
    public float pullForce = 1;
    public float tangentForce = 1;
    public float perpendicularNormalForce = 1;
    public float oscillation = 5;

    public bool m_debug = false;

    public void FixedUpdate()
    {
        foreach (Collider collider in Physics.OverlapSphere(transform.position, pullRadius, m_pullLayer))
        {
            // calculate direction from target to me
            Vector3 colliderToCenter = transform.position - collider.transform.position;

            Vector3 pull = colliderToCenter.normalized * pullForce;

            float repulseStrength = 1 - (colliderToCenter.magnitude - maxProximity) / (pullRadius - maxProximity);
            Vector3 repulse = Vector3.Lerp(Vector3.zero, -pull, repulseStrength);
            
            Vector3 pullStartPoint = transform.position - colliderToCenter.normalized * maxProximity;
            Vector3 pullEndPoint = transform.position - colliderToCenter.normalized * (maxProximity + 1);
            Vector3 normal = pullEndPoint - pullStartPoint;
            Vector3 tangent = Vector3.Cross(normal, Vector3.one).normalized;
            Vector3 perpendicularNormal = Quaternion.AngleAxis(Mathf.Sin(Time.time * oscillation) * 90, tangent) * normal;

            // apply force on target towards me
            Rigidbody rigidbody = collider.GetComponent<Rigidbody>();

            rigidbody.AddForce(-rigidbody.velocity, ForceMode.VelocityChange);
            rigidbody.AddTorque(-rigidbody.angularVelocity, ForceMode.VelocityChange);
            rigidbody.useGravity = false;

            rigidbody.AddForce(pull * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rigidbody.AddForce(repulse * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rigidbody.AddForce(tangent * tangentForce * Time.fixedDeltaTime, ForceMode.Force);
            rigidbody.AddForce(perpendicularNormal * perpendicularNormalForce * Time.fixedDeltaTime, ForceMode.VelocityChange);


            //Debug.DrawLine(pullStartPoint, pullEndPoint, Color.red);
            //Debug.DrawLine(pullEndPoint, pullEndPoint + tangent, Color.blue);
            //Debug.DrawLine(pullEndPoint, pullEndPoint + perpendicularNormal, Color.green);

            //Debug.Log(colliderToCenter.magnitude);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (m_debug)
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, pullRadius);
        }
    }
}
