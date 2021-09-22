using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is only data, not actually movement takes place
// This command is used to move the player in all six cardinal directions

public class MoveCommand : ACommand
{
    public RiftObj riftEntering;
    public RiftObj riftLeaving;

    public MoveCommand(Vector3 pPrv_Pos, DirType pMoveCard, DirType pFaceCard, RiftObj pRiftEnter, RiftObj pRiftLeave)
    {
        // Store the player's previous position
        storePos = RL_F.V3_F3(pPrv_Pos);

        if (pMoveCard == DirType.N || pMoveCard == DirType.E || pMoveCard == DirType.S || pMoveCard == DirType.W)
        {
            actionKit = new ActionKit(MoveType.Step);
        }
        else if (pMoveCard == DirType.U || pMoveCard == DirType.D)
        {
            actionKit = new ActionKit(MoveType.Climb);
        }
        moveCard = pMoveCard;
        faceCard = pFaceCard;
        // Store if the player left or entered a rift during this move
        riftEntering = pRiftEnter;
        riftLeaving = pRiftLeave;
    }

    public override void Undo()
    {
        // Reposition the player colider using the previous position value
        Sc_Player.Instance.Update_PlayerPos_Instant(RL_F.F3_V3(storePos), faceCard);
    
        // Activate or deactivate the rift
        // If the player Entered a rift on the last move
        // When undoing this means they are 'Leaving' a rift
        if (riftEntering != null)
        {
            PD.Instance.Toggle_Active_Rift(null);
        }
        // If the player was leaving a rift on the last move
        // When undoing, this means they are 'Entering' that same rift
        if (riftLeaving != null)
        {
            PD.Instance.Toggle_Active_Rift(riftLeaving);
        }
    }
}
