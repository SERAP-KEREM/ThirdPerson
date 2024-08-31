using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolsterStateMachine : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Character controller = animator.gameObject.GetComponent<Character>();

        if (controller != null)
        {
            controller.HolsterFinished();
        }
    }
}
