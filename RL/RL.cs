using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// These Reference Library values are used throughout
// The entire game project

// This is a generic library

// Collision Layers
public enum CollLayers {Default = 0, Rift = 6, Lift = 7, Obstacle = 10, Bridge = 15, Rune = 16, Rock = 17, LaserInteraction = 25, Flammable = 26};

// Cardinal directions for NESW and Up and Down.
public enum DirType {None, N, E, S, W, U, D};

// Puzzle State enum - Denoates the colour of the backing image when storing the PNG
public enum RiftState {Concept, Rough, Drafted, Done, Rejected};
public enum RiftDifficulty {Tutorial, Easy, Medium, Hard, Expert};      // How hard the puzzle is (Useful for pacing)
public enum RiftPuzzleRating {Good, Great, Excellent, Godlike};         // What I think of the puzzle's solution (Useful for pacing)

// BoardType
// Boss type boards have dangers in them that can make a player have to restart the board
public enum BoardType {Normal, Museum, Boss};

// Visual bridge material - Metal is conductive
public enum BridgeMaterial {Stone, Metal, Wood, City};
// Visual Island biome materials
public enum MeshType {Slab, Island, Canyon, Moss, Onyx, Vines, Reference, Frames};
public enum CoverType {Grass, Grass_Light, Grass_Dark, Dirt, Lines, Carpet, Grate, Reference};

// After checking movement, return what type of movement the player is allowed to perform
// We split movement type into parallel actions
// That is to say, each of these actions could be peformed at the same time
public enum MoveType {None, Step, Climb};
public enum BuildType {None, Bridge, Plank, Stairs};
public enum MineType {None, Mine};
public enum MetaType {None, Reset, Delete, Warp};
public enum LiftType {None, ChangeLift};
// If a movement is not possible, return a reason why for animation purposes
public enum NonMoveType {None, Obstacle, NoFloor, PuzzleIncomplete, NoBridgePieces, NoPlankPieces,
                        NoBothPieces, NoMinePieces, NoStairsUp, NoStairsDown, LevelBarrier,
                        PlankTangent, CantBuildAndDelete};

// Actions the player can take
public class ActionKit
{
    public MoveType moveType = MoveType.None;
    public BuildType buildType = BuildType.None;
    public MineType mineType = MineType.None;
    public MetaType metaType = MetaType.None;
    public LiftType liftType = LiftType.None;

    // For animation
    public List<NonMoveType> listOf_NonMoveType;

    // Blank start
    public ActionKit()
    {
        moveType = MoveType.None;
        buildType = BuildType.None;
        mineType = MineType.None;
        metaType = MetaType.None;
        listOf_NonMoveType = new List<NonMoveType>();
    }

    // Moving
    public ActionKit(MoveType pMoveType)
    {
        moveType = pMoveType;
    }
    
    // Meta action, Delete/Reset (Reset movement set to None)
    public ActionKit(MoveType pMoveType, MetaType pMetaType)
    {
        moveType = pMoveType;
        metaType = pMetaType;
    }

    // Moving and building
    public ActionKit(BuildType pBuildType)
    {
        buildType = pBuildType;
        if (buildType == BuildType.Stairs)
        {
            moveType = MoveType.Climb;
        }
        else if (buildType != BuildType.None)
        {
            moveType = MoveType.Step;
        }
    }

    public void NonMoveKit(NonMoveType pNonMoveType)
    {
        listOf_NonMoveType.Add(pNonMoveType);
    }

    public void MoveKit(MoveType pMoveType)
    {
        moveType = pMoveType;
    }

    public void BuildKit(BuildType pBuildType)
    {
        buildType = pBuildType;
        if (buildType == BuildType.Stairs)
        {
            moveType = MoveType.Climb;
        }
        else if (buildType != BuildType.None)
        {
            moveType = MoveType.Step;
        }
    }

    public void MineKit(MineType pMineType)
    {
        mineType = pMineType;
    }

    public void MetaKit(MetaType pMetaType)
    {
        metaType = pMetaType;
    }

    public void LiftKit(LiftType pLiftType)
    {
        liftType = pLiftType;
    }
    
    // Check if all items are null
    public bool CheckNull()
    {
        bool tBool = false;

        if (moveType != MoveType.None)
        {
            tBool = true;
        }
        else if (buildType != BuildType.None)
        {
            tBool = true;
        }
        else if (mineType != MineType.None)
        {
            tBool = true;
        }
        else if (metaType != MetaType.None)
        {
            tBool = true;
        }
        else if (liftType != LiftType.None)
        {
            tBool = true;
        }

        return tBool;
    }    
}


public enum ColourType {White, Cream};

// This dictates what the current bridge type is that we are placing
public enum PlaceType {Bridge, Plank};
// Planks can be placed in a horizontal mode, vertical mode, or cross eachother (Essentially creating a regular bridge)
public enum PlankDir {None, Hor, Ver, Crs};



// Bridge type
public enum BridgeType {None, Static, Bridge, Plank, Stairs};

// Rock type
public enum RockType {None, Solid, Mined};

// Rune types
public enum RuneType {  None, Boss, Museum, Must, Forbid, Arrow, Chevron, Tapa,
                        Pin, Power, Gate, Box, Exits, Halved, Ditto, Mirror, Knight,
                        ArrowSingle, Eye, Island};
public enum TapaType {  T1, T2, T3, T4, T5, T6, T7, T8, T11, T12, T13, T14, T15,
                        T22, T23, T24, T33, T111, T112, T113, T122, T1111}
public enum PinType {Start, End};
public enum PowerType {Source, Sink};
public enum MuseumType {LastTree, Hammers, SecurityEye, SomethingElse};
public enum BossType {LPiece, Elephant, Panda, Chicken, Dolphin, SomethingElse};


public enum RuneState {Fail, Succeed, BecomeFail, BecomeSucceed};

// For pathing a bridge layout
public enum FloorType {None, Floor, Stairs, Plank};

// Zone areas - Used for recording the area in which a rift is located
// And for showing how far a player is complete with that area
public enum Biome {Canopy, Isles, Canyon, Museum, Plaza};
public enum SubBiome {Area1, Area2, Area3}

// Different animation modes for player movement
public enum AnimationMode {None, Idle, Walking, Running, Building, Blocked, Scared, Angry, Hang, Climb, Scramble};

// Camera angle
public enum CameraAngle_Distance {VeryClose, Close, Med, Far, VeryFar, Comet};
public enum CameraAngle_Side {SideLeft, Left, Centre, Right, SideRight, LeftOblique, RightOblique};
public enum CameraAngle_Pitch {TopDown, Steep, Shallow, Horizontal, Rift};

// Characters
public enum CharacterName {Aeo, PC, Fisher, Name4, Buck, Vampire, Chicken, Panda, Name9};

// Artifacts
public enum ArtifactState {World, Museum};

public static class RL
{
    // Dictionary of colour values
    public static Dictionary <ColourType, Color> colourType_To_Colour;
    public static Dictionary <DirType, Vector3> dir_To_Vect;
    public static Dictionary <Vector3, DirType> vect_To_Dir;
    public static Dictionary <DirType, float> dir_To_Unit;
    // Dictionary of Tapa values
    public static Dictionary<int, List<int>> dict_TapaIndex;

    public static void DeclareFunction()
    {
        // Create a dictionary for transferring movement cardninals into vector directions
        dir_To_Vect = new Dictionary <DirType, Vector3>();
        dir_To_Vect.Add(DirType.None, Vector3.zero);
        dir_To_Vect.Add(DirType.N, Vector3.forward);
        dir_To_Vect.Add(DirType.E, Vector3.right);
        dir_To_Vect.Add(DirType.S, Vector3.back);
        dir_To_Vect.Add(DirType.W, Vector3.left);
        dir_To_Vect.Add(DirType.U, Vector3.up);
        dir_To_Vect.Add(DirType.D, Vector3.down);

        // Turn the cardinal direction into a unit distance value (If card and vert units are different)
        dir_To_Unit = new Dictionary <DirType, float>();
        dir_To_Unit.Add(DirType.None, 0f);
        dir_To_Unit.Add(DirType.N, RL_V.cardUnit);
        dir_To_Unit.Add(DirType.E, RL_V.cardUnit);
        dir_To_Unit.Add(DirType.S, RL_V.cardUnit);
        dir_To_Unit.Add(DirType.W, RL_V.cardUnit);
        dir_To_Unit.Add(DirType.U, RL_V.vertUnit);
        dir_To_Unit.Add(DirType.D, RL_V.vertUnit);

        // Create a dictionary for transferring vector3 directions into movement cardinals
        vect_To_Dir = new Dictionary<Vector3, DirType>();
        vect_To_Dir.Add(Vector3.zero, DirType.None);
        vect_To_Dir.Add(Vector3.forward, DirType.N);
        vect_To_Dir.Add(Vector3.right, DirType.E);
        vect_To_Dir.Add(Vector3.back, DirType.S);
        vect_To_Dir.Add(Vector3.left, DirType.W);
        vect_To_Dir.Add(Vector3.up, DirType.U);
        vect_To_Dir.Add(Vector3.down, DirType.D);

        // Tapa indexes
        dict_TapaIndex = new Dictionary<int, List<int>>();
        dict_TapaIndex.Add(0, new List<int>{1});
        dict_TapaIndex.Add(1, new List<int>{2});
        dict_TapaIndex.Add(2, new List<int>{3});
        dict_TapaIndex.Add(3, new List<int>{4});
        dict_TapaIndex.Add(4, new List<int>{5});
        dict_TapaIndex.Add(5, new List<int>{6});
        dict_TapaIndex.Add(6, new List<int>{7});
        dict_TapaIndex.Add(7, new List<int>{8});
        dict_TapaIndex.Add(8, new List<int>{1, 1});
        dict_TapaIndex.Add(9, new List<int>{1, 2});
        dict_TapaIndex.Add(10, new List<int>{1, 3});
        dict_TapaIndex.Add(11, new List<int>{1, 4});
        dict_TapaIndex.Add(12, new List<int>{1, 5});
        dict_TapaIndex.Add(13, new List<int>{2, 2});
        dict_TapaIndex.Add(14, new List<int>{2, 3});
        dict_TapaIndex.Add(15, new List<int>{2, 4});
        dict_TapaIndex.Add(16, new List<int>{3, 3});
        dict_TapaIndex.Add(17, new List<int>{1, 1, 1});
        dict_TapaIndex.Add(18, new List<int>{1, 1, 2});
        dict_TapaIndex.Add(19, new List<int>{1, 1, 3});
        dict_TapaIndex.Add(20, new List<int>{1, 2, 2});
        dict_TapaIndex.Add(21, new List<int>{1, 1, 1, 1});

        // Create a colour library
        colourType_To_Colour = new Dictionary<ColourType, Color>();
        colourType_To_Colour.Add(ColourType.White, new Color(1.00f, 1.00f, 1.00f, 1.00f));
        colourType_To_Colour.Add(ColourType.Cream, new Color(1.00f, 0.94f, 0.81f, 1.00f));
    }

    // Can't remember what this is useful for, but it works, so
    public static T SafeDestroy<T>(T obj) where T : Object
    {
        if (Application.isEditor)
            Object.DestroyImmediate(obj);
        else
            Object.Destroy(obj);
        
        return null;
    }
}