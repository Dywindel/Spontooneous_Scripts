using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockObj_Editor : MonoBehaviour
{
    //////////
    // DATA //
    //////////

    // For editor scripts, setting Rune loadout
    public void Unpack_RockSettings(RockData rockData)
    {
        RockObj rockObj = this.GetComponent<RockObj>();

        rockObj.isMineable = rockData.isMineable;
        rockObj.rockType = rockData.rockType;
    }
}
