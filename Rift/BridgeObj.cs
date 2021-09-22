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
    [HideInInspector] public RiftObj parent;

    /////////////////////
    // SELF REFERENCES //
    /////////////////////

    private BridgeObj_Mesh bridgeObj_Mesh;
 
    ///////////////////
    // USER SETTINGS //
    ///////////////////

    public BridgeType bridgeType = BridgeType.None;
    public PlankDir plankDir = PlankDir.Hor;
}
