using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlankCommand : ACommand
{
    public int[] bridgeData_ArrayPos;
    public RiftObj riftObj;

    public BridgeData prv_BridgeData;

    public PlankCommand(int[] pBridgeData_ArrayPos, RiftObj pRiftObj, BridgeData pPrv_BridgeData)
    {
        actionKit = new ActionKit(BuildType.Plank);
        bridgeData_ArrayPos = pBridgeData_ArrayPos;
        riftObj = pRiftObj;
        prv_BridgeData = pPrv_BridgeData;
    }

    public override void Undo()
    {
        // Use the rift return bridge command
        riftObj.Return_GenObj(bridgeData_ArrayPos);

        // Replace anything we original overwrote
        if (prv_BridgeData.bridgeType != BridgeType.None)
        {
            // We can place that piece back into the scene
            riftObj.Place_GenObj(prv_BridgeData.pos, prv_BridgeData.bridgeType, prv_BridgeData.plankDir);
        }
    }
}
