using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Perform an action when interacted with

public class OI_Button : OptionItem
{
    public UnityEvent action;

    public override void Input_Action()
    {
        base.Input_Action();
        
        // Just perform the action
        Invoke_Action();
    }

    public void Invoke_Action()
    {
        action.Invoke();
    }
}
