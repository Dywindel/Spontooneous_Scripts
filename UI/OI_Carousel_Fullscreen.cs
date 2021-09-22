using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OI_Carousel_Fullscreen : OI_Carousel
{
    public void Start()
    {
        // Set the array of options
        arrayOf_Options = new string[2] { "No >", "< Yes"};

        // Grab the info from player prefs, if it exists
        if (IGD.Instance.inGameDataPrefs != null)
        {
            index = IGD.Instance.inGameDataPrefs.options[0];
        }
        else
        {
            // Check the current screen resolution and update the status of the menu
            if (!Screen.fullScreen)
            {
                index = 0;
            }
            else
            {
                index = 1;
            }
        }

        Update_Visuals(index);
        Invoke_Action(index);
    }

    // The action needs to go here as I need access to certain values
    public void Action_Set_Fullscreen()
    {
        if (index == 0)
        {
            Screen.fullScreen = false;
        }
        else
        {
            Screen.fullScreen = true;
        }

        // Store in prefs
        IGD.Instance.inGameDataPrefs.options[0] = index;
    }
}
