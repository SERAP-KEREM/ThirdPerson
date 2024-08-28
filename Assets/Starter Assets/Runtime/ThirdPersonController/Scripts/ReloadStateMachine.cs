using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadStateMachine : StateMachineBehaviour
{
   public override void OnStateExit(Animator animator,AnimatorStateInfo stateInfo,int layerIndex)
    {
        ThirdPersonController controller=animator.gameObject.GetComponent<ThirdPersonController>();

        if(controller != null)
        {
            controller.ReloadFinished();
        }
    }
}
