using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script stores the check_MoveType function

public static class C_MT
{
    public static RiftObj rift_Entering = null;
    public static RiftObj rift_Leaving = null;

    // This is use to allow the player to move on moving objects.
    public static Transform active_RelativeTransform = null;
    public static Transform relativeTransform_Entering = null;
    public static Transform relativeTransform_Leaving = null;

    // When the player moves, we need to store a reference to their position and grid coordinates inside a rift
    public static Vector3 dest_AsVect_WorldSpace = Vector3.zero;
    public static Vector3 dest_AsVect_RelativeSpace = Vector3.zero;
    // Player's grid position inside a rift puzzle
    // [0] = layer, [1] = x, [2] = y
    public static int[] dest_AsIntArr_RiftSpace = new int[3] {0, 0, 0};

    public static ActionKit _C_MT(DirType moveCard)
    {
        // We will return this actionKit at the end of the check
        ActionKit actionKit = new ActionKit();

        // Grab any necessary varialbes
        GM gM = GM.Instance;
        PD pD = PD.Instance;
        Sc_SortInput sI = gM.GetComponent<Sc_SortInput>();

        // These components need to be reset here
        rift_Entering = null;
        rift_Leaving = null;
        relativeTransform_Entering = null;
        relativeTransform_Leaving = null;

        # region Perform Raycasts

        // I've set this up such that a raycast only deals with the first item it his with its mask
        // This may cause a problem later on, but it works for this setup for now

        // Prepare raycasting 
        Ray ray_Forward = new Ray(dest_AsVect_WorldSpace - RL.dir_To_Vect[moveCard], RL.dir_To_Vect[moveCard]); // Where our destination is
        Ray ray_Behind = new Ray(dest_AsVect_WorldSpace, -RL.dir_To_Vect[moveCard]); // Where we were a second ago, what we were standing on
        Ray ray_DownFront = new Ray(dest_AsVect_WorldSpace, RL.dir_To_Vect[DirType.D]);  // For rocks and natural floors
        Ray ray_DownBehind = new Ray(dest_AsVect_WorldSpace - RL.dir_To_Vect[moveCard], RL.dir_To_Vect[DirType.D]); // For rocks and natural floors
        Ray ray_Top = new Ray(dest_AsVect_WorldSpace - RL_V.vertUnit*RL.dir_To_Vect[moveCard], RL.dir_To_Vect[DirType.U]);
        Ray ray_Middle = new Ray(dest_AsVect_WorldSpace - RL_V.vertUnit*RL.dir_To_Vect[moveCard] + RL_V.vertUnit*RL.dir_To_Vect[DirType.D], RL.dir_To_Vect[DirType.U]);
        Ray ray_Bottom = new Ray(dest_AsVect_WorldSpace - RL_V.vertUnit*RL.dir_To_Vect[moveCard], RL.dir_To_Vect[DirType.D]);
        Ray ray_Bottom_Lower = new Ray(dest_AsVect_WorldSpace - RL_V.vertUnit*RL.dir_To_Vect[moveCard] + RL.dir_To_Vect[DirType.D], RL.dir_To_Vect[DirType.D]);
        Ray ray_Dir = new Ray(dest_AsVect_WorldSpace - RL.dir_To_Unit[moveCard]*RL.dir_To_Vect[moveCard], RL.dir_To_Vect[moveCard]);    // This is used for chacking rockObj

        LayerMask maskObstacle = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Obstacle));
        LayerMask maskFloor = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Rock), LayerMask.LayerToName((int)CollLayers.Bridge), LayerMask.LayerToName((int)CollLayers.Obstacle));
        LayerMask maskBridge = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Bridge));
        LayerMask maskRift = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Rift));
        LayerMask maskRock = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Rock));
        LayerMask maskObstacleRock = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Obstacle), LayerMask.LayerToName((int)CollLayers.Rock));
        LayerMask maskLift = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Lift));

        GameObject hitPrv = null;
        GameObject hitObstacle = null;
        GameObject hitNxt = null;
        GameObject hitRift_Leaving = null;
        GameObject hitRift_Entering = null;
        GameObject hitTop = null;
        GameObject hitMiddle = null;
        GameObject hitBottom = null;
        GameObject hitBottom_Lower = null;
        GameObject hitRock = null;

        BridgeObj tBridgeObj_Prv = null;    // If we were standing on a bridgeObj
        BridgeObj tBridgeObj_Nxt = null;    // If we are about to move onto a bridgeObj
        BridgeObj tBridgeObj_Top = null;
        BridgeObj tBridgeObj_Middle = null;
        BridgeObj tBridgeObj_Bottom = null;

        RockObj tRockObj = null;

        // Check for obstacles
        RaycastHit[] temp_HitRock = Physics.RaycastAll(ray_Forward, RL_V.cardUnit, maskObstacle);
        if (temp_HitRock.Length > 0)
        {
            hitObstacle = temp_HitRock[0].transform.gameObject;
        }

        // Check if there is an obstacle floor behind us
        RaycastHit[] temp_HitCurrent = Physics.RaycastAll(ray_DownBehind, RL_V.vertUnit, maskObstacle);
        if (temp_HitCurrent.Length > 0)
        {
            hitPrv = temp_HitCurrent[0].transform.gameObject;
        }

        // Check if there is an obastacle floor ahead of us
        RaycastHit[] temp_HitFloor = Physics.RaycastAll(ray_DownFront, RL_V.vertUnit, maskObstacleRock);
        if (temp_HitFloor.Length > 0)
        {
            hitNxt = temp_HitFloor[0].transform.gameObject;
        }

        // Check what we were previously/currently standing on
        temp_HitCurrent = Physics.RaycastAll(ray_Behind, RL_V.cardUnit, maskBridge);
        if (temp_HitCurrent.Length > 0)
        {
            hitPrv = temp_HitCurrent[0].transform.gameObject;
            tBridgeObj_Prv = hitPrv.GetComponent<BridgeObj>();
        }

        // Check what we're going to be standing on next
        temp_HitFloor = Physics.RaycastAll(ray_Forward, RL_V.cardUnit, maskBridge);
        if (temp_HitFloor.Length > 0)
        {
            hitNxt = temp_HitFloor[0].transform.gameObject;
            tBridgeObj_Nxt = hitNxt.GetComponent<BridgeObj>();
        }

        // Check if leaving a rift, before checking if entering a rift
        RaycastHit[] temp_HitRift_Leaving = Physics.RaycastAll(ray_Behind, RL.dir_To_Unit[moveCard]*RL_V.cardUnit, maskRift);
        if (temp_HitRift_Leaving.Length > 0)
        {
            hitRift_Leaving = temp_HitRift_Leaving[0].transform.gameObject;
        }
        if (hitRift_Leaving != null)
        {
            // We are leaving a rift
            rift_Leaving = hitRift_Leaving.transform.parent.GetComponent<RiftObj>();
            // We could set the intArray coordinates to all zero, but it doesn't technically matter
            dest_AsIntArr_RiftSpace = new int[3] {0, 0, 0};
        }
        // Check if entering a rift
        RaycastHit[] temp_HitRift_Entering = Physics.RaycastAll(ray_Forward, RL_V.cardUnit, maskRift);
        if (temp_HitRift_Entering.Length > 0)
        {
            hitRift_Entering = temp_HitRift_Entering[0].transform.gameObject;
        }
        if (hitRift_Entering != null)
        {
            // We are entering a rift
            rift_Entering = hitRift_Entering.transform.parent.GetComponent<RiftObj>();
            // We also need to then update the intArry coordinates again
            dest_AsIntArr_RiftSpace = RL_F.V3DFV3_IA(dest_AsVect_WorldSpace, rift_Entering.transform.position);
        }

        // Check for obstacles
        // Above
        if (moveCard == DirType.U)
        {
            temp_HitRock = Physics.RaycastAll(ray_Top, RL_V.vertUnit, maskObstacleRock);

            if (temp_HitRock.Length > 0)
            {
                hitObstacle = temp_HitRock[0].transform.gameObject;
            }
        }
        // Below
        else if (moveCard == DirType.D)
        {
            temp_HitRock = Physics.RaycastAll(ray_Bottom, RL_V.vertUnit, maskObstacleRock);

            if (temp_HitRock.Length > 0)
            {
                hitObstacle = temp_HitRock[0].transform.gameObject;
            }
        }

        // Check for Top Bridge items
        RaycastHit[] temp_HitTop = Physics.RaycastAll(ray_Top, RL_V.vertUnit, maskFloor);
        if (temp_HitTop.Length > 0)
        {
            hitTop = temp_HitTop[0].transform.gameObject;
            if (hitTop.layer == (int)CollLayers.Bridge)
            {
                tBridgeObj_Top = hitTop.GetComponent<BridgeObj>();
            }
        }

        // Check for Middle Bridge items
        RaycastHit[] temp_HitMiddle = Physics.RaycastAll(ray_Middle, RL_V.vertUnit, maskFloor);
        if (temp_HitMiddle.Length > 0)
        {
            hitMiddle = temp_HitMiddle[0].transform.gameObject;
            if (hitMiddle.layer == (int)CollLayers.Bridge)
            {
                tBridgeObj_Middle = hitMiddle.GetComponent<BridgeObj>();
            }
        }

        // Check for Bottom Bridge items
        RaycastHit[] temp_HitBottom = Physics.RaycastAll(ray_Bottom, RL_V.vertUnit, maskFloor);
        if (temp_HitBottom.Length > 0)
        {
            hitBottom = temp_HitBottom[0].transform.gameObject;
            if (hitBottom.layer == (int)CollLayers.Bridge)
            {
                tBridgeObj_Bottom = hitBottom.GetComponent<BridgeObj>();
            }
        }

        // Check for Bottom Lower floors
        RaycastHit[] temp_HitBottom_Lower = Physics.RaycastAll(ray_Bottom_Lower, RL_V.vertUnit, maskFloor);
        if (temp_HitBottom_Lower.Length > 0)
        {
            hitBottom_Lower = temp_HitBottom_Lower[0].transform.gameObject;
        }

        // Check for rockObj
        RaycastHit[] tHitRock = Physics.RaycastAll(ray_Dir, RL.dir_To_Unit[moveCard], maskRock);
        if (tHitRock.Length > 0)
        {
            hitRock = tHitRock[0].transform.gameObject;
            if (hitRock.layer == (int)CollLayers.Rock)
            {
                tRockObj = hitRock.GetComponent<RockObj>();
            }
        }

        // Check for lift next
        RaycastHit[] tHitLift = Physics.RaycastAll(ray_Dir, RL.dir_To_Unit[moveCard], maskLift);
        if (tHitLift.Length > 0)
        {
            GameObject hitLift = tHitLift[0].transform.gameObject;
            if (hitLift.layer == (int)CollLayers.Lift)
            {
                relativeTransform_Entering = hitLift.transform;
            }
        }

        // Check for lift previous
        tHitLift = Physics.RaycastAll(ray_Behind, RL.dir_To_Unit[moveCard], maskLift);
        if (tHitLift.Length > 0)
        {
            GameObject hitLift = tHitLift[0].transform.gameObject;
            if (hitLift.layer == (int)CollLayers.Lift)
            {
                relativeTransform_Leaving = hitLift.transform;
            }
        }

        // We can store a reference to our destination rift to make it easier to type.
        RiftObj dest_Rift = pD.activeRift;
        if (rift_Leaving != null)
        {
            dest_Rift = null;
        }
        if (rift_Entering != null)
        {
            dest_Rift = rift_Entering;
        }

        // If we're entering or leaving a relativeTransform, we need to mark the change
        if (relativeTransform_Leaving != null)
        {
            actionKit.LiftKit(LiftType.ChangeLift);
            active_RelativeTransform = null;
        }
        if (relativeTransform_Entering != null)
        {
            actionKit.LiftKit(LiftType.ChangeLift);
            active_RelativeTransform = relativeTransform_Entering;
        }

        # region NoClip

        if (gM.isNoClip)
        {
            if ((moveCard == DirType.N) || (moveCard == DirType.E) || (moveCard == DirType.S) || (moveCard == DirType.W))
            {
                actionKit.MoveKit(MoveType.Step);
            }
            else if ((moveCard == DirType.U) || (moveCard == DirType.D))
            {
                actionKit.MoveKit(MoveType.Climb);
            }

            return actionKit;
        }

        # endregion

        # endregion

        // A. For regular movement
        # region Cardinal Movement

        if ((moveCard == DirType.N) || (moveCard == DirType.E) || (moveCard == DirType.S) || (moveCard == DirType.W))
        {
            // We can't leave a puzzle until the puzzle is solved
            // We can leave through the direction we came, though
            // If we are attempting to leave a puzzle
            if (rift_Leaving != null)
            {
                // We can leave whenever we like
                if (!gM.noExitRestrictions)
                {
                    // Check if either we're not leaving in the exit direction
                    if (rift_Leaving.exitDir != moveCard)
                    {
                        // Check if the puzzle is not solved
                        if (!rift_Leaving.isSolvedNow)
                        {
                            // In this case, we can't leave
                            actionKit.NonMoveKit(NonMoveType.PuzzleIncomplete);
                        }
                    }
                }
            }

            // Outside Rift
            if (dest_Rift == null)
            {
                // Rules
                // v/ We can't walk on air
                // v/ We can't walk through rocks
                // v/ We can't move from tangent plank directions
                // v/ We can't move onto tangent plank directions
                
                // 1. Check for rock obstacles
                if (hitObstacle != null)
                {
                    actionKit.NonMoveKit(NonMoveType.Obstacle);
                }

                // 2. Check if we're Prv on a plank
                // We can't move tagentially off of a plank
                if (tBridgeObj_Prv != null && tBridgeObj_Prv.bridgeType == BridgeType.Plank)
                {
                    if (!RL_F.C_MC_PD(moveCard, tBridgeObj_Prv.plankDir))
                    {
                        // Player is moving the wrong way on a plank
                        actionKit.NonMoveKit(NonMoveType.PlankTangent);
                    }
                }

                // 3. Check for a floor, or bridge object we can walk on Nxt
                if (hitNxt != null)
                {
                    // We can walk onto almost any floor, apart from a plank that is facing the wrong way
                    if (tBridgeObj_Nxt != null && tBridgeObj_Nxt.bridgeType == BridgeType.Plank)
                    {
                        // Is the plank facing the wrong way?
                        if (!RL_F.C_MC_PD(moveCard, tBridgeObj_Nxt.plankDir))
                        {
                            // Because we're outside the level, the player cannot move onto this item
                            actionKit.NonMoveKit(NonMoveType.PlankTangent);
                        }
                    }

                    actionKit.MoveKit(MoveType.Step);
                }
                else
                {
                    actionKit.NonMoveKit(NonMoveType.NoFloor);
                }

            }
            
            // Inside Rift
            else
            {
                // Rules
                // v/ We can't walk through rocks
                // v/ We can't move or build from tangent plank directions
                // v/ We can move onto a tangent plank
                // --- If we're parallel
                // --- If we're building and there are enough materails
                // v/ We can walk on air (Place bridge) if there are enough materials
                // v/ We can overwrite planks with bridges, if our direction is tangent
                // v/ We can overwrite bridges with planks, if toggle is held down
                // v/ We move onto bridges, without placing a new bridge
                // v/ We move onto planks, without placing a new plank

                // 1. Check for obstacles
                if (hitObstacle != null)
                {
                    actionKit.NonMoveKit(NonMoveType.Obstacle);
                }

                // 2. Check if we're Prv on a plank
                // We can't move or build tagentially off of a plank
                if (tBridgeObj_Prv != null && tBridgeObj_Prv.bridgeType == BridgeType.Plank)
                {
                    if (!RL_F.C_MC_PD(moveCard, tBridgeObj_Prv.plankDir))
                    {
                        Debug.Log("We can't build tangentially from a plank");
                        // Player is moving the wrong way on a plank
                        actionKit.NonMoveKit(NonMoveType.PlankTangent);
                    }
                }

                // 3. If there is no floor here, we can walk onto it
                // As long as there are enough materials to build there
                if (hitNxt == null)
                {
                    // For placing bridges
                    if (!sI.toggle)
                    {
                        // Check we're still inside the grid
                        if (RL_F.C_IG(new int[3] {dest_Rift.gsly, dest_Rift.gsx, dest_Rift.gsy}, dest_AsIntArr_RiftSpace))
                        {
                            // Check there are enough bridge pieces left
                            if (dest_Rift.bridgePieces_Cur > 0)
                            {
                                actionKit.BuildKit(BuildType.Bridge);
                            }
                            else
                            {
                                actionKit.NonMoveKit(NonMoveType.NoBridgePieces);
                            }
                        }
                    }
                    // For placing planks
                    else
                    {
                        // Check we're still inside the grid
                        if (RL_F.C_IG(new int[3] {dest_Rift.gsly, dest_Rift.gsx, dest_Rift.gsy}, dest_AsIntArr_RiftSpace))
                        {
                            // Check there are enough plank pieces left
                            if (dest_Rift.plankPieces_Cur > 0)
                            {
                                actionKit.BuildKit(BuildType.Plank);
                            }
                            else
                            {
                                actionKit.NonMoveKit(NonMoveType.NoPlankPieces);
                            }
                        }
                    }
                }
                
                // 4. If there is a floor
                // If it's a floor item, we can step onto it, as normal
                // If it's a bridge, we can step onto it
                // --- Or replace it with a plank, if there are enough pieces
                // If it's a plank
                // --- We can step on it, if parrallel
                // --- If placing a bridge, we can put a bridge down, if enough pieces are left
                // --- If placing a plank, we can place a plank to create a cross plank pieces
                if (hitNxt != null)
                {
                    // For non bridge floors, we can just walk on them
                    if (tBridgeObj_Nxt == null)
                    {
                        actionKit.MoveKit(MoveType.Step);
                    }
                    
                    // If the floor is a bridge or a stairs
                    if (tBridgeObj_Nxt != null && RL_F.C_AB(tBridgeObj_Nxt.bridgeType))
                    {
                        // If toggle is deactivated
                        if (!sI.toggle)
                        {
                            // We can step into it
                            actionKit.MoveKit(MoveType.Step);
                        }
                        // If placing in plank mode
                        else
                        {
                            // If there are enough planks left
                            if (dest_Rift.plankPieces_Cur > 0)
                            {
                                // We can place a plank
                                actionKit.BuildKit(BuildType.Plank);
                            }
                            else
                            {
                                // We just step on it as normal
                                actionKit.MoveKit(MoveType.Step);
                            }
                        }
                    }

                    // If the floor is a plank
                    if (tBridgeObj_Nxt != null && tBridgeObj_Nxt.bridgeType == BridgeType.Plank)
                    {
                        // Check if it's parallel or cross
                        if (RL_F.C_MC_PD(moveCard, tBridgeObj_Nxt.plankDir))
                        {
                            // We can walk on it
                            actionKit.MoveKit(MoveType.Step);
                        }
                        // If we're in bridge placing mode, we can put a bridge down
                        else if (!sI.toggle)
                        {
                            // If there are enough pieces
                            if (dest_Rift.bridgePieces_Cur > 0)
                            {
                                actionKit.BuildKit(BuildType.Bridge);
                            }
                            else
                            {
                                // Not enough bridge pieces
                                actionKit.NonMoveKit(NonMoveType.NoBridgePieces);
                            }
                        }
                        else
                        {
                            // If there are enough pieces
                            if (dest_Rift.plankPieces_Cur > 0)
                            {
                                actionKit.BuildKit(BuildType.Plank);
                            }
                            else
                            {
                                // Not enough plank pieces
                                actionKit.NonMoveKit(NonMoveType.NoPlankPieces);
                            }
                        }
                    }
                }
            }
        }

        # endregion

        // B. For stairs movement
        # region  Stairs Movement

        else if ((moveCard == DirType.U) || (moveCard == DirType.D))
        {
            // Is there a staircase already here?

            // Rules
            // v/ We can move up and down stairs that are already placed
            // v/ We can't move up or down if an obstacle is in the way
            // v/ We can't move down a stairs unto a plank (Or up a stairs from a plank)

            // First, check if inside or outside a rift
            // Outside Rift, if there are no stairs, no movement is allowed.
            if (dest_Rift == null)
            {
                // If there is a stair in this direction
                if (C_ST_UD(tBridgeObj_Top, tBridgeObj_Middle, moveCard))
                {
                    if (moveCard == DirType.U)
                    {
                        // We can't climb up from a plank
                        if ((hitBottom != null) ||
                            ((tBridgeObj_Middle != null) && (tBridgeObj_Middle.bridgeType != BridgeType.Plank)))
                        {
                            // Check that the stairs aren't blocked
                            if (hitObstacle == null)
                            {
                                // We can climb up
                                actionKit.MoveKit(MoveType.Climb);
                            }
                        }
                    }
                    else if (moveCard == DirType.D)
                    {
                        // Is there a floor at the bottom for us to walk on? It can't be a plank
                        if ((hitBottom_Lower != null) ||
                                ((tBridgeObj_Bottom != null) && (tBridgeObj_Bottom.bridgeType != BridgeType.Plank)))
                        {
                            // Check that the stairs aren't blocked
                            if (hitObstacle == null)
                            {
                                // We can move down
                                actionKit.MoveKit(MoveType.Climb);
                            }
                        }
                    }
                }
                // If there isn't a stair in this direction
                else
                {
                    if (moveCard == DirType.U)
                    {
                        actionKit.NonMoveKit(NonMoveType.NoStairsUp);
                    }
                    else if (moveCard == DirType.D)
                    {
                        actionKit.NonMoveKit(NonMoveType.NoStairsDown);
                    }
                }

            }
            // Inside Rift, if there are no stairs, we apply buildmode rules
            else
            {
                // Rules
                // v/ We can't leave the puzzle vertically, check we're still inside the puzzle area
                // v/ We can move up and down by placing a stairs, if there are enough materials
                // v/ We can't build up or down if a rock is in the way
                // v/ We can't build down a stairs unto a plank
                // v/ We can't build up from a plank onto a stair
                // v/ We can build up a staircase onto a plank by replacing it, if there are enough materials

                // If there is a stair in this direction
                if (C_ST_UD(tBridgeObj_Top, tBridgeObj_Middle, moveCard))
                {
                    if (moveCard == DirType.U)
                    {
                        Debug.Log("Climb");
                        // Check that the stairs aren't blocked
                        if (hitObstacle == null)
                        {
                            Debug.Log("Climb3");
                            // We can climb up
                            actionKit.MoveKit(MoveType.Climb);
                        }
                    }
                    else if (moveCard == DirType.D)
                    {
                        // Check that the stairs aren't blocked
                        if (hitObstacle == null)
                        {
                            // We can move down
                            actionKit.MoveKit(MoveType.Climb);
                        }
                    }
                }
                // If there isn't a stair in this direction
                else
                {
                    if (moveCard == DirType.U)
                    {
                        // Check we're still inside the grid
                        if (dest_AsIntArr_RiftSpace[0] >= pD.activeRift.gsly)
                        {
                            actionKit.NonMoveKit(NonMoveType.LevelBarrier);
                        }
                        else
                        {
                            // We need a ladder and a bridge piece, but if there is a plank or a bridge already in that place
                            // Then we only need one fewer item
                            if (!(dest_Rift.bridgePieces_Cur > 0))
                            {
                                actionKit.NonMoveKit(NonMoveType.NoBothPieces);
                            }
                            // If we have enough pieces
                            else
                            {
                                // Check that the stairs aren't blocked
                                if (hitObstacle == null)
                                {
                                    // We can build a stairs
                                    actionKit.BuildKit(BuildType.Stairs);
                                }
                            }
                        }
                    }
                    else if(moveCard == DirType.D)
                    {
                        // Check we're still inside the grid
                        if (dest_AsIntArr_RiftSpace[0] < 0)
                        {
                            actionKit.listOf_NonMoveType.Add(NonMoveType.LevelBarrier);
                        }
                        else
                        {
                            // We can actually build a stairs here IF there is a floor beneath us
                            // This is a little more complex than I'd like
                            // Check what materials we have and what materials we're standing on
                            if (hitBottom_Lower == null && !(dest_Rift.bridgePieces_Cur > 0))
                            {
                                actionKit.NonMoveKit(NonMoveType.NoBothPieces);
                            }
                            // If we have enough materials
                            else
                            {
                                // Check that the stairs aren't blocked
                                if (hitObstacle == null)
                                {
                                    // We can build a stairs
                                    actionKit.BuildKit(BuildType.Stairs);
                                }
                                else
                                {
                                    // We can't build, there's an obstacle
                                    actionKit.NonMoveKit(NonMoveType.Obstacle);
                                }
                            }
                        }
                    }
                }
            }
        }

        # endregion
        
        // C. For mining
        # region Mining

        // After the actionKit has been set
        // We can use it to check for mining moves
        // We only mine if we are perform a movement
        if (actionKit.moveType != MoveType.None)
        {
            // Check if there is rockObj in this direction
            if (hitRock != null)
            {
                // Grab a reference to it's componenet values
                RockData rockData = hitRock.GetComponent<RockObj>().rockData;

                // Check if the rock is solid
                if (rockData.rockType == RockType.Solid)
                {
                    // If we have enough mining pieces, we can include mining in the actionKit
                    if (dest_Rift.minePieces_Cur > 0)
                    {
                        actionKit.MineKit(MineType.Mine);
                    }
                    // If we don't have enough mining pieces
                    else
                    {
                        // We can't move or build anymore and we reset the actionKit
                        actionKit.NonMoveKit(NonMoveType.NoMinePieces);
                    }
                }
            }
        }

        # endregion

        // D. For a reset
        # region Reset

        else if (sI.reset)
        {
            actionKit.MetaKit(MetaType.Reset);
        }

        # endregion

        // E. For a delete command
        # region Delete

        // If we're stepping or climbing while holding/tapping delete, we can remove a piece
        if (sI.delete && (actionKit.moveType == MoveType.Climb || actionKit.moveType == MoveType.Step))
        {
            // We can only do this whilst previously inside a rift
            if (C_MT.rift_Leaving != null || (C_MT.rift_Leaving == null && pD.activeRift != null))
            {
                // We can only do this while stepping or climbing, no building at the same time
                if (actionKit.buildType == BuildType.None)
                {
                    actionKit.MetaKit(MetaType.Delete);
                }
            }
        }
        // If we're building/mining while holding/tapping delete, we can't perform this action, it's not allowed
        else if (sI.delete && (actionKit.buildType == BuildType.Bridge || actionKit.buildType == BuildType.Plank || actionKit.buildType == BuildType.Stairs))
        {
            // We can only do this whilst previously inside a rift
            if (C_MT.rift_Leaving != null || (C_MT.rift_Leaving == null && pD.activeRift != null))
            {
                actionKit.NonMoveKit(NonMoveType.CantBuildAndDelete);
            }
        }

        # endregion

        // F. For the player entering a moving platform
        # region Lift
        // If we are moving onto a lift, the movement is of type lift



        # endregion

        // G. For a warp
        # region Warp

        else if (sI.warp)
        {
            actionKit.MetaKit(MetaType.Warp);
        }

        # endregion

        // We return the action kit at the end
        // Double check there are no movement errors
        // Everything else is designed to set up the actionKit before returning it
        if (actionKit.listOf_NonMoveType.Count <= 0)
        {
            return actionKit;
        }
        else
        {
            actionKit.moveType = MoveType.None;
            actionKit.buildType = BuildType.None;
            actionKit.mineType = MineType.None;
            actionKit.metaType = MetaType.None;
            actionKit.liftType = LiftType.None;
            return actionKit;
        }
    }

    // Check if a stairpiece is here based on the U/D direction passed
    public static bool C_ST_UD(BridgeObj tBridgeObj_Top, BridgeObj tBridgeObj_Middle, DirType dirType)
    {
        if (dirType == DirType.U)
        {
            // There exists a staircase above us
            if ((tBridgeObj_Top != null) && (tBridgeObj_Top.bridgeType == BridgeType.Stairs))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (dirType == DirType.D)
        {
            // There exists a staircase where we are
            if ((tBridgeObj_Middle != null) && (tBridgeObj_Middle.bridgeType == BridgeType.Stairs))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Failsafe
        return false;
    }
}
