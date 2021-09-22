using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OI_SelectOptionsGroup : OptionItem
{
    public UnityEvent<OptionGroup> action;
    public OptionGroup selectOptionGroups;

    public override void Input_Action()
    {
        base.Input_Action();
        
        // If the action button is pressed
        // Invoke the new options group
        Invoke_Action(selectOptionGroups);
    }

    public void Invoke_Action(OptionGroup pSelectOptionGroup)
    {
        action.Invoke(pSelectOptionGroup);
    }
}
