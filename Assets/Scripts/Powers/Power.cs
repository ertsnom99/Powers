using UnityEngine;

public abstract class Power : MonoBehaviour
{
    [SerializeField]
    private string m_isIdleParamNameString;

    public int IsIdleParamHashId { get; private set; }
    
    private void Awake()
    {
        IsIdleParamHashId = Animator.StringToHash(m_isIdleParamNameString);
    }
}
