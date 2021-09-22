using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////
// DATA CLASSES //
//////////////////

//////////////////
// PATH CLASSES //
//////////////////

// The truth class - This information will be stored in an array
// We'll use this to check the level solutions and for the trellis too

public class GridElement
{
    // Is this piece considered a floor
    public bool isPath;
    // A path, but with a little more detail
    public FloorType floorType;
    public PlankDir plankDir;

    public GridElement()
    {
        isPath = false;
        floorType = FloorType.None;
        plankDir = PlankDir.None;
    }

    public GridElement(bool pBool)
    {
        isPath = pBool;
    }

    public GridElement(FloorType pFloorType)
    {
        isPath = true;
        floorType = pFloorType;
    }
    public GridElement(PlankDir pPlankDir)
    {
        isPath = true;
        floorType = FloorType.Plank;
        plankDir = pPlankDir;
    }
}

///////////////////////
// RIFT DATA CLASSES //
///////////////////////

// Data for rift internals
[System.Serializable]
public class RiftData
{
    public int uniqueID;
    public RiftState riftState;
    public BoardType boardType;
    public int[] gs;
    public int[] pieces;
    public string riftName;
    // Reset, Start and End positions
    public int[] resetPos;
    public int[] startPos;
    public int[] endPos;
    public DirType exitDir;
    public string comment;

    public RiftData(    int pUniqueID, RiftState pRiftState, BoardType pBoardType, int[] pgs, int[] pPieces, Vector3 pResetPos, Vector3 pStartPos, Vector3 pEndPos,
                        string pRiftName, DirType pExitDir, string pComment)
    {
        uniqueID = pUniqueID;
        riftState = pRiftState;
        boardType = pBoardType;
        gs = pgs;
        pieces = pPieces;
        resetPos = RL_F.V3_IA(pResetPos);
        startPos = RL_F.V3_IA(pStartPos);
        endPos = RL_F.V3_IA(pEndPos);
        riftName = pRiftName;
        exitDir = pExitDir;
        comment = pComment;
    }
}

// Rift Unique IDs
[System.Serializable]
public class UniqueID
{
    public int uniqueIDTotal;

    public void Increment()
    {
        uniqueIDTotal += 1;
    }
}


// Rift Level data
[System.Serializable]
public class Rift_LevelData
{
    public RiftData riftData;
    public RuneData_AsSer[] arrayOf_RuneData;
    public RockData_AsSer[] arrayOf_RockData;

    public Rift_LevelData(RiftData pRiftData, RuneData_AsSer[] pArrayOf_RuneData, RockData_AsSer[] pArrayOf_RockData)
    {
        riftData = pRiftData;
        arrayOf_RuneData = pArrayOf_RuneData;
        arrayOf_RockData = pArrayOf_RockData;
    }
}

// Used by a grid slot to record what type of bridge is here
public class BridgeData
{
    // pos[0] - layer 'ly', pos[1] - x, pos[2] - y
    public int[] pos;
    public RiftObj parent;
    public BridgeObj bridgeObj;
    public BridgeType bridgeType;
    public PlankDir plankDir;
    
    // Empty bridge type
    public BridgeData(int[] pPos)
    {
        pos = pPos;
        bridgeType = BridgeType.None;
    }

    // User defined bridge
    public BridgeData(int[] pPos, RiftObj pParent, BridgeObj pBridgeObj, PlankDir pPlankDir)
    {
        pos = pPos;
        parent = pParent;
        bridgeObj = pBridgeObj;
        plankDir = pPlankDir;

        // Icon types
        bridgeType = pBridgeObj.bridgeType;
    }

    // Creates a deep copy, rather than a reference
    public BridgeData (BridgeData copy_BridgeData)
    {
        pos = copy_BridgeData.pos;
        parent = copy_BridgeData.parent;
        bridgeObj = copy_BridgeData.bridgeObj;
        bridgeType = copy_BridgeData.bridgeType;
        plankDir = copy_BridgeData.plankDir;
    }
}

// Used by a grid slow to record what type of rune is here
public class RuneData
{
    // This used in game, for live checking of the runeState
    // 0 - Rule broke, 1 - Rule success, 2 (10) - Rule has change from success to failed, 3 - Rule has changed from failed to success
    public RuneState runeState = RuneState.Fail;

    // pos[0] - layer 'ly', pos[1] - x, pos[2] - y
    public int[] pos;
    public RiftObj parent;
    public RuneObj runeObj;
    public RuneType runeType;
    public DirType dirType;
    public int numberType;
    public TapaType tapaType;
    public PinType pinType;
    public PowerType powerType;
    public MuseumType museumType;
    public BossType bossType;
    public bool powerMode;
    public bool ignoreIcon;

    // Each grid position will store one bridge and one icon.
    // If there are no icons in this grid position, we mark is as 'Empty'
    public RuneData(int[] pPos)
    {
        pos = pPos;
        runeType = RuneType.None;
    }

    // User defined lot
    public RuneData(int[] pPos, RiftObj pParent, RuneObj pRuneObj)
    {
        pos = pPos;
        parent = pParent;
        runeObj = pRuneObj;

        // Rune types
        runeType = runeObj.runeType;
        dirType = runeObj.dirType;
        numberType = runeObj.numberType;
        tapaType = runeObj.tapaType;
        pinType = runeObj.pinType;
        powerType = runeObj.powerType;
        powerMode = runeObj.powerMode;
        museumType = runeObj.museumType;
        bossType = runeObj.bossType;
        ignoreIcon = runeObj.ignoreIcon;
    }
}

[System.Serializable]
public class RuneData_AsSer
{
    // pos[0] - layer 'ly', pos[1] - x, pos[2] - y
    public int[] pos;
    public RuneType runeType;
    public DirType dirType;
    public int numberType;
    public TapaType tapaType;
    public PinType pinType;
    public PowerType powerType;
    public MuseumType museumType;
    public BossType bossType;
    public bool powerMode;
    public bool ignoreIcon;

    // User defined lot
    public RuneData_AsSer(RuneObj runeObj)
    {
        pos = runeObj.arrPos;

        // Rune types
        runeType = runeObj.runeType;
        dirType = runeObj.dirType;
        numberType = runeObj.numberType;
        tapaType = runeObj.tapaType;
        pinType = runeObj.pinType;
        powerType = runeObj.powerType;
        powerMode = runeObj.powerMode;
        museumType = runeObj.museumType;
        bossType = runeObj.bossType;
        ignoreIcon = runeObj.ignoreIcon;
    }
}

public class RockData
{
    // This position is stored in RockSpace
    // pos[0] - layer 'ly + 1', pos[1] - x, pos[2] - y
    public int[] pos_RS;
    public RockObj rockObj;
    // Collider size
    public float[] size;
    public bool isMineable;
    public RockType rockType;

    public RockData(int[] pPos)
    {
        pos_RS = pPos;
        rockType = RockType.None;
    }

    public RockData(int[] pPos, RockObj pRockObj)
    {
        rockObj = pRockObj;
        pos_RS = pPos;
        BoxCollider boxCollider = pRockObj.GetComponent<BoxCollider>();
        size = new float[3] {boxCollider.size.x, boxCollider.size.y, boxCollider.size.z};
        isMineable = pRockObj.isMineable;
        rockType = pRockObj.rockType;
    }

    public RockData (RockData rockData)
    {
        rockObj = rockData.rockObj;
        pos_RS = rockData.pos_RS;
        size = rockData.size;
        isMineable = rockData.isMineable;
        rockType = rockData.rockType;
    }
}

[System.Serializable]
public class RockData_AsSer
{
    // This position is stored in RockSpace
    // pos[0] - layer 'ly + 1', pos[1] - x, pos[2] - y
    public int[] pos_RS;
    // Collider size
    public float[] size;
    public bool isMineable;
    public RockType rockType;

    public RockData_AsSer(int[] pPos)
    {
        pos_RS = pPos;
        rockType = RockType.None;
    }

    public RockData_AsSer(int[] pPos, RockObj rockObj)
    {
        pos_RS = pPos;
        BoxCollider boxCollider = rockObj.GetComponent<BoxCollider>();
        size = new float[3] {boxCollider.size.x, boxCollider.size.y, boxCollider.size.z};
        isMineable = rockObj.isMineable;
        rockType = rockObj.rockType;
    }

    public RockData_AsSer (RockData rockData)
    {
        pos_RS = rockData.pos_RS;
        size = rockData.size;
        isMineable = rockData.isMineable;
        rockType = rockData.rockType;
    }
}

public class LiftData
{
    public Transform relativeTransform;
    public Vector3 player_RelativePosition;

    public LiftData(Transform pTransform, Vector3 pPlayer_RelativePosition)
    {
        relativeTransform = pTransform;
        player_RelativePosition = pPlayer_RelativePosition;
    }
}

/////////////
// WARPING //
/////////////

public class Warp
{
    public int index;
    public string name;
    public Sprite icon;
    public bool isDiscovered;  // Warps aren't accessible until they've been discovered

    // Warp point based off of current warpSO
    public Warp(WarpSO warpSO, int pIndex)
    {
        index = pIndex;
        name = warpSO.name;
        icon = warpSO.icon;
        isDiscovered = false;
    }

    // Empty warp point
    public Warp()
    {
        isDiscovered = false;
    }
}

///////////////
// LIGHTNING //
///////////////

// This will become an SO, which I can use to trigger
// The conditions for a scene
public class LightingArea
{
    public Color colourA;
    public Color colourB;
    public float lightIntensity;
}

//////////
// MISC //
//////////

[System.Serializable]
public class ListOf_PoweredMembers
{
    public List<Sc_Cont_Anim> listOf_Cont_Anim;
    public List<RuneObj> listOf_RuneObj;
}

[System.Serializable]
public class ListOf_ListOf_PoweredMembers
{
    public List<ListOf_PoweredMembers> listOf_ListOf_PoweredMembers;
}