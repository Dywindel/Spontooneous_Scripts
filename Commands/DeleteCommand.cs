using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player can use this command to remove a piece from the board

public class DeleteCommand : ACommand
{
    public RiftObj riftObj;
    public BridgeData prv_BridgeData;   // Store anything we deleted
    
    public DeleteCommand(DirType pMoveCard, RiftObj pRiftObj, BridgeData pPrv_BridgeData)
    {
        if (pMoveCard == DirType.N || pMoveCard == DirType.E || pMoveCard == DirType.S || pMoveCard == DirType.W)
        {
            actionKit = new ActionKit(MoveType.Step, MetaType.Delete);
        }
        else if (pMoveCard == DirType.U || pMoveCard == DirType.D)
        {
            actionKit = new ActionKit(MoveType.Climb, MetaType.Delete);
        }
        riftObj = pRiftObj;
        prv_BridgeData = pPrv_BridgeData;
    }

    public override void Undo()
    {
        // Replace anything we original deleted
        if (prv_BridgeData.bridgeType != BridgeType.None)
        {
            // We can place that piece back into the scene
            riftObj.Place_GenObj(prv_BridgeData.pos, prv_BridgeData.bridgeType, prv_BridgeData.plankDir);
        }
    }
}
