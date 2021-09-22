using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

// This is an individual puzzle in the game world

public class RiftObj : MonoBehaviour
{
    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    [HideInInspector] public GM gM;
    [HideInInspector] public RL_M rL_M;
    [HideInInspector] public PD pD;
    [HideInInspector] public AC aC;
    [HideInInspector] public Sc_Player player;

    public BoxCollider areaCollider;
    public Transform resetPoint;
    public Transform startPoint;
    public Transform endPoint;
    public GameObject obj_GridLines;
    [SerializeField] public GameObject[] arrayOf_GridLines;
    [SerializeField] public MeshRenderer[] arrayOf_GridLines_MR;


    ///////////////////
    // USER SETTINGS //
    ///////////////////

    // Each puzzle will have a unique ID to separate from other puzzles
    // Starts as -1. -1 means it hasn't been assigned
    // Once the number has been assigned, it isn't changed
    public int uniqueID = -1;

    [SerializeField]
    // User set level name
    public string riftName;
    
    public RiftState riftState;

    public CameraAngle_Pitch cameraAngle_Pitch = CameraAngle_Pitch.Rift;

    // Whether this is a normal of boss board
    public BoardType boardType;
    // The area where this rift is located
    public Biome biome;
    // Bridge material type (Stone or metal - Metal is conductive)
    public BridgeMaterial bridgeMaterial = BridgeMaterial.Stone;

    // Exit direction - The player can leave through this direction without penalty
    public DirType exitDir;

    [Range(1, 10)]
    public int gsly = 1;     // Layers
    [Range(1, 25)]
    public int gsx = 5;     // Width
    [Range(1, 25)]
    public int gsy = 5;     // Length

    public int bridgePieces_Max = 10;
    public int bridgePieces_Cur = 0;
    public int plankPieces_Max = 2;
    public int plankPieces_Cur = 0;
    public int minePieces_Max = 0;
    public int minePieces_Cur = 0;

    // Ovverride the rift camera to focus somewhere else
    public Camera overrideCamera;

    // List of external items to activate, deactivate
    public List<SinkRune_Trigger> listOf_SinkRune_Triggers;
    public List<Sc_Cont_Anim> listOf_Anim_Trigger;
    public ListOf_ListOf_PoweredMembers poweredMembers;
    public List<Sc_Cont_Anim> listOf_SolvedState_Anim_Trigger;

    [TextArea]
    public string comment = "No Comment.";

    ///////////////////
    // INTERNAL DATA //
    ///////////////////

    // Rift data
    
    [HideInInspector] public bool isSolvedOnce = false;
    [HideInInspector] public bool isSolvedNow = false;

    // Item data
    // List of rocks
    public RockData[,,] arrayOf_RockData;
    public List<RockData> listOf_RockData;
    // List of runes
    public RuneData[,,] arrayOf_RuneData;
    public List<RuneData> listOf_RuneData;
    // List of bridges/bridge items
    public BridgeData[,,] arrayOf_BridgeData;
    public bool piecePlaced = false;   // Record if this rift just placed a piece

    // This list if passed from the PD and allows the riftObj to remember which icons
    // Are solved or not
    [HideInInspector]
    public List<RuneData> listOf_FaultyRunes;
    [HideInInspector]
    public RiftBox riftBox;

    // For mutli-rift puzzles
    [HideInInspector] public int[] trellisOffset;
    public BridgeData[,,] arrayOf_Trellis_BridgeData;     // If multiple rifts are joined together
    public RuneData[,,] arrayOf_Trellis_RuneData;
    public GridElement[,,] arrayOf_GridElement_TrellisSpace;
    [HideInInspector] public int[] trellis_gs;
    public List<RiftObj> listOf_AdjoiningRifts;

    // For Vanilla trellis
    public PathElement[,,] arrayOf_PathElement_TrellisSpace;
    [HideInInspector] public Dictionary <int[], Trellis> pos_To_Trellis;
    [HideInInspector] public List<Segment> listOf_Segments = new List<Segment>();
    [HideInInspector] public List<Island> listOf_Islands = new List<Island>();

    // For Plank trellis
    public PathElement[,,] arrayOf_PathElement_TrellisSpace_Plank;
    [HideInInspector] public Dictionary <int[], Trellis> pos_To_Trellis_Plank;
    [HideInInspector] public List<Segment> listOf_Segments_Plank = new List<Segment>();
    [HideInInspector] public List<Island> listOf_Islands_Plank = new List<Island>();
   

    public void Start()
    {
        WorldReferences();

        // Eventually, this will not need to be here, but for now I'm not sure how to change this for the main game
        // Ideally, I want to perform as few setup actions as possible
        Setup_RiftData_Editor();

        // Here, we collect any 'Static' bridge objects'
        Collect_StaticBridges();

        // Finally, we check if this rift is in the solved state
        //pD.Check_IfRift_Solved(this);
    }

    void WorldReferences()
    {
        // World references
        gM = GM.Instance;
        rL_M = gM.GetComponent<RL_M>();
        pD = PD.Instance;
        aC = AC.Instance;

        player = Sc_Player.Instance;

        // Add self to the list of RiftObj
        pD.listOf_RiftObjs.Add(this);
    }

    void InitialiseLists()
    {
        listOf_RockData = new List<RockData>();
        listOf_RuneData = new List<RuneData>();
        listOf_FaultyRunes = new List<RuneData>();

        arrayOf_RockData = new RockData[gsly + 1, gsx, gsy];
        arrayOf_RuneData = new RuneData[gsly, gsx, gsy];
        arrayOf_BridgeData = new BridgeData[gsly, gsx, gsy];

        for (int ly = 0; ly < gsly; ly++)
        {
            for (int j = 0; j < gsy; j++)
            {
                for (int i = 0; i < gsx; i++)
                {
                    int[] temp_Pos = new int[3] {ly, i, j};
                    arrayOf_RuneData[ly, i, j] = new RuneData(temp_Pos);
                    arrayOf_BridgeData[ly, i, j] = new BridgeData(temp_Pos);
                }
            }
        }

        // Rock data has a slightly larger grid, an additional layer value
        for (int ly = 0; ly < gsly + 1; ly++)
        {
            for (int j = 0; j < gsy; j++)
            {
                for (int i = 0; i < gsx; i++)
                {
                    int[] temp_Pos = new int[3] {ly, i, j};
                    arrayOf_RockData[ly, i, j] = new RockData(temp_Pos);
                }
            }
        }
    }

    // NOTE: Could make this something that occurs in the editor, doesn't need to happen on start
    // Find if there are any bridge objects in the game area that are 'Static'
    // Add them to the rift array
    void Collect_StaticBridges()
    {
        // Grab all the static bridge objects
        BridgeObj[] arrayOf_BridgeObj = GetComponentsInChildren<BridgeObj>();
        List<BridgeObj> listOf_StaticBridgeObj = new List<BridgeObj>();
        foreach (BridgeObj bridgeObj in arrayOf_BridgeObj)
        {
            // If this object is a static element
            if (bridgeObj.bridgeType == BridgeType.Static)
            {
                listOf_StaticBridgeObj.Add(bridgeObj);
            }
        }

        // Run through each static bridge obj
        foreach (BridgeObj staticBridgeObj in listOf_StaticBridgeObj)
        {
            // Store it's location inside the main bridge array
            int[] intArr = RL_F.V3DFV3_IA(staticBridgeObj.transform.position, transform.position);

            // I need to initialise the bridgeObj data
            // This just requires sending the parent rift to the BridgeObj script
            staticBridgeObj.parent = this;

            // Create a new bridgeData item and add it to the array
            arrayOf_BridgeData[intArr[0], intArr[1], intArr[2]] = new BridgeData(intArr, this, staticBridgeObj, PlankDir.None);
        }

        // These bridge objects bypass the spawn pool
    }

    // Reset any variables that change during player action
    public void Reset_Variables()
    {
        piecePlaced = false;
    }

    // This version of the setup script runs in the editor
    public void Setup_RiftData_Editor()
    {
        InitialiseLists();

        // Setup the number of bridge pieces
        bridgePieces_Cur = bridgePieces_Max;
        plankPieces_Cur = plankPieces_Max;
        minePieces_Cur = minePieces_Max;

        // Make sure this rift is added to the list of adjoining rifts
        if (!listOf_AdjoiningRifts.Contains(this))
        {
            listOf_AdjoiningRifts.Add(this);
        }

        // Go through each RuneObj that is a child of this RiftObj and collect them
        RuneObj[] runeChildren = transform.GetComponentsInChildren<RuneObj>();
        foreach (RuneObj runeChild in runeChildren)
        {
            // Ignore any runes that use the ignore flag
            if (!runeChild.ignoreIcon)
            {
                // Have the runeChild recognise itself
                runeChild.PassStart(this);

                // Check to make sure this exists within the game grid
                if (RL_F.C_IG(new int[3] {gsly, gsx, gsy}, runeChild.runeData.pos))
                {
                    // Grab the runeData from that child and overwrite the same position in the grid
                    arrayOf_RuneData[runeChild.runeData.pos[0], runeChild.runeData.pos[1], runeChild.runeData.pos[2]] =
                        runeChild.runeData;
                    // Also store it as a list
                    listOf_RuneData.Add(runeChild.runeData);
                }
                else
                {
                    Debug.Log("Rune of Type: " + runeChild.runeData.runeType + " does not exist in the Rift box.");
                }
            }
        }

        // Go through each RockPbj that is a child of this RiftObj and collect them
        RockObj[] rockChildren = transform.GetComponentsInChildren<RockObj>();
        foreach (RockObj rockChild in rockChildren)
        {
            // Have the runeChild recognise itself
            rockChild.PassStart(this);

            // Check to make sure this exists within an extended version of the game grid
            // RockSpace has an additional layer
            if (RL_F.C_IG(new int[3] {gsly + 1, gsx, gsy}, rockChild.rockData.pos_RS))
            {
                // Grab the rockData from that child and overwrite the same position in the grid in RockSpace
                arrayOf_RockData[rockChild.rockData.pos_RS[0], rockChild.rockData.pos_RS[1], rockChild.rockData.pos_RS[2]] =
                    rockChild.rockData;
                // Also store it as a list
                listOf_RockData.Add(rockChild.rockData);
            }
            else
            {
                Debug.Log("Rock of Type: " + rockChild.rockData.rockType + " does not exist in the Rift box.");
            }
        }
    }

    /////////////////
    // PUZZLE META //
    /////////////////

    // When the player resets a puzzle
    public void Action_Reset()
    {
        // Reset all the bridge components, send any bridge pieces back to the pool
        for (int ly = 0; ly < gsly; ly++)
        {
            for (int j = 0; j < gsy; j++)
            {
                for (int i = 0; i < gsx; i++)
                {
                    // If this bridgeData is not None or Static, we need to return it to it's relevent pool
                    if (arrayOf_BridgeData[ly, i, j].bridgeType != BridgeType.None && arrayOf_BridgeData[ly, i, j].bridgeType != BridgeType.Static)
                    {
                        // All of the funcationality for changing the riftObj
                        // Is stored in the Return_Bridge function
                        Return_GenObj(new int[3] {ly, i, j});
                    }

                    // Reset any mined rocks to their solid state
                    if (arrayOf_RockData[ly, i, j].rockType != RockType.None)
                    {
                        // Run the function for resetting a rockObj
                        UnMine_RockObj(new int[3] {ly, i, j});
                    }
                }
            }
        }
    }
    
    /////////////
    // BRIDGES //
    /////////////

    // This function places a single bridgeObj into the level gameworld of any type
    public void Place_GenObj(int[] place_Pos, BridgeType bridgeType, PlankDir plankDir = PlankDir.None)
    {
        // Instantiate a bridge piece
        GameObject go_BridgeObj = RL_Pool.Instance.SpawnFromPool(bridgeType, RL_F.IADFV3_V3(place_Pos, transform.position), Quaternion.identity, transform);
        BridgeObj sc_BridgeObj = go_BridgeObj.GetComponent<BridgeObj>();

        // Make a SFX
        AC.Instance.SFX_Bridge_Place();

        // Update the info inside the BridgeObj
        sc_BridgeObj.bridgeType = bridgeType;
        sc_BridgeObj.plankDir = plankDir;
        sc_BridgeObj.parent = this;

        // Update the info inside the BridgeData
        // Grab the bridgeData as a temp variable
        BridgeData temp_BridgeData = arrayOf_BridgeData[place_Pos[0], place_Pos[1], place_Pos[2]];
        // Store a reference to the bridgeObj
        temp_BridgeData.bridgeObj = sc_BridgeObj;
        // Store the bridge type
        temp_BridgeData.bridgeType = bridgeType;
        // Store the plank direction
        temp_BridgeData.plankDir = plankDir;
        // Return the bridgeData item
        arrayOf_BridgeData[place_Pos[0], place_Pos[1], place_Pos[2]] = temp_BridgeData;

        // Decrement the relevent totals
        Spend_Pieces_Total(bridgeType, plankDir);

        // Here, I can confirm or deny if we're able to check the puzzle solution now
        // Currently this just confirms after a placement of every piece
        piecePlaced = true;
    }

    // Place a Bridge Object
    public void Place_BridgeObj(int[] dest_Pos)
    {
        // If there is nothing in our way, place the Bridge
        // If there is any other bridgeObj here, remove it and place down the specified bridge
        if (arrayOf_BridgeData[dest_Pos[0], dest_Pos[1], dest_Pos[2]].bridgeType != BridgeType.None)
        {
            // Return the object that was here
            Return_GenObj(dest_Pos);
        }

        // Place the bridge
        Place_GenObj(dest_Pos, BridgeType.Bridge);
    }

    // Place a plank object
    public void Place_PlankObj(int[] dest_Pos, DirType moveCard)
    {
        // If there is nothing in our way, place the plank
        // If there is any other bridgeObj here, remove it and place down the specified plank
        bool isCross = false;
        if (arrayOf_BridgeData[dest_Pos[0], dest_Pos[1], dest_Pos[2]].bridgeType != BridgeType.None)
        {
            // If this item is of type plank, we know (Due to the way checking is setup), that this newly placed plank will be a crs
            if (arrayOf_BridgeData[dest_Pos[0], dest_Pos[1], dest_Pos[2]].bridgeType == BridgeType.Plank)
            {
                isCross = true;
            }

            // Return the object that was here
            Return_GenObj(dest_Pos);
        }

        // Calculate the plank orientation, if it's not a cross
        PlankDir plankDir = PlankDir.None;
        if (isCross)
        {
            plankDir = PlankDir.Crs;
        }
        else if (moveCard == DirType.N || moveCard == DirType.S)
        {
            plankDir = PlankDir.Ver;
        }
        else if (moveCard == DirType.E || moveCard == DirType.W)
        {
            plankDir = PlankDir.Hor;
        }

        // Place the plank
        Place_GenObj(dest_Pos, BridgeType.Plank, plankDir);
    }

    // This function places stairs into the level gameworld
    public void Place_Stairs(int[] dest_Pos, DirType moveCard)
    {
        // When moving up, we check the destination position, as the player will already have moved
        // When moving down, we want to check the position 1 unit above the player
        int temp_yInt = 0;
        if (moveCard == DirType.D)
        {
            temp_yInt = 1;
        }

        // First, we check if there exists a bridge item in this pos
        if (arrayOf_BridgeData[dest_Pos[0] + temp_yInt, dest_Pos[1], dest_Pos[2]].bridgeType == BridgeType.Bridge ||
            arrayOf_BridgeData[dest_Pos[0] + temp_yInt, dest_Pos[1], dest_Pos[2]].bridgeType == BridgeType.Plank)
        {
            Return_GenObj(new int[] {dest_Pos[0] + temp_yInt, dest_Pos[1], dest_Pos[2]});
        }

        // If we're moving down 
        if (moveCard == DirType.D)
        {
            // We can just place a stairs here
            Place_GenObj(new int[3] {dest_Pos[0] + temp_yInt, dest_Pos[1], dest_Pos[2]}, BridgeType.Stairs);

            // First, we check if there is a floor already here. In which case, we place nothing
            Ray ray_Lower = new Ray(RL_F.IADFV3_V3(dest_Pos, transform.position), RL.dir_To_Vect[DirType.D]);
            LayerMask maskObstacle = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Obstacle));
            RaycastHit[] temp_HitLower = Physics.RaycastAll(ray_Lower, RL_V.cardUnit, maskObstacle);
            // If there is nothing in the way
            if (temp_HitLower.Length == 0)
            {
                // We check if there is a rock already here, in which case we also place nothing
                if (arrayOf_RockData[dest_Pos[0], dest_Pos[1], dest_Pos[2]].rockType != RockType.Solid)
                {
                    // We want to check if a bridge already exists here. If not, we'll need to place one
                    if (arrayOf_BridgeData[dest_Pos[0], dest_Pos[1], dest_Pos[2]].bridgeType == BridgeType.None)
                    {
                        Place_GenObj(dest_Pos, BridgeType.Bridge);
                    }
                }
            }
        }
        // If we're moving up
        else if (moveCard == DirType.U)
        {
            // We check if there is a rock already here, in which case we place nothing
            if (arrayOf_RockData[dest_Pos[0], dest_Pos[1], dest_Pos[2]].rockType != RockType.Solid)
            {

                // Place some stairs
                Place_GenObj(new int[3] {dest_Pos[0] + temp_yInt, dest_Pos[1], dest_Pos[2]}, BridgeType.Stairs);
            }
        }
    }

    // Mine a rockObj
    public void Mine_RockObj(int[] dest_pos)
    {
        // RockObjs exist in a slightly different space, to we add 1 to the layer value
        // Grab the rockData
        RockData rockData = arrayOf_RockData[dest_pos[0] + 1, dest_pos[1], dest_pos[2]];

        // Update the rockdata
        rockData.rockType = RockType.Mined;
        // Update the info inside the rockObj
        rockData.rockObj.rockType = RockType.Mined;
    
        // Return the rockData
        arrayOf_RockData[dest_pos[0] + 1, dest_pos[1], dest_pos[2]] = rockData;

        // Decrement the relevent totals
        minePieces_Cur -= 1;
    }
    
    // Return a single obj at a coordinate position into its relevent pool spawner
    public void Return_GenObj(int[] pBrgPos)
    {
        // Grab a reference to the bridgeData as a temp variable
        BridgeData temp_BridgeData = arrayOf_BridgeData[pBrgPos[0], pBrgPos[1], pBrgPos[2]];

        // Put the bridge pack into the spawner
        RL_Pool.Instance.ReturnToPool(temp_BridgeData.bridgeType, temp_BridgeData.bridgeObj.gameObject);

        // Note the plankType
        PlankDir plankDir = temp_BridgeData.plankDir;

        // Increment the current bridge totals
        Return_Pieces_Total(temp_BridgeData.bridgeType, plankDir);
        
        // Create new, blank BridgeData as replacement
        temp_BridgeData = new BridgeData(pBrgPos);
        // Overwrite the bridgeData item
        arrayOf_BridgeData[pBrgPos[0], pBrgPos[1], pBrgPos[2]] = temp_BridgeData;
    }

    public void UnMine_RockObj(int[] dest_pos)
    {
        // Grab a reference to the rockData as a temp variable
        // Remember to convert to rockspace
        RockData rockData = arrayOf_RockData[dest_pos[0] + 1, dest_pos[1], dest_pos[2]];

        // We only perform changes if this rock is of type mined
        if (rockData.rockType == RockType.Mined)
        {
            // Set the rock to it's unmined state
            rockData.rockType = RockType.Solid;
            // Same for the obj
            rockData.rockObj.rockType = RockType.Solid;

            // Overwrite the rockData item
            // in Rockspace
            arrayOf_RockData[dest_pos[0] + 1, dest_pos[1], dest_pos[2]] = rockData;

            // Increment the relevent totals
            minePieces_Cur += 1;
        }
    }

    ///////////////////
    // TRELLIS GRAPH //
    ///////////////////

    // Calculate the network connections or 'Trellis' of the bridges
    // Inside this rift
    // This will be useful for some Runes later on

    # region Trellis Classes

    public class Segment
    {
        public List<int[]> listOf_Pos;
        public int island = -1;

        public Segment()
        {
            listOf_Pos = new List<int[]>();
            island = -1;
        }
    }

    public class Trellis
    {
        public List<int> listOf_Segments;

        public Trellis()
        {
            listOf_Segments = new List<int>();
        }
    }
    
    public class PathElement
    {
        public List<DirType> listOf_PathDirs;

        public PathElement()
        {
            listOf_PathDirs = new List<DirType>();
        }
    }

    public class Island
    {
        public List<int> listOf_Segment;
        public List<int[]> listOf_Pos;

        public Island()
        {
            listOf_Segment = new List<int>();
            listOf_Pos = new List<int[]>();
        }
    }

    # endregion
    
    public void Update_TrellisSpace()
    {
        Update_TrellisSpace_BridgeStairs();
        //Update_TrellisSpace_Planks();
    }

    // This is a compendium of what the trellis space functions are doing
    public void Update_TrellisSpace_BridgeStairs()
    {
        // Ensure you perform Calculate_TrellisSpace() before using the following functions
        Calculate_TrellisSpace();
        Calculate_TrellisPath(ref arrayOf_PathElement_TrellisSpace, false);
        Calculate_TrellisSegments(ref arrayOf_PathElement_TrellisSpace, ref listOf_Segments, ref pos_To_Trellis, false, ref listOf_Islands);
    }

    // This version uses a grid that includes planks as paths
    public void Update_TrellisSpace_Planks()
    {
        // Ensure you perform Calculate_TrellisSpace() before using the following functions
        Calculate_TrellisSpace();
        Calculate_TrellisPath(ref arrayOf_PathElement_TrellisSpace_Plank, true);
        Calculate_TrellisSegments(ref arrayOf_PathElement_TrellisSpace_Plank, ref listOf_Segments_Plank, ref pos_To_Trellis_Plank, true, ref listOf_Islands_Plank);
    }

    /////////////////////////
    // SETUP TRELLIS SPACE //
    /////////////////////////

    public void Calculate_TrellisSpace()
    {
        // We want to create a truth table for the layout of all the squares in the grid
        // That we deem part of a 'path'.
        // This includes Bridges, Stairs and Rocks (The top parts)

        // Find the minimum and maximum vector positions of all the adjoined rifts
        // Start with some initial conditions
        Vector3 minVec = transform.position;
        Vector3 maxVec = transform.position + new Vector3(gsx - 1, gsly - 1, gsy - 1);
        // Go through each rift, updating the vectors with the minimal or maximum values
        foreach (RiftObj riftObj in listOf_AdjoiningRifts)
        {
            minVec = Vector3.Min(minVec, riftObj.transform.position);
            maxVec = Vector3.Max(maxVec, riftObj.transform.position + new Vector3(riftObj.gsx - 1, riftObj.gsly - 1, riftObj.gsy - 1));
        }

        // Create a new path array for these new sizes
        trellis_gs = RL_F.V3DFV3_IA(maxVec + Vector3.one, minVec);

        // Generate a null version of each array used this algorithm
        // For each rift
        foreach (RiftObj tRiftObj in listOf_AdjoiningRifts)
        {
            tRiftObj.arrayOf_GridElement_TrellisSpace = new GridElement[trellis_gs[0], trellis_gs[1], trellis_gs[2]];
            for (int ly = 0; ly < trellis_gs[0]; ly++)
            {
                for (int j = 0; j < trellis_gs[2]; j++)
                {
                    for (int i = 0; i < trellis_gs[1]; i++)
                    {
                        int[] tPos = new int[3] {ly, i, j};
                        tRiftObj.arrayOf_GridElement_TrellisSpace[ly, i, j] = new GridElement();
                    }
                }
            }
        }

        ///////////////////////////////////////
        // RULES FOR FLOORS IN TRELLIS SPACE //
        ///////////////////////////////////////

        // Create a basic array in trellis space of all the floors and stairs in
        // Each parth of the trellis array
        foreach (RiftObj tRiftObj in listOf_AdjoiningRifts)
        {
            // Calculate the offset
            tRiftObj.trellisOffset = RL_F.V3DFV3_IA(tRiftObj.transform.position, minVec);
            // We use this when storing a pathData into the new grid array
        }
            
        foreach (RiftObj tRiftObj in listOf_AdjoiningRifts)
        {
            // Update the GridElement array of all rift components
            foreach (RiftObj ttRiftObj in listOf_AdjoiningRifts)
            {
                PD_Check.Return_ArrayOf_GridElement(ref tRiftObj.arrayOf_GridElement_TrellisSpace, ttRiftObj, trellis_gs, ttRiftObj.trellisOffset);
            }
        }
    }

    public void Calculate_TrellisPath(ref PathElement[,,] t_arrayOf_PathElement_TrellisSpace, bool isPlank)
    {
        t_arrayOf_PathElement_TrellisSpace = new PathElement[trellis_gs[0], trellis_gs[1], trellis_gs[2]];

        for (int ly = 0; ly < trellis_gs[0]; ly++)
        {
            for (int j = 0; j < trellis_gs[2]; j++)
            {
                for (int i = 0; i < trellis_gs[1]; i++)
                {
                    int[] tPos = new int[3] {ly, i, j};
                    t_arrayOf_PathElement_TrellisSpace[ly, i, j] = new PathElement();
                }
            }
        }

        //////////////////////////////////////////
        // FIND DIR NEIGHBOURS IN TRELLIS SPACE //
        //////////////////////////////////////////

        // Go through trellis space and store what path directions we can go in, based on which things
        // We can walk on
        for (int ly = 0; ly < trellis_gs[0]; ly++)
        {
            for (int i = 0; i < trellis_gs[1]; i++)
            {
                for (int j = 0; j < trellis_gs[2]; j++)
                {
                    // If this element is not None
                    if (arrayOf_GridElement_TrellisSpace[ly, i, j].floorType != FloorType.None)
                    {
                        // Check each direction and figure out how many neighbours this position has

                        // Store our position in rift space
                        int[] curPos_TS = new int[3] {ly, i, j};

                        // Check each neighbour, to see if we can move there
                        foreach (DirType dirType in Enum.GetValues(typeof(DirType)))
                        {
                            int[] nghPos_TS = RL_F.U_PS_DT(curPos_TS, dirType);

                            // Check we're still inside the grid if we were to move in this direction
                            if (RL_F.C_IG(trellis_gs, nghPos_TS))
                            {
                                FloorType cur_FloorType = arrayOf_GridElement_TrellisSpace[curPos_TS[0], curPos_TS[1], curPos_TS[2]].floorType;
                                FloorType ngh_FloorType = arrayOf_GridElement_TrellisSpace[nghPos_TS[0], nghPos_TS[1], nghPos_TS[2]].floorType;

                                // For cardinal directions
                                if (dirType == DirType.N || dirType == DirType.E || dirType == DirType.S || dirType == DirType.W)
                                {
                                    // Check if there is a floor in this direction
                                    if (ngh_FloorType != FloorType.None)
                                    {
                                        // If we're not in plank mode, we ignore planks
                                        if (!isPlank && ngh_FloorType != FloorType.Plank)
                                        {
                                            // Therefore, we can add this direction to our movement list
                                            t_arrayOf_PathElement_TrellisSpace[curPos_TS[0], curPos_TS[1], curPos_TS[2]].listOf_PathDirs.Add(dirType);
                                        }
                                        // If we are in plank mode, we need to perform some checks
                                        if (isPlank)
                                        {
                                            PlankDir tPlankDir = arrayOf_GridElement_TrellisSpace[curPos_TS[0], curPos_TS[1], curPos_TS[2]].plankDir;

                                            // This check must remain true for movement on a plank to occur
                                            bool checkForPlanks = true;

                                            // For ourselves, we can only move if the plank is parallel
                                            if (cur_FloorType == FloorType.Plank)
                                            {
                                                // If we can't perform this move, we cancel the check bool
                                                if (!RL_F.C_MC_PD(dirType, arrayOf_GridElement_TrellisSpace[curPos_TS[0], curPos_TS[1], curPos_TS[2]].plankDir))
                                                {
                                                    checkForPlanks = false;
                                                }
                                            }

                                            // For our neighbour, we can only move if the plank is parallel
                                            if (ngh_FloorType == FloorType.Plank)
                                            {
                                                // If we can't perform this move, we cancel the check bool
                                                if (!RL_F.C_MC_PD(dirType, arrayOf_GridElement_TrellisSpace[nghPos_TS[0], nghPos_TS[1], nghPos_TS[2]].plankDir))
                                                {
                                                    print ("Plank neighbour is NOT allowed: " + dirType + "-" + tPlankDir);
                                                    checkForPlanks = false;
                                                }
                                                else
                                                {
                                                    print ("Plank neighout is allowed");
                                                }
                                            }
                                            
                                            // If the checkbool still holds
                                            if (checkForPlanks)
                                            {
                                                // We can add that direction to our list of movements, regardless
                                                t_arrayOf_PathElement_TrellisSpace[curPos_TS[0], curPos_TS[1], curPos_TS[2]].listOf_PathDirs.Add(dirType);
                                            }
                                        }
                                    }
                                }
                                
                                // For down direction
                                if (dirType == DirType.D)
                                {
                                    // For downward movement, opur current location must be of type stairs
                                    if (cur_FloorType == FloorType.Stairs)
                                    {
                                        // If there is a floor below us, planks aren't allowed
                                        if (ngh_FloorType != FloorType.None && ngh_FloorType != FloorType.Plank)
                                        {
                                            // Therefore, we can add this direction to our movement list
                                            t_arrayOf_PathElement_TrellisSpace[curPos_TS[0], curPos_TS[1], curPos_TS[2]].listOf_PathDirs.Add(dirType);
                                        }
                                    }
                                }

                                // For up direction
                                if (dirType == DirType.U)
                                {
                                    // For upward movement, we can't move up from a plank
                                    if (cur_FloorType != FloorType.Plank)
                                    {
                                        // There must be a stairs above us
                                        if (arrayOf_GridElement_TrellisSpace[nghPos_TS[0], nghPos_TS[1], nghPos_TS[2]].floorType == FloorType.Stairs)
                                        {
                                            // Therefore, we can add this direction to our movement list
                                            t_arrayOf_PathElement_TrellisSpace[curPos_TS[0], curPos_TS[1], curPos_TS[2]].listOf_PathDirs.Add(dirType);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        ////////////////////////
        // TRELLIS ALGORITHMS //
        ////////////////////////
    }

    public void Calculate_TrellisSegments(ref PathElement[,,] t_ArrayOf_PathElement, ref List<Segment> t_ListOf_Segments,
                                            ref Dictionary<int[], Trellis> t_Pos_To_Trellis, bool isPlank, ref List<Island> t_listOf_Islands)
    {
        # region Trellis Calculation

        t_ListOf_Segments = new List<Segment>();
        // We use this equality comparer as int[] are treated as separated objects and we want them to
        // Be the same object when the array values are the same
        t_Pos_To_Trellis = new Dictionary<int[], Trellis>(new MyEqualityComparer());

        // Go through each path in the scene
        for (int ly = 0; ly < trellis_gs[0]; ly++)
        {
            for (int i = 0; i < trellis_gs[1]; i++)
            {
                for (int j = 0; j < trellis_gs[2]; j++)
                {
                    // Only consider path elements that can be walked on
                    // Or are plank dependent
                    if ((isPlank && arrayOf_GridElement_TrellisSpace[ly, i, j].floorType != FloorType.None) ||
                            (!isPlank && arrayOf_GridElement_TrellisSpace[ly, i, j].floorType != FloorType.None && arrayOf_GridElement_TrellisSpace[ly, i, j].floorType != FloorType.Plank))
                    {
                        int[] curPos = new int[3] {ly, i, j};

                        ////////////////////////
                        // COLLECT NEIGHBOURS //
                        ////////////////////////

                        // Create a new Trellis item with this pos
                        t_Pos_To_Trellis.Add(curPos, new Trellis());

                        // Check a thing exists
                        //print(t_Pos_To_Trellis.ContainsKey(new int[] { 1, 10, 1 }));

                        // We need to grab the list of neighbour pos first and count them, before we can do anything
                        List<int[]> listOf_Neighbour_Pos = new List<int[]>();

                        // Go through each neighbour in our neighbour array
                        foreach (DirType dirType in t_ArrayOf_PathElement[curPos[0], curPos[1], curPos[2]].listOf_PathDirs)
                        {
                            // Add that position offset to our neighbour positions list
                            listOf_Neighbour_Pos.Add(RL_F.U_PS_DT(curPos, dirType));
                        }

                        //////////////////////
                        // CHECK NEIGHBOURS //
                        //////////////////////

                        // If this bridge piece has two neighbours or less, our response is different
                        if (listOf_Neighbour_Pos.Count <= 2)
                        {
                            // There are several situations we need to check here
                            // 1. If neighbours are not prepped yet
                            // 2. If a neighbour is part of a segment
                            // 3. If both neighbours are part of different segments
                            // 4. If both neighbours are part of the same segment

                            ///////////////////////////
                            // GENERATE SEGMENT LIST //
                            ///////////////////////////

                            List<int> listOf_NeighbourSegments = new List<int>();

                            // Check if these neighbours already have a Trellis attached to them
                            for (int p = 0; p < listOf_Neighbour_Pos.Count; p++)
                            {
                                int[] neigh_Pos = listOf_Neighbour_Pos[p];

                                // Does this pos exist in our dictionary?
                                if (t_Pos_To_Trellis.ContainsKey(neigh_Pos))
                                {
                                    // If the neighbour only has 1 segment, we can add this segment for reference
                                    if (t_Pos_To_Trellis[neigh_Pos].listOf_Segments.Count == 1)
                                    {
                                        // Add a reference to this segment
                                        int seg = t_Pos_To_Trellis[neigh_Pos].listOf_Segments[0];
                                        listOf_NeighbourSegments.Add(seg);
                                    }
                                    else
                                    {
                                        // Go through each segment value and add yourself to the first segment that is empty
                                        foreach (int seg in t_Pos_To_Trellis[neigh_Pos].listOf_Segments)
                                        {
                                            // The first one of these, which only contains 1 value, we add ourselves to it
                                            if (t_ListOf_Segments[seg].listOf_Pos.Count == 1)
                                            {
                                                // Add a reference to this segment
                                                listOf_NeighbourSegments.Add(seg);
                                                // As soon as we find our first empty segment, we leave this forloop
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            ////////////////////////
                            // CHECK SEGMENT LIST //
                            ////////////////////////

                            // 1. if neighbours aren't prepped yet or there are no neighbours
                            if (listOf_NeighbourSegments.Count == 0)
                            {
                                Add_New_Segment(curPos, ref t_ListOf_Segments, ref t_Pos_To_Trellis);
                            }
                            // 2. If a neighbour is part of a segment
                            else if (listOf_NeighbourSegments.Count == 1)
                            {
                                for (int p = 0; p < listOf_Neighbour_Pos.Count; p++)
                                {
                                    int[] neigh_Pos = listOf_Neighbour_Pos[p];

                                    // Does this pos exist in our dictionary?
                                    if (t_Pos_To_Trellis.ContainsKey(neigh_Pos))
                                    {
                                        // If the neighbour only has 1 segment, we can add this segment for reference
                                        if (t_Pos_To_Trellis[neigh_Pos].listOf_Segments.Count == 1)
                                        {
                                            // Add our segment to our trellis value
                                            // And add our pos to the list of segments
                                            int seg = t_Pos_To_Trellis[neigh_Pos].listOf_Segments[0];
                                            Add_To_Segment(curPos, seg, ref t_ListOf_Segments, ref t_Pos_To_Trellis);
                                        }
                                        else
                                        {
                                            // Go through each segment value and add yourself to the segment that is 'empty'
                                            foreach (int seg in t_Pos_To_Trellis[neigh_Pos].listOf_Segments)
                                            {
                                                // The first one of these, which only contains 1 value, we add ourselves to it
                                                if (t_ListOf_Segments[seg].listOf_Pos.Count == 1)
                                                {
                                                    // Add our segment to our trellis value
                                                    // And add our bridgeData to the list of segments
                                                    Add_To_Segment(curPos, seg, ref t_ListOf_Segments, ref t_Pos_To_Trellis);
                                                    // As soon as we find our first empty segment, we leave this forloop
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // 3. If both neighbours are part of different segments
                            else if (listOf_NeighbourSegments[0] != listOf_NeighbourSegments[1])
                            {
                                // Differentiate the smaller and larger segments
                                int smallerSeg = listOf_NeighbourSegments[0];
                                int largerSeg = listOf_NeighbourSegments[1];
                                if (listOf_NeighbourSegments[0] > listOf_NeighbourSegments[1])
                                {
                                    smallerSeg = listOf_NeighbourSegments[1];
                                    largerSeg = listOf_NeighbourSegments[0];
                                }

                                // Add our smaller segment to our trellis value
                                // And add our pos to the smaller value in the list of segments
                                Add_To_Segment(curPos, smallerSeg, ref t_ListOf_Segments, ref t_Pos_To_Trellis);

                                // Grab a list of all the pos inside the larger segment
                                List<int[]> tListOf_Pos = t_ListOf_Segments[largerSeg].listOf_Pos;
                                // Clear that segment list
                                t_ListOf_Segments[largerSeg].listOf_Pos = new List<int[]>();
                                // Remove the previous segment value from the pos list
                                // And move them into the smaller segment values

                                for (int p = 0; p < tListOf_Pos.Count; p++)
                                {
                                    int[] tPos = tListOf_Pos[p];

                                    t_Pos_To_Trellis[tPos].listOf_Segments.Remove(largerSeg);

                                    // If the smaller segment does not already exist
                                    if (!t_Pos_To_Trellis[tPos].listOf_Segments.Contains(smallerSeg))
                                    {
                                        // Add the smaller segment
                                        t_Pos_To_Trellis[tPos].listOf_Segments.Add(smallerSeg);
                                    }

                                    // If this pos is not already in the smaller segment list
                                    if (!t_ListOf_Segments[smallerSeg].listOf_Pos.Contains(tPos))
                                    {
                                        // Add this pos to the smaller segment
                                        t_ListOf_Segments[smallerSeg].listOf_Pos.Add(tPos);
                                    }
                                }
                            }
                            // 4. If the both neighbours are part of the same segment
                            else if (listOf_NeighbourSegments[0] == listOf_NeighbourSegments[1])
                            {
                                // Store a reference to the seg
                                int seg = listOf_NeighbourSegments[0];

                                // Add our segment to our trellis value
                                // And add our curPos to the list of segments
                                Add_To_Segment(curPos, seg, ref t_ListOf_Segments, ref t_Pos_To_Trellis);
                            }
                        }
                    
                        // For some cases it will be 3+ connections
                        else
                        {
                            // Go through each connection
                            for (int p = 0; p < listOf_Neighbour_Pos.Count; p++)
                            {
                                int[] neigh_Pos = listOf_Neighbour_Pos[p];

                                // Does this pos already exist in a segment
                                if (t_Pos_To_Trellis.ContainsKey(neigh_Pos))
                                {
                                    // If the neighbour only has 1 segment, we can add ourself straight to it
                                    if (t_Pos_To_Trellis[neigh_Pos].listOf_Segments.Count == 1)
                                    {
                                        // Add our segment to our trellis value
                                        // And add our bridgeData to the list of segments
                                        int seg = t_Pos_To_Trellis[neigh_Pos].listOf_Segments[0];
                                        Add_To_Segment(curPos, seg, ref t_ListOf_Segments, ref t_Pos_To_Trellis);
                                    }
                                    else
                                    {
                                        // Go through each segment value and add yourself to the segment that is empty
                                        foreach (int seg in t_Pos_To_Trellis[neigh_Pos].listOf_Segments)
                                        {
                                            // The first one of these, which only contains 1 value, we add ourselves to it
                                            if (t_ListOf_Segments[seg].listOf_Pos.Count == 1)
                                            {
                                                // Add our segment to our trellis value
                                                // And add our bridgeData to the list of segments
                                                Add_To_Segment(curPos, seg, ref t_ListOf_Segments, ref t_Pos_To_Trellis);
                                                // As soon as we find our first empty segment, we leave this forloop
                                                break;
                                            }
                                        }
                                    }
                                }
                                // Create a new segment and add self to that segment
                                else
                                {
                                    Add_New_Segment(curPos, ref t_ListOf_Segments, ref t_Pos_To_Trellis);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Don't clean up the segment list, it's useful for how
        // The islands setup uses it
        int totalSeg = 0;
        foreach (Segment seg in t_ListOf_Segments)
        {
            if (seg.listOf_Pos.Count > 0)
            {
                totalSeg += 1;
            }
        }

        //print ("Total Segments: " + totalSeg);
        
        # endregion
        
        ///////////////////////
        // ISLANDS ALGORITHM //
        ///////////////////////

        # region Island Calculation

        // Go through every segment and Path item in each segement
        // If said segment contains a duplicate pos, we add that poisition to our island int

        // Each island stores a list of segments and a list of pos
        t_listOf_Islands = new List<Island>();

        // Create the checked list outside the while function
        List<int> listOf_Checked_Segments = new List<int>();

        for (int s = 0; s < t_ListOf_Segments.Count; s++)
        {
            // Ignore any segments that don't contain any positions
            if (t_ListOf_Segments[s].listOf_Pos.Count > 0)
            {
                // Check to see if this segment isn't already part of an island
                if (t_ListOf_Segments[s].island == -1)
                {
                    // First of all, create a new island
                    t_listOf_Islands.Add(new Island());
                    // Store the segment value inside the island parameter
                    int i = t_listOf_Islands.Count - 1;
                    t_ListOf_Segments[s].island = i;
                    t_listOf_Islands[i].listOf_Segment.Add(s);

                    // Create a new checking list
                    List<int> listOf_Checking_Segments = new List<int>();
                    // Add this element to our checking list
                    listOf_Checking_Segments.Add(s);
                    // We go through our checking list
                    while (listOf_Checking_Segments.Count > 0)
                    {
                        // Store this element
                        int seg = listOf_Checking_Segments.First();
                        
                        // Go through every position attached to this segment
                        foreach (int[] pos in t_ListOf_Segments[seg].listOf_Pos)
                        {
                            // Go through every segment in those pos
                            foreach (int seg2 in t_Pos_To_Trellis[pos].listOf_Segments)
                            {
                                // If this item hasn't already been checked
                                if (!listOf_Checked_Segments.Contains(seg2))
                                {
                                    // If this item isn't geared up to be checked
                                    if (!listOf_Checking_Segments.Contains(seg2))
                                    {
                                        // Add it to the checking list
                                        listOf_Checking_Segments.Add(seg2);
                                        // Add it's island value
                                        t_ListOf_Segments[seg2].island = i;
                                        // Add this segment to the island list
                                        t_listOf_Islands[i].listOf_Segment.Add(seg2);
                                    }
                                }
                            }
                        }

                        // Remove this element from the checking list
                        listOf_Checking_Segments.Remove(seg);
                        listOf_Checked_Segments.Add(seg);
                    }
                }
                else
                {
                    // We don't do anything, we'll have already finished working with the segment in the code above
                }
            }
        }
        
        //print ("Total Islands: " + t_listOf_Islands.Count);

        # endregion        
    }

    ///////////////////////
    // TRELLIS FUNCTIONS //
    ///////////////////////

    void Add_New_Segment(int[] pCurPos, ref List<Segment> t_ListOf_Segments, ref Dictionary<int[], Trellis> t_Pos_To_Trellis)
    {
        // Create a new segment
        t_ListOf_Segments.Add(new Segment());
        // Store that segment variable
        int seg = t_ListOf_Segments.Count - 1;
        // Store ourselves in the segment list
        t_ListOf_Segments[seg].listOf_Pos.Add(pCurPos);
        // Store that segment variable with ourselves
        t_Pos_To_Trellis[pCurPos].listOf_Segments.Add(seg);
        // We don't add any information to the neighbour bridgeData, they will do that themselves
    }

    void Add_To_Segment(int[] pCurPos, int pSeg, ref List<Segment> t_ListOf_Segments, ref Dictionary<int[], Trellis> t_Pos_To_Trellis)
    {
        // Add our segment to our trellis value
        // And add our pos to the list of segments
        t_Pos_To_Trellis[pCurPos].listOf_Segments.Add(pSeg);
        t_ListOf_Segments[pSeg].listOf_Pos.Add(pCurPos);
    }

    //////////////
    // TRIGGERS //
    //////////////

    // Whenever the riftObj's state is updated
    public void Action_Upon_UpdateState()
    {
        Check_SolveState_Triggers();

        Check_RiftBox();
    }
    
    // Updates animation states if riftObj is solved
    public void Check_SolveState_Triggers()
    {
        foreach (Sc_Cont_Anim animTrigger in listOf_SolvedState_Anim_Trigger)
        {
            animTrigger.Switch(isSolvedNow);
        }
    }

    public void Check_RiftBox()
    {
        if (riftBox != null)
        {
            riftBox.Update_RuneData();
        }
    }

    /////////////
    // VISUALS //
    /////////////

    // Using only a position, update the surrounding meshes
    public void Update_SurroundingMeshes(int[] pos)
    {
        List<BridgeObj_Mesh> listOf_Ref_NeighbourMeshes = new List<BridgeObj_Mesh>();

        // Go through nearby layers
        for (int k = pos[0] - 1; k <= pos[0] + 1; k++)
        {
            // Go through each row, x
            for (int i = pos[1] - 1; i <= pos[1] + 1; i ++)
            {
                // And each column, y
                for (int j = pos[2] - 1; j <= pos[2] + 1; j++)
                {
                    // Make sure we're inside trellis space grid
                    if (RL_F.C_IG(trellis_gs, new int[3] {k + trellisOffset[0], i + trellisOffset[1], j + trellisOffset[2]}))
                    {
                        // Ignore the centre square
                        if (!(k == pos[0] && i == pos[1] && j == pos[2]))
                        {
                            // If a Path exists here, we add it to the binary
                            if (arrayOf_GridElement_TrellisSpace[k + trellisOffset[0], i + trellisOffset[1], j + trellisOffset[2]].isPath)
                            {
                                // Store a reference to this neighbour, if this neighbour is a bridgeObj
                                // To do this, use a reverse lookup script for the trellis space setup
                                foreach (RiftObj tRiftObj in listOf_AdjoiningRifts)
                                {
                                    // Adjust positioning by leaving one riftspace and entering another
                                    int[] posNS = new int[3]{k + trellisOffset[0] - tRiftObj.trellisOffset[0], i + trellisOffset[1] - tRiftObj.trellisOffset[1], j + + trellisOffset[2] - tRiftObj.trellisOffset[2]};
                                    // Go through each rift, until you find the one that this position belongs to
                                    if (RL_F.C_IG(new int[3] {tRiftObj.gsly, tRiftObj.gsx, tRiftObj.gsy}, posNS))
                                    {
                                        // Does the bridgeData in that array have a type that is Bridge or Stairs?
                                        if (tRiftObj.arrayOf_BridgeData[posNS[0], posNS[1], posNS[2]].bridgeType == BridgeType.Bridge ||
                                                tRiftObj.arrayOf_BridgeData[posNS[0], posNS[1], posNS[2]].bridgeType == BridgeType.Stairs)
                                        {                                                
                                            // Add that bridge mesh to our list to update
                                            listOf_Ref_NeighbourMeshes.Add(tRiftObj.arrayOf_BridgeData[posNS[0], posNS[1], posNS[2]].bridgeObj.GetComponent<BridgeObj_Mesh>());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Update each previous bridge mesh
        foreach (BridgeObj_Mesh temp_Script in listOf_Ref_NeighbourMeshes)
        {
            // If they still exist
            if (temp_Script != null)
                temp_Script.UpdateMesh(0);
        }
    }

    public void Update_Visuals()
    {
        // Change the material of a bridge based on if it's part of an island icon group
        Update_Visuals_Bridge();

        // Update the grid colours based on player layer
        Update_Visuals_Grid();
    }
    
    public void Update_Visuals_Bridge()
    {
        // We want to store all the positions that are connected to this rune in trellis space
        List<int[]> listOf_AdjointPos_TS = new List<int[]>();

        // Go through each Rune
        foreach (RuneData runeData in listOf_RuneData)
        {
            // Find any Rune of type Power
            // That is a power source
            if (runeData.runeType == RuneType.Power && runeData.powerType == PowerType.Source)
            {
                // Check there is a trellis path here
                int[] powerRunePos_TS = new int[3] {runeData.pos[0] + trellisOffset[0], runeData.pos[1] + trellisOffset[1], runeData.pos[2] + trellisOffset[2]};
                if (pos_To_Trellis.ContainsKey(powerRunePos_TS))
                {
                    // We want to store all the positions
                    // In their relevent space in each correct rift obj
                    listOf_AdjointPos_TS.Add(powerRunePos_TS);

                    // Grab any segment value for this position
                    int seg = pos_To_Trellis[powerRunePos_TS].listOf_Segments[0];

                    // Grab the island value
                    int t_Island = listOf_Segments[seg].island;
                    
                    // Use this island to grab a list of segments
                    List<int> t_ListOf_Segments = listOf_Islands[t_Island].listOf_Segment;

                    // Use the segments to grab all the positions in trellis space
                    // This will create duplicates, but I can't be bothered to optimise this yet
                    for (int s = 0; s < t_ListOf_Segments.Count; s++)
                    {
                        listOf_AdjointPos_TS.AddRange(listOf_Segments[t_ListOf_Segments[s]].listOf_Pos);
                    }
                }
            }
        }

        // Reset all the materials to their off position
        foreach (RiftObj tRiftObj in listOf_AdjoiningRifts)
        {
            for (int ly = 0; ly < tRiftObj.gsly; ly++)
            {
                for (int i = 0; i < tRiftObj.gsx; i++)
                {
                    for (int j = 0; j < tRiftObj.gsy; j++)
                    {
                        // For bridges that aren't noneType
                        if (tRiftObj.arrayOf_BridgeData[ly, i, j].bridgeType != BridgeType.None)
                        {
                            // Set the material to off
                            //print ("Bridge Type: " + tRiftObj.arrayOf_BridgeData[ly, i, j].bridgeObj.bridgeType);
                            //print ("Help: " + tRiftObj.arrayOf_BridgeData[ly, i, j].bridgeObj.GetComponent<BridgeObj_Mesh>().bridgeObj);
                            tRiftObj.arrayOf_BridgeData[ly, i, j].bridgeObj.GetComponent<BridgeObj_Mesh>().MaterialDictionary(false);
                        }
                    }
                }
            }
        }

        // Go through each adjoint pos
        foreach (int[] adjPos_TS in listOf_AdjointPos_TS)
        {
            // I have to figure out which offset is the correct offset to use
            // Run through each adjoined rift
            foreach (RiftObj tRiftObj in listOf_AdjoiningRifts)
            {
                // Check if the offset position inside their riftspace exists within their grid
                int[] adjPos = new int[3] {adjPos_TS[0] - tRiftObj.trellisOffset[0], adjPos_TS[1] - tRiftObj.trellisOffset[1], adjPos_TS[2] - tRiftObj.trellisOffset[2]};

                if (RL_F.C_IG(new int[3]{tRiftObj.gsly, tRiftObj.gsx, tRiftObj.gsy}, adjPos_TS))
                {
                    // Check this position exists within the adjoint list, using trellis space
                    if (listOf_AdjointPos_TS.Contains(adjPos_TS))
                    {
                        // Then, we can grab the bridge item
                        // Set the bridge material to be electric
                        tRiftObj.arrayOf_BridgeData[adjPos[0], adjPos[1], adjPos[2]].bridgeObj.GetComponent<BridgeObj_Mesh>().MaterialDictionary(true);
                    }
                }
            }
        }
    }

    public void Update_Visuals_Grid()
    {
        // Make all the grids grey
        for (int i = 0; i < arrayOf_GridLines.Length; i++)
        {
            arrayOf_GridLines[i].GetComponent<MeshRenderer>().material = rL_M.gridLines[0];
        }

        //Debug.Log(RL_F.V3DFV3_IA(player.player_Collider.transform.position, this.transform.position)[0]);
        // Get players current Y position
        int lyVal = RL_F.V3DFV3_IA(player.player_Collider.transform.position, this.transform.position)[0];
        
        // Colourise player's current position
        if (boardType == BoardType.Normal)
        {
            arrayOf_GridLines[lyVal].GetComponent<MeshRenderer>().material = rL_M.gridLines[1];
        }
        else if (boardType == BoardType.Museum)
        {
            arrayOf_GridLines[lyVal].GetComponent<MeshRenderer>().material = rL_M.gridLines[2];
        }
        else if (boardType == BoardType.Boss)
        {
            arrayOf_GridLines[lyVal].GetComponent<MeshRenderer>().material = rL_M.gridLines[3];
        }
        
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    public void Spend_Pieces_Total(BridgeType bridgeType = BridgeType.None, PlankDir plankDir = PlankDir.None)
    {
        if (bridgeType == BridgeType.Bridge)
        {
            bridgePieces_Cur -= 1;
        }
        else if (bridgeType == BridgeType.Stairs)
        {
            bridgePieces_Cur -= 1;
            //plankPieces_Cur -= 1;
        }
        else if (bridgeType == BridgeType.Plank)
        {
            if (plankDir == PlankDir.Crs)
            {
                // Cross planks cost 2
                plankPieces_Cur -= 2;
            }
            else
            {
                plankPieces_Cur -= 1;
            }
            
        }
    }

    public void Return_Pieces_Total(BridgeType bridgeType, PlankDir plankDir)
    {
        if (bridgeType == BridgeType.Bridge)
        {
            bridgePieces_Cur += 1;
        }
        else if (bridgeType == BridgeType.Stairs)
        {
            bridgePieces_Cur += 1;
            //plankPieces_Cur += 1;
        }
        else if (bridgeType == BridgeType.Plank)
        {

            if (plankDir == PlankDir.Crs)
            {
                // Cross planks cost 2
                plankPieces_Cur += 2;
            }
            else
            {
                plankPieces_Cur += 1;
            }
        }
    }

    public int Return_PiecesTotal_Cur()
    {
        int piecesTotal_Cur = bridgePieces_Cur + plankPieces_Cur + minePieces_Cur;

        return piecesTotal_Cur; 
    }

    public class MyEqualityComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(int[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }

    /////////////
    // TRIGGER //
    /////////////

    // Activate or deactivate the rift
    public void Toggle_Active_Rift(bool toggle)
    {
        // We update active rift inside the PD
        if (toggle)
        {
            pD.Toggle_Active_Rift(this);
        }
        else
        {
            pD.Toggle_Active_Rift(null);
        }
    }
}
