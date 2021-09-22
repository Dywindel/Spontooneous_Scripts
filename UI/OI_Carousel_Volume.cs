using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OI_Carousel_Volume : OI_Carousel
{
    FMOD.Studio.Bus audioBus;

    public enum BusType {MUS, SFX, AMB};
    public BusType busType;
    string busName = "bus:/Master/MUS";

    void Awake()
    {
        if (busType == BusType.MUS)
        {
            busName = "bus:/Master/MUS";
        }
        else if (busType == BusType.SFX)
        {
            busName = "bus:/Master/SFX";
        }
        else if (busType == BusType.AMB)
        {
            busName = "bus:/Master/AMB";
        }

        audioBus = FMODUnity.RuntimeManager.GetBus(busName);
    }

    void Start()
    {
        // Generate the array of options
        Generate_VolumeOptions();

        // Grab the current volume of the master
        index = Action_Get_Volume();

        // Check the IGD if the player has saved settings
        if (IGD.Instance.inGameDataPrefs != null)
        {
            if (busType == BusType.MUS)
            {
                index = IGD.Instance.inGameDataPrefs.options[2];
            }
            else if (busType == BusType.SFX)
            {
                index = IGD.Instance.inGameDataPrefs.options[3];
            }
            else if (busType == BusType.AMB)
            {
                index = IGD.Instance.inGameDataPrefs.options[4];
            }
        }

        Action_Set_Volume();
        Update_Visuals(index);
    }

    void Generate_VolumeOptions()
    {
        arrayOf_Options = new string[11];

        for (int i = 0; i < 11; i++)
        {
            if (i == 0)
            {
                arrayOf_Options[i] = "0 >";
            }
            else if (i == 10)
            {
                arrayOf_Options[i] = "< 10";
            }
            else
            {
                arrayOf_Options[i] = "< " + i + " >";
            }
        }
    }

    public void Action_Set_Volume()
    {
        float newVolume = (float)index/10.0f;

        // Set volume function
        audioBus.setVolume(newVolume);

        // Store this data in the playerPrefs
        if (busType == BusType.MUS)
        {
            IGD.Instance.inGameDataPrefs.options[2] = index;
        }
        else if (busType == BusType.SFX)
        {
            IGD.Instance.inGameDataPrefs.options[3] = index;
        }
        else if (busType == BusType.AMB)
        {
            IGD.Instance.inGameDataPrefs.options[4] = index;
        }
    }

    // Reverse get the volume index value from the Audio Mixer
    int Action_Get_Volume()
    {
        float getVolume = 0f;
        // Get volume function
        audioBus.getVolume(out getVolume);

        int newVolumeIndex = (int)Mathf.Round(10f*getVolume);

        return newVolumeIndex;
    }
}
