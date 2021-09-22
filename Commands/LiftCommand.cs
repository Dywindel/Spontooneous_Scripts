using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores if the player has stepped onto or off of a moving platform

public class LiftCommand : ACommand
{
    // Store the relative transform
    public LiftData relative_LiftData_Leaving;
    public LiftData relative_LiftData_Entering;

    public LiftCommand( Vector3 pPlayerRelativePosition_Leaving, Vector3 pPlayer_RelativePosition_Entering,
                        Transform pRelativeTransform_Leaving = null, Transform pRelativeTransform_Entering = null)
    {
        Sc_Player player = Sc_Player.Instance;

        relative_LiftData_Leaving = new LiftData(pRelativeTransform_Leaving, pPlayerRelativePosition_Leaving);
        relative_LiftData_Entering = new LiftData(pRelativeTransform_Entering, pPlayer_RelativePosition_Entering);
    }

    public override void Undo()
    {
        Sc_Player player = Sc_Player.Instance;

        // A LiftCommand is only stored if the player enters or leaves a relativeTransform

        // If we previously entered a relative transform, we need to rejoin the worldspace
        if (relative_LiftData_Entering.relativeTransform != null)
        {
            player.player_Relative.parent = player.transform;
            // Then, set the player_Relative to its previously stored position in the world
            // The value we used here will be in worldspace as this is how we stored it
            player.player_Relative.localPosition = relative_LiftData_Leaving.player_RelativePosition;

            // Safety check, round up values
            player.player_Relative.localPosition = RL_F.Round_V3(player.player_Relative.localPosition);
        }
        // If we previously left a relative transform, we need to rejoin that relative transform
        if (relative_LiftData_Leaving.relativeTransform != null)
        {
            // First, set the parent to the transform parent
            player.player_Relative.parent = relative_LiftData_Leaving.relativeTransform;
            // Then, set the player_Relative position to its previously stored relative position
            // The value we're using here will be relative to the transform as this is how we stored it
            player.player_Relative.localPosition = relative_LiftData_Leaving.player_RelativePosition;
            
            // Safety check, round up values
            player.player_Relative.localPosition = RL_F.Round_V3(player.player_Relative.localPosition);
        }
    }
}
