using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This is a list of Option Items that the user
// Can scan through

public class OptionGroup : MonoBehaviour
{
    public UnityEvent action_Back;
    public OptionItem[] arrayOf_OptionItems;
    public bool isMenuVertical;     // For vertical and horizontal menus
    public bool isCyclic = false;   // Stops menus from jumping from bottom to top each time
    public int index;

    public void Input_Back()
    {
        action_Back.Invoke();
    }

    // Move the selected item in a positive or negative direction
    public void Update_ItemSelect(bool isPositive)
    {
        

        // Deselect the previous index
        Update_Animator(index, false);

        if (isPositive)
        {
            if (!isCyclic && index == (arrayOf_OptionItems.Length - 1))
            {
                // Do nothing
            }
            else
            {
                // SFX
                AC.Instance.OneShot_UI_Keys();

                index = (index + 1) % arrayOf_OptionItems.Length;
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
                // SFX
                AC.Instance.OneShot_UI_Keys();

                index = (arrayOf_OptionItems.Length + index - 1) % arrayOf_OptionItems.Length;
            }
        }

        // We select the new index
        Update_Animator(index, true);
    }

    // Activate the selected index (For mouse)
    public void Update_ItemSelect(int pIndex)
    {
        if (index != pIndex)
        {
            // SFX
            AC.Instance.OneShot_UI_Keys();
        }

        // Deselect the previous index
        Update_Animator(index, false);

        if (pIndex >= 0 || pIndex < arrayOf_OptionItems.Length)
        {
            index = pIndex;
        }

        // We select the new index
        Update_Animator(index, true);
    }

    public void Update_Animator(int pIndex, bool isHighlighted)
    {
        arrayOf_OptionItems[pIndex].Update_Animator(isHighlighted);
    }
}
