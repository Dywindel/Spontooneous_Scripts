using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// A carousel selection

public class OI_Carousel : OptionItem
{
    public UnityEvent<int> action;

    public int index;
    public string[] arrayOf_Options;
    [Tooltip ("When reaching the extreme index value, it will jump to the other extreme")]
    public bool isCyclic = false;
    
    public override void Input_Direction(bool isPositive)
    {
        base.Input_Direction(isPositive);
        
        if (isPositive)
        {
            if (!isCyclic && index == (arrayOf_Options.Length - 1))
            {
                // Do nothing
            }
            else
            {
                index = (index + 1) % arrayOf_Options.Length;
            }
        }
        else
        {
            if (!isCyclic && index == 0)
            {
                // Do nothing
            }
            else
            {
                index = (arrayOf_Options.Length + index - 1) % arrayOf_Options.Length;
            }
        }

        Update_Visuals(index);
        Invoke_Action(index);
    }

    // Update visuals
    public void Update_Visuals(int index)
    {
        tMPro.text = arrayOf_Options[index];
    }

    public void Invoke_Action(int val)
    {
        action.Invoke(val);
    }
}
