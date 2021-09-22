using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This is a controller for the Warping mechanism

public class WC : MonoBehaviour
{
    #region Singleton

    public static WC Instance {get; private set; }

    private void Awake()
    {
        Instance = this;
    }
        
    #endregion
    
    // World References
    Sc_SortInput sI;
    Sc_Camera_Brain camera_Brain;

    private bool isActive = false;
    int activeWarpPoint = 0;     // The active warpPoint we're currently at
    Vector2 activeWarpSelection = new Vector2(0, 0);        // The actively selected icon
    int warpMax = 25;  // Current max number of warp points.
    int row = 0;
    float stepValue = 0f;
    [HideInInspector] public Vector3 warpTo = Vector3.zero;
    Coroutine cr;   // Plays when we're performing a warp
    
    public Sprite emptyIcon;
    public GameObject warpMenu;
    public Image warpMenu_Cover;
    public Image warpMenu_Cover_Small;
    public GameObject panel_WarpIcons;
    public GameObject panel_WarpIcon;
    public RectTransform panel_WarpSelect;
    public TextMeshProUGUI text_WarpName;
    
    public WarpSO[] arrayOf_WarpSO;
    [HideInInspector] public Warp[] arrayOf_Warps;
    [HideInInspector] public WarpPoint[] arrayOf_WarpPoints;
    [HideInInspector] public Image[] arrayOf_WarpIcons;

    void Start()
    {
        WorldReferences();

        InitialiseLists();
        Setup_Warp_Database();
        Setup_UI_WarpGrid();
        
        Update_UI_WarpGrid();
    }

    void WorldReferences()
    {
        sI = GM.Instance.GetComponent<Sc_SortInput>();
        camera_Brain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Sc_Camera_Brain>();
    }

    void InitialiseLists()
    {
        // Calculate row length
        row = (int)Mathf.Ceil(Mathf.Sqrt(warpMax));
        arrayOf_Warps = new Warp[row*row];
        arrayOf_WarpPoints = new WarpPoint[row*row];
        arrayOf_WarpIcons = new Image[row*row];
    }

    // Setup the warp Database
    public void Setup_Warp_Database()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < row; j++)
            {
                // Check to see if the WarpSO exists
                if (arrayOf_WarpSO[i + row*j] != null)
                {
                    // Update the database based on the warpSO
                    arrayOf_Warps[i + row*j] = new Warp(arrayOf_WarpSO[i + row*j], i + row*j);
                }
                else
                {
                    // Create a blank warp
                    arrayOf_Warps[i + row*j] = new Warp();
                }
            }
        }
    }

    // Setup the warp grid
    public void Setup_UI_WarpGrid()
    {
        // Step value
        stepValue = 1f/row;

        // Create a grid of icons
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < row; j++)
            {
                // Create a panel
                arrayOf_WarpIcons[i + j*row] = Instantiate(panel_WarpIcon, panel_WarpIcons.transform).GetComponent<Image>();
                // Reposition the icons inside the grid
                arrayOf_WarpIcons[i + j*row].GetComponent<RectTransform>().anchorMin = new Vector2(stepValue * i, stepValue * j);
                arrayOf_WarpIcons[i + j*row].GetComponent<RectTransform>().anchorMax = new Vector2(stepValue * (i + 1), stepValue * (j + 1));
                arrayOf_WarpIcons[i + j*row].GetComponent<RectTransform>().offsetMax = Vector2.zero;
                arrayOf_WarpIcons[i + j*row].GetComponent<RectTransform>().offsetMin = Vector2.zero;
                // Setup the image based on the init warps
                if (arrayOf_WarpSO[i + row*j] != null)
                {
                    arrayOf_WarpIcons[i + row*j].sprite = arrayOf_Warps[i + row*j].icon;
                }
                else
                {
                    arrayOf_WarpIcons[i + row*j].sprite = emptyIcon;
                }
            }
        }
    }

    // Update the UI grid based on which warps have been discovered
    public void Update_UI_WarpGrid()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < row; j++)
            {
                // If the warp has been discovered, paint it white
                if (arrayOf_Warps[i + row*j].isDiscovered)
                {
                    arrayOf_WarpIcons[i + row*j].color = Color.white;

                    // If this is the current warp point, paint it light blue?
                    if (activeWarpPoint == i + row*j)
                    {
                        arrayOf_WarpIcons[i + row*j].color = Color.cyan;
                    }
                }
                // If not, paint it red, with an X? I don't know
                else
                {
                    arrayOf_WarpIcons[i + row*j].color = new Color(0.1f, 0.1f, 0.1f, 1f);
                }
            }
        }
    }

    // Here we record player menu selection
    void Update()
    {
        // Only if the warpMenu is active
        if (isActive)
        {
            // Don't update anything while we're performing a warp
            // Or in play mode
            if (cr == null && !sI.isPlayer)
            {
                // If we're in this menu (I'll add this later)
                if (sI.moveCard_Menu != DirType.None)
                {
                    if (sI.moveCard_Menu == DirType.N)
                    {
                        Update_ItemSelect(Vector2.up);
                    }
                    else if (sI.moveCard_Menu == DirType.E)
                    {
                        Update_ItemSelect(Vector2.right);
                    }
                    else if (sI.moveCard_Menu == DirType.S)
                    {
                        Update_ItemSelect(Vector2.down);
                    }
                    else if (sI.moveCard_Menu == DirType.W)
                    {
                        Update_ItemSelect(Vector2.left);
                    }
                }
                if (sI.undo)
                {
                    // Back out of the warp menu
                    OnDeactivate();
                }
                if (sI.select)
                {
                    OnWarp();
                }
            }
        }
    }

    // If the index changes, update the item we're selecting
    public void Update_ItemSelect(Vector2 dir)
    {
        // We check if we can add in that direction (Movement is not cyclic)
        Vector2 moveTo = activeWarpSelection + dir;
        // We only update play the SFX if the direction has changed
        if (dir != Vector2.zero)
        {
            if (moveTo.x < row && moveTo.x >= 0 && moveTo.y < row && moveTo.y >= 0)
            {
                // SFX
                AC.Instance.OneShot_UI_Keys();

                // We can move to the new selection
                activeWarpSelection = moveTo;
            }
        }

        // Move the reticule
        panel_WarpSelect.anchorMin = new Vector2(stepValue * activeWarpSelection.x, stepValue * activeWarpSelection.y);
        panel_WarpSelect.anchorMax = new Vector2(stepValue * (activeWarpSelection.x + 1), stepValue * (activeWarpSelection.y + 1));
        panel_WarpSelect.offsetMax = Vector2.zero;
        panel_WarpSelect.offsetMin = Vector2.zero;

        // Update the text name
        if (arrayOf_Warps[(int)(activeWarpSelection.x + (row * activeWarpSelection.y))].isDiscovered)
        {
            text_WarpName.color = Color.white;
            text_WarpName.text = arrayOf_Warps[(int)(activeWarpSelection.x + (row * activeWarpSelection.y))].name;
            if (activeWarpPoint == (int)activeWarpSelection.x + (row * activeWarpSelection.y))
            {
                text_WarpName.color = Color.cyan;
            }
        }
        else
        {
            text_WarpName.color = Color.red;
            text_WarpName.text = "No Connection";
        }
    }

    public void OnActivate(int index)
    {
        if (cr == null)
        {
            // We are currently in the warp menu
            isActive = true;

            // Store the active index warp
            activeWarpPoint = index;

            // Disable the player controller
            sI.Action_PlayerControls(false);

            // We check if the current active warpPoint has been discovered for the first time
            if (!arrayOf_Warps[index].isDiscovered)
            {
                // We can activate it and update the icon
                arrayOf_Warps[index].isDiscovered = true;
            }

            Update_UI_WarpGrid();

            // Animate the coroutine
            cr = StartCoroutine(Perform_ZoomIn(true));
        }
    }

    public void OnDeactivate()
    {
        if (cr == null)
        {
            // Deactivate the warp menu
            isActive = false;
            // Animate the coroutine
            cr = StartCoroutine(Perform_ZoomIn(false));
        }
    }

    public void OnWarp()
    {
        // Grab the active selected
        int moveTo = (int)(activeWarpSelection.x + row*activeWarpSelection.y);

        // Update the variable for where we're moving to
        warpTo = arrayOf_WarpPoints[moveTo].transform.position;

        // Check to see this isn't already selected
        if (activeWarpPoint != moveTo)
        {
            // This part becomes a coroutine
            cr = StartCoroutine(Perform_Warp());            
        }
        else
        {
            // Play some SFX

            // Wobbled the icon

        }
    }

    // Used for when the camera component is zooming in on the PC
    public IEnumerator Perform_ZoomIn(bool isZoomIn)
    {
        if (isZoomIn)
        {
            print ("ZOOM IN");
            // If this is out first time on this warp, play a slightly different SFX

            // Reposition the camera
            camera_Brain.Set_Camera_WarpScreen(transform);
            yield return new WaitForSeconds(1.5f);
            // Activate the menu
            warpMenu.SetActive(true);
            // Fade in the warp menu
            yield return Animation_WarpMenuCover(warpMenu_Cover_Small, false);
            print ("ZOOM IN DONE");
        }
        else
        {
            print ("ZOOM OUT");
            // Immediately shutoff the cover
            yield return Animation_WarpMenuCover(warpMenu_Cover_Small, true);
            // Deactivate the menu
            warpMenu.SetActive(false);
            // Reposition the camera
            camera_Brain.Set_Camera_Player();
            print ("ZOOM OUT IS DONE");
            // Enable the player controller
            sI.Action_PlayerControls(true);
        }

        // Nullify the coroutine
        cr = null;

        yield return null;
    }

    public IEnumerator Perform_Warp()
    {
        // SFX
        AC.Instance.OneShot_UI_Enter();

        // Set warping to true
        sI.warp = true;

        // Move the camera in
        print ("Move in camera");
        yield return camera_Brain.Warp_Zoom(true);

        // Change the cover
        print ("Change the cover");
        yield return Animation_WarpMenuCover(warpMenu_Cover, true);

        // Update the UI's index
        // The new activeWarpPoint is the previous activeWarpSelection
        int prv_ActiveWarpPoint = activeWarpPoint;
        activeWarpPoint = (int)(activeWarpSelection.x + (row * activeWarpSelection.y));
        // The new activeWarpSelection is the previous activeWarpPoint
        activeWarpSelection = new Vector2(prv_ActiveWarpPoint % row, Mathf.Floor(prv_ActiveWarpPoint/row));
        Update_UI_WarpGrid();
        Update_ItemSelect(Vector2.zero);
        
        // Call the move action
        print ("Move the player");
        UM.Instance.Action_Move(DirType.None);
        yield return new WaitForSeconds(1f);

        // Send away the cover
        print ("Change the cover");
        yield return Animation_WarpMenuCover(warpMenu_Cover, false);

        // Zoom the camera out
        print ("Zoom out camera");
        yield return camera_Brain.Warp_Zoom(false);

        // Nullify the coroutine
        cr = null;

        yield return null;
    }

    public IEnumerator Animation_WarpMenuCover(Image cover, bool isCovering)
    {
        float tf = 1f;
        if (isCovering)
        {
            tf = 0f;
        }

        for (float t = 0; t < 1f; t += Time.deltaTime * 2f)
        {
            cover.color = new Color(0f, 0f, 1f, Mathf.Abs(tf - t));
            yield return null;
        }

        cover.color = new Color(0f, 0f, 1f, Mathf.Abs(tf - 1f));
        yield return null;
    }
}
