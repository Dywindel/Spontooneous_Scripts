using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OI_Carousel_Resolution : OI_Carousel
{
    Resolution[] listof_Resolutions;

    // Start is called before the first frame update
    void Start()
    {
        // In this version of the script, the items in the carousel
        // Are collect from the player's screen settings
        Collect_Resolutions();

        // Set the index based on the player prefs
        if (IGD.Instance.inGameDataPrefs != null)
        {
            // If this is the first time we're using this index value, set it to the max values
            if (IGD.Instance.inGameDataPrefs.options[1] == -1)
            {
                // Stay with the current resolution
            }
            else
            {
                index = IGD.Instance.inGameDataPrefs.options[1];
            }
        }

        Update_Visuals(index);
        Invoke_Action(index);
    }

    void Collect_Resolutions()
    {
        // Grab all the resolutions picked up from the screen
        listof_Resolutions = Screen.resolutions;
        arrayOf_Options = new string[listof_Resolutions.Length];

        for (int i = 0; i < listof_Resolutions.Length; i++)
        {
            string option = "???";
            //Send each resolution as a string into the carousel items array
            if (i == 0)
            {
                option = listof_Resolutions[i].width + " x " + listof_Resolutions[i].height 
                                                    + " : " + listof_Resolutions[i].refreshRate + " >";
            }
            else if (i == listof_Resolutions.Length - 1)
            {
                option = "< " + listof_Resolutions[i].width + " x " + listof_Resolutions[i].height 
                                                    + " : " + listof_Resolutions[i].refreshRate;
            }
            else
            {
                option = "< " + listof_Resolutions[i].width + " x " + listof_Resolutions[i].height 
                                                    + " : " + listof_Resolutions[i].refreshRate + " >";
            }

            arrayOf_Options[i] = option;

            // If our current screen resolution is equal to one of these values
            // We set that as the starting value for the game
            // Unity won't let us compare resolutions. We have to compare w&h
            if (listof_Resolutions[i].width == Screen.currentResolution.width
                && listof_Resolutions[i].height == Screen.currentResolution.height)
            {
                index = i;
            }
        }
    }

    // The action needs to go here as I need access to certain values
    public void Action_Set_Resolution()
    {
        Screen.SetResolution(listof_Resolutions[index].width, listof_Resolutions[index].height, Screen.fullScreen);

        // Store in prefs
        IGD.Instance.inGameDataPrefs.options[1] = index;
    }
}
