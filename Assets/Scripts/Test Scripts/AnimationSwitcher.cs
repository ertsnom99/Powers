using UnityEngine;

public class AnimationSwitcher : MonoBehaviour
{
    private Animator m_animator;
    
    protected int m_isHoldingLeftSphereParamHashId = Animator.StringToHash(IsHoldingLeftSphereParamNameString);
    protected int m_isHoldingRightSphereParamHashId = Animator.StringToHash(IsHoldingRightSphereParamNameString);
    
    public const string IsHoldingLeftSphereParamNameString = "IsHoldingLeftSphere";
    public const string IsHoldingRightSphereParamNameString = "IsHoldingRightSphere";

    private void Awake()
    {
        m_animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            m_animator.SetBool(m_isHoldingLeftSphereParamHashId, true);
        }
        else if(Input.GetKeyDown("a"))
        {
            m_animator.SetBool(m_isHoldingLeftSphereParamHashId, false);
        }

        if (Input.GetKeyDown("w"))
        {
            m_animator.SetBool(m_isHoldingRightSphereParamHashId, true);
        }
        else if (Input.GetKeyDown("s"))
        {
            m_animator.SetBool(m_isHoldingRightSphereParamHashId, false);
        }
    }
}
