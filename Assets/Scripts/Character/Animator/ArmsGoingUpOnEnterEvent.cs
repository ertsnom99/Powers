using UnityEngine;

public class ArmsGoingUpOnEnterEvent : StateMachineBehaviour
{
    [Header("Arm side")]
    [SerializeField]
    private ArmSide m_armSide;
    
    private string m_showPowerEvent;

    private void Awake()
    {
        switch (m_armSide)
        {
            case ArmSide.Left:
                m_showPowerEvent = ArmsEventsManager.LEFT_ARM_SHOW_POWER_EVENT;
                break;
            case ArmSide.Right:
                m_showPowerEvent = ArmsEventsManager.RIGHT_ARM_SHOW_POWER_EVENT;
                break;
        }
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AnimatorEventsManager animatorEventsManager = animator.GetComponent<AnimatorEventsManager>();

        animatorEventsManager.SendEvent(m_showPowerEvent);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
    //
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
