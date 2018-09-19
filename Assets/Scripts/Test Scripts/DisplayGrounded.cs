using UnityEngine;
using UnityEngine.UI;

public class DisplayGrounded : MonoBehaviour
{
    [SerializeField]
    private RBCharacterMovement m_characterMovement;
    [SerializeField]
    private Text m_text;

    private void LateUpdate()
    {
        m_text.text = m_characterMovement.IsGrounded ? "Grounded" : "Nope";
    }
}
