using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is an individual puzzle in the game world

public class RiftObj : MonoBehaviour
{
    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    [HideInInspector] public GM gM;
    [HideInInspector] public PD pD;
    [HideInInspector] public AC aC;
    [HideInInspector] public Sc_Player player;

    public BoxCollider areaCollider;
    public BoxCollider startPoint;


    ///////////////////
    // USER SETTINGS //
    ///////////////////

    [SerializeField]
    public string RiftName;     // User set level name

    [Range(1, 3)]
    public int gsly = 1;     // Layers
    [Range(1, 25)]
    public int gsx = 5;     // Width
    [Range(1, 25)]
    public int gsy = 5;     // Length
    
    public RiftType riftType = RiftType.Isles;

    public int bridgePiecesRemaining = 10;

    ///////////////////
    // INTERNAL DATA //
    ///////////////////

    // Rift data
    public int id = -1;     // Unique puzzle id
    [HideInInspector] public bool isSolvedOnce = false;
    [HideInInspector] public bool isSolvedNow = false;

    // [0] = layer, [1] = x, [2] = y
    public int[] player_RiftPos;   // This stores the player's grid position inside a rift puzzle

    // Item data
    // List of icons
    public IconData[,,] arrayOf_IconData;
    public List<IconData> listOf_IconData;
    // List of bridges/bridge items
    public BridgeData[,,] arrayOf_BridgeData;
    public List<BridgeData> listOf_BridgeData;

    void Start()
    {
        WorldReferences();

        InitialiseLists();

        SetupRift_Data();
        SetupRift_Obj();
    }

    void WorldReferences()
    {
        // World references
        gM = GameObject.FindGameObjectWithTag("GM").GetComponent<GM>();
        pD = GameObject.FindGameObjectWithTag("PD").GetComponent<PD>();
        aC = GameObject.FindGameObjectWithTag("AC").GetComponent<AC>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Sc_Player>();

        // Add self to the list of RiftObj
        pD.listOf_RiftObjs.Add(this);
    }

    void InitialiseLists()
    {
        listOf_IconData = new List<IconData>();
        listOf_BridgeData = new List<BridgeData>();

        arrayOf_IconData = new IconData[gsly, gsx, gsy];
        arrayOf_BridgeData = new BridgeData[gsly, gsx, gsy];

        for (int ly = 0; ly < gsly; ly++)
        {
            for (int j = 0; j < gsy; j++)
            {
                for (int i = 0; i < gsx; i++)
                {
                    arrayOf_IconData[ly, i, j] = new IconData(ly, i, j);
                    arrayOf_BridgeData[ly, i, j] = new BridgeData(ly, i, j);
                }
            }
        }
    }

    // Create the data for storing all the puzzle elements
    void SetupRift_Data()
    {
        // Go through each IconObj that is a child of this RiftObj and collect them
        IconObj[] iconChildren = transform.GetComponentsInChildren<IconObj>();
        foreach (IconObj iconChild in iconChildren)
        {
            // Have the iconChild recognise itself
            iconChild.PassStart(this);

            // Grab the iconData from that child and overwrite the same position in the grid
            arrayOf_IconData[iconChild.iconData.ly, iconChild.iconData.x, iconChild.iconData.y] = iconChild.iconData;
            // Also store it as a list
            listOf_IconData.Add(iconChild.iconData);
        }

        // Go through each BridgeObj as before. This will only be relevent when loading the game in to reply it
        BridgeObj[] bridgeChildren = transform.GetComponentsInChildren<BridgeObj>();
        foreach (BridgeObj bridgeChild in bridgeChildren)
        {
            bridgeChild.PassStart();

            // Store any bridges in their array positions
            arrayOf_BridgeData[bridgeChild.bridgeData.ly, bridgeChild.bridgeData.x, bridgeChild.bridgeData.y] = bridgeChild.bridgeData;
            // And store it inside the list
            listOf_BridgeData.Add(bridgeChild.bridgeData);
        }
    }

    // Instantiate all the game objects
    void SetupRift_Obj()
    {
        // Floor colliders - There may be different colliders for each layer, how to switch between these?


    }
    
    /////////////
    // BRIDGES //
    /////////////

    // This function places a bridge into the level gameworld
    public void Place_Bridge(int[] pos)
    {
        // Instantiate a bridge piece
        GameObject go_BridgeObj = RL_Pool.Instance.SpawnFromPool(RL_Pool.PoolType.Bridge, RL_F.Return_Vector3_Difference(pos, transform.position), Quaternion.identity);
        BridgeObj sc_BridgeObj = go_BridgeObj.GetComponent<BridgeObj>();
        // Start up the bridge
        sc_BridgeObj.PassStart();
        // Store it's bridgeData
        arrayOf_BridgeData[pos[0], pos[1], pos[2]] = sc_BridgeObj.bridgeData;
        listOf_BridgeData.Add(sc_BridgeObj.bridgeData);

        // Decrement the current bridge total
        bridgePiecesRemaining -= 1;
    }

    public void Undo_Bridge()
    {
        // Remove most recent bridge
        
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    public void Update_PlayerRift_Pos(MoveCard pMoveCard)
    {
        if (pMoveCard == MoveCard.N)
            player_RiftPos[2] += 1;
        else if (pMoveCard == MoveCard.E)
            player_RiftPos[1] += 1;
        else if (pMoveCard == MoveCard.S)
            player_RiftPos[2] -= 1;
        else if (pMoveCard == MoveCard.W)
            player_RiftPos[1] -= 1;
        else if (pMoveCard == MoveCard.U)
            player_RiftPos[0] += 1;
        else if (pMoveCard == MoveCard.D)
            player_RiftPos[0] -= 1;
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
            // Grab the player's relative position inside the rift grid
            player_RiftPos = RL_F.Return_IntArray_Difference(player.player_Collider.transform.position, transform.position);

            pD.Toggle_Active_Rift(this);
        }
        else
        {
            pD.Toggle_Active_Rift(null);
        }
    }
}
