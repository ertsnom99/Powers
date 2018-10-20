using UnityEngine;

public class Tester : MonoBehaviour
{
    public GravityField m_script;

    private bool rtest = true;

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            rtest = !rtest;
            m_script.EnableGravity(rtest);
        }
    }
}
