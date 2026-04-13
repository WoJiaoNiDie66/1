using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

// MSB = MyStateBehaviour
public class MSB_RST_SKILL : StateMachineBehaviour
{
    public PlayerMain_A0 _playerMain;
    public int lastInfo;
    public int currentInfo = -1;

    public bool resetFlag = true; // used to reset certain variables in PlayerMain_A0, can be used in anim to trigger certain enemy skill
    public int curNextSkill = 0; // used to reset next skill index in PlayerDecision, can be used in anim to trigger certain enemy skill

    public bool debugMode = false;
    // This function is called when the state is entered
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_playerMain == null)
        {
            _playerMain = animator.GetComponent<PlayerMain_A0>();
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
        if (resetFlag) _playerMain._playerDecision.curNextSkill = curNextSkill; // reset next skill index in PlayerDecision, can be used in anim to trigger certain enemy skill
        // other function
        // vfx
        // sfx
    }
}