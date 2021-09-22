using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeCommand : ACommand
{
    public int[] bridgeData_ArrayPos;
    public RiftObj riftObj;
    public BridgeData prv_BridgeData;   // In case we overwrite anything that was here before

    public BridgeCommand(int[] pBridgeData_ArrayPos, RiftObj pRiftObj, BridgeData pPrv_BridgeData)
    {
        actionKit = new ActionKit(BuildType.Bridge);
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
            riftObj.arrayOf_BridgeData[prv_BridgeData.pos[0], prv_BridgeData.pos[1], prv_BridgeData.pos[2]].bridgeObj.GetComponent<BridgeObj_Mesh>().UpdateMesh(0);
        }

        // If we've return or placed a bridge, you need to update the trellis graph and then update meshes at those points
        if (riftObj != null)
        {
            // Update trellis space
            riftObj.Update_TrellisSpace();

            riftObj.Update_SurroundingMeshes(bridgeData_ArrayPos);
        }

        // We can also double check the puzzle solve state
        PD.Instance.Check_IfRift_Solved(riftObj);
    }
}
