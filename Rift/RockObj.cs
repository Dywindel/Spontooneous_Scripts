using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to help find rocks inside the game area

public class RockObj : MonoBehaviour
{
    ///////////////////
    // USER SETTINGS //
    ///////////////////

    public RockData rockData;

    // Array position in the special rock array (Which is shift down in layer by 1 value)
    // We denote this with a 'RockSpace' qualifier
    public int[] arrPos_RS;

    // Store whether the rock is mineable
    public bool isMineable;

    // Stores whether the rock is currently whole or empty
    public RockType rockType;

    public void PassStart(RiftObj pParent)
    {
        // Find this Rune's position relative to the Rift parent
        // How to take into account possible -1 value when rock is below the rigt start point?
        // Add a unity of Vector3.up
        // Now the rock at {-1, 0, 0} will be stored as {0, 0, 0} inside the rockData array
        arrPos_RS = RL_F.V3DFV3_IA(transform.position + (Vector3.up*RL_V.vertUnit), pParent.transform.position);

        // Update self runeData
        rockData = new RockData(arrPos_RS, this);
    }

    ///////////////////
    // VISUALISATION //
    ///////////////////

    // Update how the rock appears depending on its state
    public void Update_RockMesh()
    {
        // How do I get the rockData?
    }
}
