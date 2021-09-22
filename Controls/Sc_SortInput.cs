using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Sort and organise player input from controller or keyboard

public class Sc_SortInput : MonoBehaviour
{
    PlayerControls controls;

    // Control Scheme mode
    // 0 - Keyboard and mouse, 1 - Gamepad
    [System.NonSerialized] public int controlScheme = 1;

    //////////////
    // MOVEMENT //
    //////////////

    [System.NonSerialized] public Vector2 move;
    [System.NonSerialized] public Vector2 cameraStick;
    [System.NonSerialized] public Vector2 mousePos;
    [System.NonSerialized] public Vector2 cameraPan;
    [System.NonSerialized] public DirType moveCard;
    
    [System.NonSerialized] public DirType storeMoveCard = DirType.None;
    [System.NonSerialized] public bool isMove = false;
    [System.NonSerialized] public bool toggle = false;
    [System.NonSerialized] public bool stairsUp = false;
    [System.NonSerialized] public bool stairsDown = false;

    [System.NonSerialized] public bool isOptions = false;   // Whether we're in the options menu
    [System.NonSerialized] public bool isPlayer = false;    // Whether we can move the player
    [System.NonSerialized] public bool select = false;
    [System.NonSerialized] public bool start = false;
    [System.NonSerialized] public bool undo = false;
    [System.NonSerialized] public bool reset = false;
    [System.NonSerialized] public bool delete = false;
    [System.NonSerialized] public bool warp = false;

    // UI controls
    [System.NonSerialized] public DirType moveCard_Menu;

    // This will later be moved to options menu
    public bool isHoldMode = true;

    // 'Jeneric' button triggers for held buttons
    // This generic format allows you to add as many buttons as you need
    // And specify any specific time length for repeats.
	[System.NonSerialized]
	public bool[] jen_FinalBool;    // The final button state, the is used by the player
	[System.NonSerialized]
	public bool[] jen_ActiveBool;   // The current button state, from input
	[System.NonSerialized]
	public float[] jen_Timer;       // The current timer value. Activates the button when it reaches zero
    [System.NonSerialized]
    private int jen_Total = 6;      // Number of 'jen' buttons being used.
    // 0 - Undo, 1 - isMove, 2 - Reset, 3 - StairsUp, 4 - StairsDown, 5 - Select

    void Awake()
    {
        // Setup jen buttons
		jen_FinalBool = new bool[jen_Total];
		jen_ActiveBool = new bool[jen_Total];
        jen_Timer = new float[jen_Total];
		for (int i = 0; i < jen_Total; i++)
		{
			jen_FinalBool[i] = false;
			jen_ActiveBool[i] = false;
			jen_Timer[i] = 0.0f;
		}

        // Setup input controls
        controls = new PlayerControls();

        controls.Player.Pause.started += ctx => Action_Pause();

        controls.Player.Select.performed += ctx => jen_ActiveBool[5] = true;
        controls.Player.Select.canceled += ctx => jen_ActiveBool[5] = false;

        controls.Player.Undo.performed += ctx => jen_ActiveBool[0] = true;
        controls.Player.Undo.canceled += ctx => jen_ActiveBool[0] = false;

        controls.Player.Reset.performed += ctx => jen_ActiveBool[2] = true;
        controls.Player.Reset.canceled += ctx => jen_ActiveBool[2] = false;

        // Removed the delete button, I think this will produce better puzzles
        //controls.Player.Delete.started += ctx => Check_HoldOrTap(ref delete, 0);
        //controls.Player.Delete.performed += ctx => Check_HoldOrTap(ref delete, 1);
        //controls.Player.Delete.canceled += ctx => Check_HoldOrTap(ref delete, 2);

        controls.Player.Toggle.started += ctx => Check_HoldOrTap(ref toggle, 0);
        controls.Player.Toggle.performed += ctx => Check_HoldOrTap(ref toggle, 1);
        controls.Player.Toggle.canceled += ctx => Check_HoldOrTap(ref toggle, 2);

        controls.Player.StairsUp.performed += ctx => jen_ActiveBool[3] = true;
        controls.Player.StairsUp.canceled += ctx => jen_ActiveBool[3] = false;

        controls.Player.StairsDown.performed += ctx => jen_ActiveBool[4] = true;
        controls.Player.StairsDown.canceled += ctx => jen_ActiveBool[4] = false;

        controls.Player.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Player.Camera.performed += ctx => cameraStick = ctx.ReadValue<Vector2>();
        controls.Player.Mouse.performed += ctx => mousePos = ctx.ReadValue<Vector2>();
    }

    void OnEnable()
    {
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void Update()
    {
        // If movement is above a certain magnitude, we count it as a move action
        if (move.magnitude >= 0.125f)
        {
            // Store the active jen bool
            jen_ActiveBool[1] = true;

            // Sort vector movement into cardinal movement
            // Find the largest vector value and store that movement cardinal
            if (Mathf.Abs(move.x) >= Mathf.Abs(move.y))
            {
                if (move.x > 0f)
                {
                    moveCard = DirType.E;
                }
                else
                {
                    moveCard = DirType.W;
                }
            }
            else
            {
                if (move.y > 0f)
                {
                    moveCard = DirType.N;
                }
                else
                {
                    moveCard = DirType.S;
                }
            }

            // If the stored movement cardinal has changed, we can reset the timer
            if (storeMoveCard != moveCard)
            {
                jen_Timer[1] = 0.0f;
            }
        }
        else
        {
            // No movement
            moveCard = DirType.None;
            // Store the active jen bool
            jen_ActiveBool[1] = false;
        }

        // Store the current movement cardinal
        storeMoveCard = moveCard;

        // Camera panning
        // For controller mode
        if (controlScheme == 1)
        {
            cameraPan = cameraStick;
        }
        // For mouse mode
        else if (controlScheme == 0)
        {
            cameraPan = new Vector2(Mathf.Clamp(mousePos.x/Screen.width - 0.5f, -1f, 1f), Mathf.Clamp(mousePos.y/Screen.height - 0.5f, -1f, 1f));
        }


        // Update all jen_Bools
		for (int i = 0; i < jen_Total; i++)
		{
			Jen_TimedStateFunction(i);
		}

        // Pass the final bools to the named bools
        undo = jen_FinalBool[0];
        isMove = jen_FinalBool[1];
        reset = jen_FinalBool[2];
        stairsUp = jen_FinalBool[3];
        stairsDown = jen_FinalBool[4];
        select = jen_FinalBool[5];


        // For up and down movement
        if (stairsUp)
        {
            moveCard = DirType.U;
        }
        else if (stairsDown)
        {
            moveCard = DirType.D;
        }

        // For menu items        
        // We can borrow the moveCard until the final step
        if (moveCard != DirType.None && moveCard != DirType.U && moveCard != DirType.D)
        {
            // If the jeneric action is available, perform it
            if (jen_FinalBool[1])
            {
                moveCard_Menu = moveCard;
            }
            else
            {
                moveCard_Menu = DirType.None;
            }
        }
    }

    public void Action_Pause()
    {
        // We can only change the pause state of this item when we're in the main game
        if (!GM.Instance.isTitleMenu)
        {
            isOptions = !isOptions;
            Action_PlayerControls(!isPlayer);
        }
    }

    public void Action_PlayerControls(bool p_PlayerControls)
    {
        isPlayer = p_PlayerControls;
    }

    void Action_Select()
    {
        // Perform the select action
    }

    void Action_Reset()
    {
        // Perform the reset action
    }

    //////////////////
	//	FUNCTIONS	//
	//////////////////

    // Switch between holding and tapping buttons based on options menu (To implement)
    void Check_HoldOrTap(ref bool refBool, int isStarted)
    {
        // Started
        if (isStarted == 0)
        {
            if (!isHoldMode)
            {
                refBool = !refBool;
            }
        }
        // Performed
        else if (isStarted == 1)
        {
            if (isHoldMode)
            {
                refBool = true;
            }
        }
        // Is cancelled
        else if (isStarted == 2)
        {
            if (isHoldMode)
            {
                refBool = false;
            }
        }
    }

	// This 'jeneric' script allows the user to hold down a button, which repeats an action after a fixed period
	// Or allows the user to tap the button quickly
	void Jen_TimedStateFunction(int p_ButtonIndex)
	{
        // The button starts in the false state
        jen_FinalBool[p_ButtonIndex] = false;
        
		if (jen_ActiveBool[p_ButtonIndex])
		{
			// The button is false, unless we've just pressed it, or a specific amount of time has passed
			jen_FinalBool[p_ButtonIndex] = false;

			if (jen_Timer[p_ButtonIndex] == 0.0f)
			{
				jen_FinalBool[p_ButtonIndex] = true;
			}
			else if (jen_Timer[p_ButtonIndex] >= RL_V.timer_ButtonRepeat)
			{
				// Reset the timer and button
				jen_Timer[p_ButtonIndex] = 0.0f;
				jen_FinalBool[p_ButtonIndex] = true;
			}
			
			jen_Timer[p_ButtonIndex] += Time.deltaTime;

		}
		else
		{
			// Reset the timer and the button
			jen_Timer[p_ButtonIndex] = 0.0f;
			jen_FinalBool[p_ButtonIndex] = false;
		}
	}
}
