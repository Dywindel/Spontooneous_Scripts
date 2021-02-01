using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Sort and organise player input from controller or keyboard

public class Sc_SortInput : MonoBehaviour
{
    PlayerControls controls;

    //////////////
    // MOVEMENT //
    //////////////

    [System.NonSerialized]
    public Vector2 move;
    [System.NonSerialized]
    public MoveCard moveCard;
    [System.NonSerialized]
    public MoveCard storeMoveCard = MoveCard.None;
    [System.NonSerialized]
    public bool isMove = false;

    [System.NonSerialized]
    public bool pause = false;
    [System.NonSerialized]
    public bool select = false;
    [System.NonSerialized]
    public bool start = false;
    [System.NonSerialized]
    public bool undo = false;
    [System.NonSerialized]
    public bool reset = false;

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
    private int jen_Total = 2;      // Number of 'jen' buttons being used. 0 - Undo, 1 - isMove

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

        controls.Player.Pause.started += ctx => pause = true;
        controls.Player.Pause.canceled += ctx => pause = false;
        controls.Player.Select.performed += ctx => select = true;
        controls.Player.Select.canceled += ctx => select = false;

        controls.Player.Undo.performed += ctx => jen_ActiveBool[0] = true;
        controls.Player.Undo.canceled += ctx => jen_ActiveBool[0] = false;

        controls.Player.Reset.started += ctx => reset = true;
        controls.Player.Reset.performed += ctx => reset = false;

        controls.Player.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
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
                    moveCard = MoveCard.E;
                }
                else
                {
                    moveCard = MoveCard.W;
                }
            }
            else
            {
                if (move.y > 0f)
                {
                    moveCard = MoveCard.N;
                }
                else
                {
                    moveCard = MoveCard.S;
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
            moveCard = MoveCard.None;
            // Store the active jen bool
            jen_ActiveBool[1] = false;
        }
        // Store the current movement cardinal
        storeMoveCard = moveCard;

        // Update all jen_Bools
		for (int i = 0; i < jen_Total; i++)
		{
			Jen_TimedStateFunction(i);
		}

        // Pass the final bools to the named bools
        undo = jen_FinalBool[0];
        isMove = jen_FinalBool[1];
    }

    void Action_Pause()
    {
        // Perform the Pause action
    }

    void Action_Select()
    {
        // Perform the select action
    }

    void Action_Reset()
    {
        // Perform the reset action
    }

    //##############//
	//	FUNCTIONS	//
	//##############//

	// This 'jeneric' script allows the user to hold down a button, which repeat an action after a fixed period
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
			//Reset the timer and the button
			jen_Timer[p_ButtonIndex] = 0.0f;
			jen_FinalBool[p_ButtonIndex] = false;
		}
	}
}
