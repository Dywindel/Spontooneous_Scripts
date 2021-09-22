using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// When the player walks into the trigger, it sets up all necessary items
// For what this area needs
public class Sc_Act_Area : MonoBehaviour
{
    public AreaSettingsSO areaSettingsSO;

    void OnTriggerEnter(Collider other)
    {
        // Check if player
        if (other.gameObject.tag == "Player")
        {
            // Update the area sound
            AC.Instance.Activate_AreaSound(areaSettingsSO);

            // Update the area lighting
            LD.Instance.Activate_AreaLighting(areaSettingsSO);
        }
    }
}
