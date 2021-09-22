using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For placing a staircase

public class StairsCommand : ACommand
{
    public int[] bridgeData_ArrayPos;
    public RiftObj riftObj;
    public List<BridgeData> listOf_Prv_BridgeData = new List<BridgeData>();    // There are only 2, where we moved from[0] and where we're moving to[1]

    public StairsCommand(DirType pMoveCard, int[] pBridgeData_ArrayPos, RiftObj pRiftObj, List<BridgeData> pListOf_Prv_BridgeData)
    {
        actionKit = new ActionKit(BuildType.Stairs);
        moveCard = pMoveCard;
        bridgeData_ArrayPos = pBridgeData_ArrayPos; 
        riftObj = pRiftObj;
        listOf_Prv_BridgeData = pListOf_Prv_BridgeData;
    }

    public override void Undo()
    {
        // Return the stairs that were placed
        riftObj.Return_GenObj(bridgeData_ArrayPos);

        // If we previously moved up, check if there was a bridge already here
        if (moveCard == DirType.U)
        {
            // If the place we were moving to was not none
            if (listOf_Prv_BridgeData[1].bridgeType != BridgeType.None)
            {
                // We need to put that item back
                riftObj.Place_GenObj(listOf_Prv_BridgeData[1].pos, listOf_Prv_BridgeData[1].bridgeType);
            }
        }
        // If we previously moved down
        else if (moveCard == DirType.D)
        {
            // If the place we were moving to was not none
            if (listOf_Prv_BridgeData[1].bridgeType != BridgeType.None)
            {
                // We need to put that bridge back
                // However, there is already a bridge here!
                // Technically we return a bridge then create the old bridge (But I think this is always the same thing)
                riftObj.Return_GenObj(listOf_Prv_BridgeData[1].pos);
                riftObj.Place_GenObj(listOf_Prv_BridgeData[1].pos, listOf_Prv_BridgeData[1].bridgeType);
            }
            else
            {
                // Using the pos reference.
                // Check if there is a bridge that isn't none in the previous position
                if (riftObj.arrayOf_BridgeData[listOf_Prv_BridgeData[1].pos[0], listOf_Prv_BridgeData[1].pos[1], listOf_Prv_BridgeData[1].pos[2]].bridgeType != BridgeType.None)
                {
                    // We can return that bridge item
                    // Which is index[1]
                    riftObj.Return_GenObj(listOf_Prv_BridgeData[1].pos);
                }
                
            }
            // If the place were were moving from had a bridge, we'll also need to put that back
            if (listOf_Prv_BridgeData[0].bridgeType != BridgeType.None)
            {
                // We need to put that bridge back
                riftObj.Place_GenObj(listOf_Prv_BridgeData[0].pos, listOf_Prv_BridgeData[0].bridgeType);
            }
        }

        // If we've return or placed a stairs, you need to update the trellis graph and then update meshes at those points
        if (riftObj != null)
        {
            // Update trellis space
            riftObj.Update_TrellisSpace();

            riftObj.Update_SurroundingMeshes(bridgeData_ArrayPos);
        }
    }
}
