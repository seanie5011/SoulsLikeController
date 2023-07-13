using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBool : StateMachineBehaviour
{
    // used to decide what parameter to set and what value to give it
    public string isInteractingBool;
    public bool isInteractingStatus;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    // when finished animation and want to go back to the base layer
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // so will set whatever is the isInteractingBool (so "isInteracting") to
        // the isInteractingStatus (so false)
        animator.SetBool(isInteractingBool, isInteractingStatus);
    }
}
