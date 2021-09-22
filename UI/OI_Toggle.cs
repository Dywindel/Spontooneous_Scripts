using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Toggle a tick box

public class OI_Toggle : OptionItem
{
    public UnityEvent action;

    public bool toggle = false;

    public override void Input_Action()
    {
        base.Input_Action();

        // If the action button is pressed
        // Change the toggle boolean
        toggle = !toggle;
        Invoke_Action();
    }

    public override void Input_Direction(bool isPositive)
    {
        base.Input_Direction(isPositive);

        // If the left of right stick is pressed
        // Change the toggle boolean
        toggle = !toggle;
        Invoke_Action();
    }

    public void Invoke_Action()
    {
        action.Invoke();
    }
}
