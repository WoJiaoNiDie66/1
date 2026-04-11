using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

// ISB = InteractStateBehaviour
public class ISB_Lever2Door : StateMachineBehaviour
{
    public Ptr2Door _ptr2Door; // 直接拖入 DoorController 的腳本，讓它成為一個公開變數
    public int lastInfo;
    public int currentInfo = -1;

    public bool debugMode = false;
    // This function is called when the state is entered
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_ptr2Door == null)
        {
            _ptr2Door = animator.GetComponent<Ptr2Door>();
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
        _ptr2Door.TriggerDoor(); // 直接呼叫 Ptr2Door 的 TriggerDoor 方法，讓它去觸發 DoorController 的開門邏輯
        // other function
        // vfx
        // sfx
    }
}