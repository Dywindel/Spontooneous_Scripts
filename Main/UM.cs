using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This scripts controls the main update loop for player interaction while playing the game

public class UM : MonoBehaviour
{
    # region Singleton

    public static UM Instance {get; private set; }

    private void Awake()
    {
        Instance = this;
    }
        
    # endregion
    
    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    GM gM;
    PD pD;
    Sc_SortInput sI;
    Sc_Player player;

    ///////////////
    // ANIMATION //
    ///////////////

    // Enum describing why we couldn't move just now
    public NonMoveType nonMoveType;

    ////////////////////
    // COMMAND BUFFER //
    ////////////////////

    // ICommands
    List<List<ACommand>> _CommandBuffer = new List<List<ACommand>>();
    List<ACommand> _CurrentCommands;

    void Start()
    {
        WorldReferences();
    }

    void WorldReferences()
    {
        gM = GM.Instance;
        pD = PD.Instance;
        player = Sc_Player.Instance;

        sI = gM.GetComponent<Sc_SortInput>();
    }

    void Update()
    {
        // We ignore player movement when we're inside a menu
        if (sI.isPlayer) // Menu bool goes here
        {
            // Check for movement input, stairs movement
            // We don't need to check for a delete key, as a delete key only works when a direction is also input
            if (sI.isMove || sI.stairsUp || sI.stairsDown)
            {
                Action_Move(sI.moveCard);
            }

            // Reset
            if (sI.reset)
            {
                // If we're inside an active rift
                if (pD.activeRift != null)
                {
                    // I don't want to be able to perform a reset command, if the last command we performed was a reset
                    // Reset is single, so I can take the first element always
                    if (_CommandBuffer.Last()[0].actionKit.metaType != MetaType.Reset)
                    {
                        // Cancel the animation coroutine, if it's moving
                        player.Stop_Animation();

                        Action_Move(DirType.None);
                    }
                }
            }

            // Undo
            if (sI.undo)
            {
                // Cancel the animation coroutine, if it's moving
                player.Stop_Animation();

                Perform_Undo();
            }
        }

        // We constantly check the animation mode
        player.Update_Animation_Mode();
    }

    // This is the bulk of movement for the player in the game
    // And what effects the player has on moving in the world
    public void Action_Move(DirType moveCard)
    {
        // Update destination coordinates
        Update_DestinationCoordinates(moveCard);

        // Check if movement is possible based on game rules and return what type of movement to perform
        // Pass in a reference to the nonMoveType animation enum, which can be updated if the moveType returns None
        ActionKit actionKit = C_MT._C_MT(moveCard);

        //print ("Move: " + actionKit.moveType + " - Build: " + actionKit.buildType + " - Mine: " + actionKit.mineType + " - Meta: " + actionKit.metaType + " - NonMove_Count: " + actionKit.listOf_NonMoveType.Count);

        // Update the active rift, if we're moving
        if (actionKit.moveType != MoveType.None)
        {
            Update_ActiveRift();
        }

        // Store actions inside the command buffer
        Perform_CommandBuffer(actionKit, moveCard);

        // Perform instance actions after the command buffer
        Perform_Instant(actionKit, moveCard);

        if (pD.activeRift != null)
        {
            pD.activeRift.Update_TrellisSpace();
        }

        // Perform actions after the instant actions occur
        Perform_AfterInstant(actionKit, moveCard);

        // Visual Updates
        Perform_Gradual(actionKit, moveCard);

        // Update rift visuals
        if (pD.activeRift != null)
        {
            pD.activeRift.Update_Visuals();
        }

        // Run the animations for player movement
        player.Run_Movements_Animation();

        // Check if puzzle is solved, after no movement for a couple of seconds
        pD.Check_IfRift_Solved(pD.activeRift);

        // Reset any neccessary variables
        Reset_Variables();
    }

    /////////////////////
    // RESET VARIABLES //
    /////////////////////

    void Reset_Variables()
    {
        pD.Reset_Variables();
    }

    /////////////////////
    // PERFORM INSTANT //
    /////////////////////

    // Data changes that are instantaneous during player actions

    // Update the active rift state
    void Update_ActiveRift()
    {
        // Update the active rift inside the pD
        // Not a current concern for Stairs
        if (C_MT.rift_Leaving != null)
        {
            pD.Toggle_Active_Rift(null);
        }
        if (C_MT.rift_Entering != null)
        {
            pD.Toggle_Active_Rift(C_MT.rift_Entering);
        }
    }

    // Update the destination coordinates for the player
    void Update_DestinationCoordinates(DirType moveCard)
    {
        // The active position we are moving to after movement is performed
        // To get this value correctly, I need the angle the player is at
        C_MT.dest_AsVect_WorldSpace = player.player_Collider.transform.position + RL.dir_To_Unit[moveCard]*RL.dir_To_Vect[moveCard];
        C_MT.dest_AsVect_RelativeSpace = player.player_Collider.transform.localPosition + RL.dir_To_Unit[moveCard]*RL.dir_To_Vect[moveCard];

        // The active coordinates we are moving to after movement is performed, if inside a rift
        if (pD.activeRift != null)
        {
            C_MT.dest_AsIntArr_RiftSpace = RL_F.V3DFV3_IA(C_MT.dest_AsVect_WorldSpace, pD.activeRift.transform.position);
        }
        else
        {
            // Zeroed if we're not moving anywhere
            C_MT.dest_AsIntArr_RiftSpace = new int[3] {0, 0, 0};
        }
    }

    void Perform_CommandBuffer(ActionKit actionKit, DirType moveCard)
    {
        // We do not perform the command buffer stage if nothing happens
        if (actionKit.CheckNull())
        {
           // We split this up based on type and store relevent actions inside the command list
            // Use if, not else if as we might do multiple actions
            _CurrentCommands = new List<ACommand>();
            
            // MoveType
            if (actionKit.moveType != MoveType.None)
            {
                // Store the move commands
                _CurrentCommands.Add( new MoveCommand(player.player_Collider.transform.position, moveCard, player.new_FaceDir, C_MT.rift_Entering, C_MT.rift_Leaving));
            }

            // BuildType
            if (actionKit.buildType != BuildType.None)
            {
                // Bridges
                if (actionKit.buildType == BuildType.Bridge)
                {
                    // We want to grab a reference to the bridgeData we are moving to
                    BridgeData prv_BridgeData = null;

                    prv_BridgeData = new BridgeData(pD.activeRift.arrayOf_BridgeData[C_MT.dest_AsIntArr_RiftSpace[0], C_MT.dest_AsIntArr_RiftSpace[1], C_MT.dest_AsIntArr_RiftSpace[2]]);
                    
                    // Store the move commands
                    _CurrentCommands.Add( new BridgeCommand(C_MT.dest_AsIntArr_RiftSpace, pD.activeRift, prv_BridgeData));
                }
                // Planks
                else if (actionKit.buildType == BuildType.Plank)
                {
                    // We want to grab a reference to the bridgeData we are moving to
                    BridgeData prv_BridgeData = null;

                    prv_BridgeData = new BridgeData(pD.activeRift.arrayOf_BridgeData[C_MT.dest_AsIntArr_RiftSpace[0], C_MT.dest_AsIntArr_RiftSpace[1], C_MT.dest_AsIntArr_RiftSpace[2]]);

                    // Store the move commands
                    _CurrentCommands.Add( new PlankCommand(C_MT.dest_AsIntArr_RiftSpace, pD.activeRift, prv_BridgeData));
                }
                // Stairs
                else if (actionKit.buildType == BuildType.Stairs)
                {
                    int temp_yInt = 0;
                    if (moveCard == DirType.D)
                    {
                        temp_yInt = 1;
                    }

                    // We want to grab the bridge types for where we were and where we're moving to
                    List<BridgeData> listOf_Prv_BridgeData = new List<BridgeData>();
                    // Note, we want to make a copy of the BridgeData values, not a reference to the BrideData. I've created a constructor for this purpose
                    // Where we moved from[0]
                    // We ignore this for updwards movement, as there will always have to be some bridge item for us to moveup from
                    if (moveCard == DirType.D)
                    {
                        listOf_Prv_BridgeData.Add( new BridgeData(pD.activeRift.arrayOf_BridgeData[C_MT.dest_AsIntArr_RiftSpace[0] + (2*temp_yInt - 1), C_MT.dest_AsIntArr_RiftSpace[1], C_MT.dest_AsIntArr_RiftSpace[2]]) );
                    }
                    else
                    {
                        // We mark this as none because, during the undo step, we will always land on the previous bridge component (So, we don't need to store it right now)
                        listOf_Prv_BridgeData.Add( new BridgeData(new int[] {C_MT.dest_AsIntArr_RiftSpace[0] + (2*temp_yInt - 1), C_MT.dest_AsIntArr_RiftSpace[1], C_MT.dest_AsIntArr_RiftSpace[2]}) );
                    }
                    // Where we moved to[1]
                    listOf_Prv_BridgeData.Add( new BridgeData(pD.activeRift.arrayOf_BridgeData[C_MT.dest_AsIntArr_RiftSpace[0], C_MT.dest_AsIntArr_RiftSpace[1], C_MT.dest_AsIntArr_RiftSpace[2]]) );
                    
                    // Store the move commands
                    _CurrentCommands.Add( new StairsCommand(moveCard, new int[3] {C_MT.dest_AsIntArr_RiftSpace[0] + temp_yInt, C_MT.dest_AsIntArr_RiftSpace[1], C_MT.dest_AsIntArr_RiftSpace[2]}, pD.activeRift, listOf_Prv_BridgeData));
                }
            }

            // MineType
            if (actionKit.mineType != MineType.None)
            {
                // Store the rockObj in the location we're at
                RockData prv_RockData = null;

                // Remember to add one layer int as we're in RockSpace
                prv_RockData = new RockData( pD.activeRift.arrayOf_RockData[C_MT.dest_AsIntArr_RiftSpace[0] + 1, C_MT.dest_AsIntArr_RiftSpace[1], C_MT.dest_AsIntArr_RiftSpace[2]] );
                
                // Store the mining command
                _CurrentCommands.Add( new MineCommand(C_MT.dest_AsIntArr_RiftSpace, pD.activeRift, prv_RockData));
            }

            // MetaType
            if (actionKit.metaType != MetaType.None)
            {
                // Reset
                if (actionKit.metaType == MetaType.Reset)
                {
                    // Grab the list of bridgeData in the previous locations (Before we place the stairs)
                    List<BridgeData> temp_ListOf_Prv_BridgeData = new List<BridgeData>();
                    for (int ly = 0; ly < pD.activeRift.gsly; ly++)
                    {
                        for (int j = 0; j < pD.activeRift.gsy; j++)
                        {
                            for (int i = 0; i < pD.activeRift.gsx; i++)
                            {
                                if (pD.activeRift.arrayOf_BridgeData[ly, i, j].bridgeType != BridgeType.None)
                                {
                                    // Note, we want to make a copy of the BridgeData values, not a reference to the BrideData. I've created a constructor for this purpose
                                    temp_ListOf_Prv_BridgeData.Add( new BridgeData(pD.activeRift.arrayOf_BridgeData[ly, i, j]) );
                                }
                            }
                        }
                    }

                    // Store the move commands
                    _CurrentCommands.Add( new ResetCommand(temp_ListOf_Prv_BridgeData, C_MT.dest_AsVect_WorldSpace, pD.activeRift));
                }

                // Delete
                else if (actionKit.metaType == MetaType.Delete)
                {
                    // Store the rift we just left.
                    // If we didn't leave, store the currently active rift
                    RiftObj refRift = C_MT.rift_Leaving;
                    if (C_MT.rift_Leaving == null)
                    {
                        refRift = pD.activeRift;
                    }

                    // The previous position we were at
                    int[] prv_IntArr = new int[3] {0, 0, 0}; 

                    BridgeData prv_BridgeData = null; 

                    // Create a bridgeData copy of that item
                    if (moveCard == DirType.N || moveCard == DirType.E || moveCard == DirType.S || moveCard == DirType.W)
                    {
                        prv_IntArr = new int[3] {C_MT.dest_AsIntArr_RiftSpace[0] - (int)RL.dir_To_Vect[moveCard][1], C_MT.dest_AsIntArr_RiftSpace[1] - (int)RL.dir_To_Vect[moveCard][0], C_MT.dest_AsIntArr_RiftSpace[2] - (int)RL.dir_To_Vect[moveCard][2]};
                    }
                    else if (moveCard == DirType.U)
                    {
                        prv_IntArr = new int[3] {C_MT.dest_AsIntArr_RiftSpace[0] - (int)RL.dir_To_Vect[moveCard][1], C_MT.dest_AsIntArr_RiftSpace[1] - (int)RL.dir_To_Vect[moveCard][0], C_MT.dest_AsIntArr_RiftSpace[2] - (int)RL.dir_To_Vect[moveCard][2]};
                    }
                    else if (moveCard == DirType.D)
                    {
                        prv_IntArr = new int[3] {C_MT.dest_AsIntArr_RiftSpace[0] - (int)RL.dir_To_Vect[moveCard][1], C_MT.dest_AsIntArr_RiftSpace[1] - (int)RL.dir_To_Vect[moveCard][0], C_MT.dest_AsIntArr_RiftSpace[2] - (int)RL.dir_To_Vect[moveCard][2]};
                    }

                    prv_BridgeData = new BridgeData(refRift.arrayOf_BridgeData[prv_IntArr[0], prv_IntArr[1], prv_IntArr[2]]);

                    _CurrentCommands.Add( new DeleteCommand(moveCard, refRift, prv_BridgeData));
                }

                // Warp
                else if (actionKit.metaType == MetaType.Warp)
                {
                    // Just need to store the previous position we were at
                    _CurrentCommands.Add( new WarpCommand(C_MT.dest_AsVect_WorldSpace));
                }
            }
     
            // LiftType
            else if (actionKit.liftType != LiftType.None)
            {
                // If the lift has changed
                // Store a reference to the player's previous and next position, relative or in worldspace, depending on
                // If they're moving into a relative transform or not
                Vector3 player_RelativePosition_Leaving = Vector3.zero;
                
                if (C_MT.relativeTransform_Entering != null)
                {
                    // If we're entering a relative transform, we need to store the previous player worldspace position
                    player_RelativePosition_Leaving = player.player_Relative.localPosition;
                }
                if (C_MT.relativeTransform_Leaving != null)
                {
                    // If we're leaving a relative transform, we need to store the previous player's local position
                    player_RelativePosition_Leaving = player.player_Relative.localPosition;
                }

                // Store a reference to the exiting and entering transform
                _CurrentCommands.Add(new LiftCommand(player_RelativePosition_Leaving, Vector3.zero, C_MT.relativeTransform_Leaving, C_MT.relativeTransform_Entering));
            }
            
            // Finally, pass the list of ACommands into the CommandBuffer
            _CommandBuffer.Add(_CurrentCommands); 
        }
        
    }

    // Update relevent instant variables and objects after the command buffer has stored data
    void Perform_Instant(ActionKit actionKit, DirType moveCard)
    {
        // Sometimes we want to update the animation state of the player
        if (actionKit.moveType == MoveType.Climb)
        {
            player.moveType = MoveType.Climb;
        }
        else
        {
            player.moveType = MoveType.None;
        }

        // Face direction
        // Update the face direction always, regardless of the actionKit
        // Update the faceDir, if moving cardinally
        if (moveCard == DirType.N || moveCard == DirType.E || moveCard == DirType.S || moveCard == DirType.W)
        {
            player.new_FaceDir = moveCard;
        }

        // If we had a NonMoveType, update the animator on the player
        if (actionKit.listOf_NonMoveType.Count > 0)
        {
            player.nonMoveType = actionKit.listOf_NonMoveType[0];
        }
        else
        {
            player.nonMoveType = NonMoveType.None;
        }

        // MoveType
        if (actionKit.moveType != MoveType.None)
        {
            // Teleport the player collider and relative game objects
            player.player_Collider.transform.position = C_MT.dest_AsVect_WorldSpace;
        }

        // BuildType
        if (actionKit.buildType != BuildType.None)
        {
            // Bridges
            if (actionKit.buildType == BuildType.Bridge)
            {
                // Place a bridge piece into the movement target location, where the player is currently
                pD.activeRift.Place_BridgeObj(C_MT.dest_AsIntArr_RiftSpace);
            }
            // Planks
            else if (actionKit.buildType == BuildType.Plank)
            {
                // Place a plank piece into the scene
                pD.activeRift.Place_PlankObj(C_MT.dest_AsIntArr_RiftSpace, moveCard);
            }
            // Stairs
            else if (actionKit.buildType == BuildType.Stairs)
            {
                print ("Placing stairs");

                // Place a stairs into this scene
                // We do this after the comman buffer, just so we can scope what items were here previously
                pD.activeRift.Place_Stairs(C_MT.dest_AsIntArr_RiftSpace, moveCard);
            }

            // We also update the GameData after placing an item
            IGD.Instance.Update_PuzzleState(pD.activeRift);
        }

        // MineType
        if (actionKit.mineType != MineType.None)
        {
            // Mining
            if (actionKit.mineType == MineType.Mine)
            {
                // Mine a Rock inside the grid
                pD.activeRift.Mine_RockObj(C_MT.dest_AsIntArr_RiftSpace);
            }

            // We also update the GameData after placing an item
            IGD.Instance.Update_PuzzleState(pD.activeRift);
        }

        // MetaType
        if (actionKit.metaType != MetaType.None)
        {
            // Reset
            if (actionKit.metaType == MetaType.Reset)
            {
                // Reset the player position
                player.Update_PlayerPos_Instant(pD.activeRift.resetPoint.position, DirType.N);

                // Perform instant updates (There are no gradual updates)
                pD.activeRift.Action_Reset();
            }

            // Delete
            else if (actionKit.metaType == MetaType.Delete)
            {
                // Store the rift we just left.
                // If we didn't leave, store the currently active rift
                RiftObj refRift = C_MT.rift_Leaving;
                if (C_MT.rift_Leaving == null)
                {
                    refRift = pD.activeRift;
                }

                // The previous position we were at
                int[] prv_IntArr = new int[3] {C_MT.dest_AsIntArr_RiftSpace[0] - (int)RL.dir_To_Vect[moveCard][1], C_MT.dest_AsIntArr_RiftSpace[1] - (int)RL.dir_To_Vect[moveCard][0], C_MT.dest_AsIntArr_RiftSpace[2] - (int)RL.dir_To_Vect[moveCard][2]};

                // Delete the bridge at that point, if it's not none
                if (refRift.arrayOf_BridgeData[prv_IntArr[0], prv_IntArr[1], prv_IntArr[2]].bridgeType != BridgeType.None)
                {
                    refRift.Return_GenObj(new int[3] {prv_IntArr[0], prv_IntArr[1], prv_IntArr[2]});
                }
            }
            // Warp
            else if (actionKit.metaType == MetaType.Warp)
            {
                // Reset the bool inside the sI
                sI.warp = false;
                // Update the player position
                player.Update_PlayerPos_Instant(WC.Instance.warpTo, DirType.N);
            }
        }

        // LiftType
        if (actionKit.liftType != LiftType.None)
        {
            // If the liftType has changed
            // Check what is the activeLift and set the player relative transform to be a child of it
            if (C_MT.active_RelativeTransform == null)
            {
                player.player_Relative.parent = player.transform;
                // Reset the player relative angles
                player.player_Relative.localRotation = Quaternion.Euler(0f, 0f, 0f);
                // Need to round up 'Relative' position values
                player.player_Relative.localPosition = RL_F.Round_V3(player.player_Relative.localPosition);
            }
            else
            {
                player.player_Relative.parent = C_MT.active_RelativeTransform;
                // Set the player relative angles
                player.player_Relative.localRotation = Quaternion.Euler(0f, 0f, 0f);
                // Need to round up 'Relative' position values
                player.player_Relative.localPosition = RL_F.Round_V3(player.player_Relative.localPosition);
            }
        }
        
    }

    // Perform after instant
    // Currently this just updates the info inside GameData
    void Perform_AfterInstant(ActionKit actionKit, DirType moveCard)
    {
        // MoveType
        if (actionKit.moveType != MoveType.None)
        {
            // Update the GameData
            IGD.Instance.Update_PlayerRelPos(player.player_Collider.transform.position, player.player_Relative.localPosition, C_MT.active_RelativeTransform);
        }

        // BuildType
        if (actionKit.buildType != BuildType.None)
        {
            // Update the GameData
            IGD.Instance.Update_PuzzleState(pD.activeRift);
        }

        // MineType
        if (actionKit.mineType != MineType.None)
        {
            // Update the GameData
            IGD.Instance.Update_PuzzleState(pD.activeRift);
        }

        // MetaType
        if (actionKit.metaType != MetaType.None)
        {
            // Reset
            if (actionKit.metaType == MetaType.Reset)
            {
                // Update the GameData
                IGD.Instance.Update_PuzzleState(pD.activeRift);
            }
        }

        // LiftType
        if (actionKit.liftType != LiftType.None)
        {
            // This will override the currently stored player position, which should be fine
            IGD.Instance.Update_PlayerRelPos(player.player_Collider.transform.position, player.player_Relative.localPosition, C_MT.active_RelativeTransform);
        }
    }


    // Update Visuals
    void Perform_Gradual(ActionKit actionKit, DirType moveCard)
    {
        // We want to update the meshes where we move to when
        // - We performed any build actions
        // - We perform an undo move
        if (actionKit.buildType != BuildType.None)
        {
            // Update the mesh of the bridge we just moved to
            // This uses the Trellis calculation we performed slightly earlier
            // Ensure Trellis is performed before updating each mesh
            if (pD.activeRift != null)
            {
                // If there is a bridgeitem here
                if (pD.activeRift.arrayOf_BridgeData[C_MT.dest_AsIntArr_RiftSpace[0], C_MT.dest_AsIntArr_RiftSpace[1], C_MT.dest_AsIntArr_RiftSpace[2]].bridgeType != BridgeType.None)
                {
                    pD.activeRift.arrayOf_BridgeData[C_MT.dest_AsIntArr_RiftSpace[0], C_MT.dest_AsIntArr_RiftSpace[1], C_MT.dest_AsIntArr_RiftSpace[2]].bridgeObj.GetComponent<BridgeObj_Mesh>().UpdateMesh(1);
                }
            }
        }
        // If we just deleted a piece, we need to update the mesh components around the piece we deleted
        if (actionKit.metaType == MetaType.Delete)
        {
            // This can be achieved by grabbing the coordinates of that piece, collecting any bridgeMeshes around it, then updating their meshes
            int[] prv_Pos = new int[3] {C_MT.dest_AsIntArr_RiftSpace[0] - (int)RL.dir_To_Vect[moveCard][0], C_MT.dest_AsIntArr_RiftSpace[1] - (int)RL.dir_To_Vect[moveCard][1], C_MT.dest_AsIntArr_RiftSpace[2] - (int)RL.dir_To_Vect[moveCard][2]};
            // We also need to store the rift that this position belongs to
            // We need to check if we haven't accidentally left a rift during the delete phase
            RiftObj prvRift = pD.activeRift;
            if (C_MT.rift_Leaving != null)
            {
                prvRift = C_MT.rift_Leaving;
            }

            prvRift.Update_SurroundingMeshes(prv_Pos);
        }

        // Perform player animations
        player.Perform_Gradual(actionKit, moveCard);
    }

    ////////////////////////
    // UNDO FUNCTIONALITY //
    ////////////////////////

    // Undo
    void Perform_Undo()
    {
        if (_CommandBuffer.Count > 0)
        {
            // Grab the list of undoCommands
            List<ACommand> undoCommands = _CommandBuffer.Last();
            
            // Remove the commands first
            _CommandBuffer.Remove(undoCommands);
            
            // Perform the undo commands
            foreach (ACommand undoCommand in undoCommands)
            {
                undoCommand.Undo();
            }
        }
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    // Limit the command buffer to a specific move size
    void AddToCommandBuffer(List<ACommand> _CurrentCommands)
    {
        // If the command buffer is full, remove the first item
        if (_CommandBuffer.Count >= RL_V.maxBuffer)
        {
            _CommandBuffer.Remove(_CommandBuffer.First());
        }

        _CommandBuffer.Add(_CurrentCommands);
    }

}
