using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This command is used to reset a rift puzzle
// It's quite substantial in what data is stored
public class ResetCommand : ACommand
{
    public List<BridgeData> listOf_Prv_BridgeData;

    // Player position
    public Vector3 prv_PlayerPos;
    public RiftObj riftObj;

    public ResetCommand(List<BridgeData> p_ListOf_Prv_BridgeData, Vector3 pPrv_PlayerPos, RiftObj pRiftObj)
    {
        actionKit = new ActionKit(MoveType.None, MetaType.Reset);
        listOf_Prv_BridgeData = p_ListOf_Prv_BridgeData;
        prv_PlayerPos = pPrv_PlayerPos;
        riftObj = pRiftObj;
    }

    public override void Undo()
    {
        // Update player position
        Sc_Player.Instance.Update_PlayerPos_Instant(prv_PlayerPos, DirType.N);

        // Re-spawn the bridge pieces as described in the prv_BridgeData list
        foreach (BridgeData prv_BridgeData in listOf_Prv_BridgeData)
        {
            riftObj.Place_GenObj(prv_BridgeData.pos, prv_BridgeData.bridgeType);
        }

        // Visuals
        // Update the trellis graph
        riftObj.Update_TrellisSpace();
        // Update all the newly placed meshes
        foreach (BridgeData prv_BridgeData in listOf_Prv_BridgeData)
        {
            // Grab all the new bridgeObjs that were just created using the positions of the old bridgeData
            BridgeObj tBridgeObj = riftObj.arrayOf_BridgeData[prv_BridgeData.pos[0], prv_BridgeData.pos[1], prv_BridgeData.pos[2]].bridgeObj;
            // Update their meshes
            tBridgeObj.GetComponent<BridgeObj_Mesh>().UpdateMesh(0);
        }
    }
}
