using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

// ISB = InteractStateBehaviour
public class ISB_REACT : StateMachineBehaviour
{
    public SB2EventNode _eventNode;
    public int lastInfo;
    public int currentInfo = -1;

    public bool debugMode = false;
    // This function is called when the state is entered
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_eventNode == null)
        {
            _eventNode = animator.GetComponentInChildren<SB2EventNode>();
        }
        lastInfo = currentInfo;
        currentInfo = stateInfo.shortNameHash;
        // Call the custom function when entering this state
        CustomFunction(animator, stateInfo, layerIndex);
    }

    private void CustomFunction(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement your custom logic here
        if (debugMode) Debug.Log("Entered State: " + stateInfo.shortNameHash);
        // You can access other components or the Animation state here
        // Example: animator.gameObject.GetComponent<MyComponent>().DoSomething();

        /*
            order from anim:

            clear anim flag
            control passive effect
            clear get hit trigger
            clear input
            set invincible
            move & look (sometimes in update) (include jump)
            hitbox
            value editing
            call other functions

            vfx
            sfx
        */
        _eventNode.TriggerEvent();
        // other function
        // vfx
        // sfx
    }
}
