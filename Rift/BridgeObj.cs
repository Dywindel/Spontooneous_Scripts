using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bridge object that can be built inside the puzzle area

public class BridgeObj : MonoBehaviour
{
    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    [HideInInspector]
    public GM gM;
    [HideInInspector]
    public PD pD;
    [HideInInspector]
    public AC aC;

    /////////////////////
    // SELF REFERENCES //
    /////////////////////

    private BridgeObj_Mesh bridgeObj_Mesh;

    ///////////////////
    // USER SETTINGS //
    ///////////////////
    
    public BridgeData bridgeData;

    public BridgeType bridgeType = BridgeType.None;
    public DolleyType dolleyType = DolleyType.A;
    public StairType stairType = StairType.Lower;

    public void PassStart()
    {
        bridgeObj_Mesh = GetComponent<BridgeObj_Mesh>();

        // Setting up the bridgeData class for it's specific type

        // Changing the mess using BridgeObj_Mesh to get the right
        // Art dependent on it's surrounding bridges
    }
}
