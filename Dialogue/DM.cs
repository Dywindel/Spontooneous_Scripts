using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DM : MonoBehaviour
{
    #region Singleton

    // World References
    Sc_SortInput sI;
    Sc_Camera_Brain camera_Brain;
    private DialogueVertexAnimator dialogueVertexAnimator;

    public static DM Instance {get; private set; }

    private void Awake()
    {
        Instance = this;

        dialogueVertexAnimator = new DialogueVertexAnimator(textBox);
    }

    #endregion

    public void Start()
    {
        sI = GM.Instance.GetComponent<Sc_SortInput>();
        camera_Brain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Sc_Camera_Brain>();
    }

    private bool isActive = false;
    private bool isConversation = false;

    public TMP_Text textBox;
    public GameObject dialogueWindow;
    public GameObject dialogue_Select;
    public GameObject dialogue_Speaker;
    public TextMeshProUGUI text_SpeakerName;
    public GameObject panel_SelectSection;
    public GameObject object_SelectIcon;
    private GameObject[] dialogueSelectable_Items;

    public ConversationSO activeConversation;
    public SpeechSO bark;
    public Animator activeAnim;
    public Transform activeFocus;
    //public int speechIndex = 0;     // Which speech we're on in a conversation

    private Coroutine cr_Speech = null;
    private Coroutine cr_Line = null;

    // Allows the conversation to start immediately, without having to press interact
    private bool doOnce = false;

    // Selectable UI
    int row = 0;
    float stepValue = 0f;
    [HideInInspector] public Image[] arrayOf_SpeechIcons;
    public RectTransform panel_IconSelect;
    int activeIconSelection = 0;     // The active selection we're currently at
    Vector2 activeIconPosition = new Vector2(0, 0);        // The actively selected icon
    public TextMeshProUGUI text_DialogueName;
    public NPCManager active_NPCManager;


    public void OnActivate()
    {
        // Invoke the starting event if this is our first interaction
        // Currently, only ConversationSO NPCs can invoke events.
        if (active_NPCManager.conversationSO != null && active_NPCManager.conversationSO.speechIndex == 0)
        {
            if (active_NPCManager.event_StartConversation != null)
            {
                active_NPCManager.event_StartConversation.Invoke();
            }
        }

        // Set this menu to be active
        isActive = true;
        // Disable player movement
        sI.isPlayer = false;

        // Open the dialogue window
        dialogueWindow.SetActive(true);

        // We may disable or enbale the Speaker and select window depending on certain conditions
        // For NonNPC dialogue
        if (activeConversation != null && activeConversation.isNonNPC)
        {
            dialogue_Speaker.SetActive(false);
            dialogue_Select.SetActive(false);

            // Run the conversation
            Run_Conversation();
        }
        // If this is NPC dialogue, we need to move the camera
        else
        {
            if (activeConversation != null)
            {
                // Set the name
                text_DialogueName.text = activeConversation.characterName.ToString();
                camera_Brain.Set_Camera_NPC(activeFocus.position, activeConversation.cameraAngle_Distance, activeConversation.cameraAngle_Side, activeConversation.cameraAngle_Pitch);
            }
            else
            {
                // Set the name
                text_DialogueName.text = bark.setName;
                // Do not move the camera? Let's see what that looks like
            }
        }
        
        // For selectable conversations
        if (activeConversation != null && activeConversation.isSelectable)
        {
            // Sort dialogue icons
            SetVariables();
            Setup_UI_SelectableGrid();
            // And the reticule
            Update_ItemSelect(Vector2.zero);
        }
        // For none-selectable conversations
        else
        {
            // Disable the select window
            dialogue_Select.SetActive(false);

            // Run the conversation
            Run_Conversation();
        }
    }

    public void OnDeactivate()
    {
        // Deactivate the menu bool
        isActive = false;
        // Enable player movement
        sI.isPlayer = true;
        // Clear the dialogue box
        dialogueVertexAnimator.Clear_DialogueBox();

        // If we're at the end of the dialogue perform the end event
        if (!active_NPCManager.repeat_LastLine && active_NPCManager.hasReachedLastLine)
        {
            active_NPCManager.event_EndConversation.Invoke();
        }

        // Deactivate variables
        active_NPCManager = null;

        // Deactivate the active interaction
        UM_Triggers.Instance.Deactivate_ActiveInteraction();
        // Disable future interactions for a short period
        StartCoroutine(UM_Triggers.Instance.Wait_BeforeReInteraction());

        // Change the camera
        camera_Brain.Set_Camera_Player();

        // Enable the other windows
        dialogue_Speaker.SetActive(true);
        dialogue_Select.SetActive(true);

        // Close the dialogue window
        dialogueWindow.SetActive(false);
    }

    // Here we record player menu selection
    void Update()
    {
        // Only Update if
        // - The dialogueSelector is active
        // - There isn't a conversation running
        // - The player isn't in playmode
        if (isActive && !isConversation && !sI.isPlayer)
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
                // Back out of the menu
                OnDeactivate();
            }
            if (sI.select)
            {
                // Play the active conversation
                Run_Conversation();
            }
        }
    }

    // If the index changes, update the item we're selecting
    public void Update_ItemSelect(Vector2 dir)
    {
        // We check if we can add in that direction (Movement is not cyclic)
        Vector2 moveTo = activeIconPosition + dir;
        // We only play the SFX if the direction has changed
        if (dir != Vector2.zero)
        {
            // Check we're inside the grid
            if (moveTo.x < row && moveTo.x >= 0 && moveTo.y < row && moveTo.y >= 0)
            {
                // Check our int value isn't too high
                int moveToInt = (int)(moveTo.x + row*moveTo.y);
                if (moveToInt < activeConversation.conversation.Length)
                {
                    // SFX
                    AC.Instance.OneShot_UI_Keys();

                    // We can move to the new selection
                    activeIconPosition = moveTo;

                    // We also update the speech index
                    activeConversation.speechIndex = (int)(activeIconPosition.x + row*activeIconPosition.y);
                }
            }
        }

        // Move the reticule
        panel_IconSelect.anchorMin = new Vector2(stepValue * activeIconPosition.x, stepValue * activeIconPosition.y);
        panel_IconSelect.anchorMax = new Vector2(stepValue * (activeIconPosition.x + 1), stepValue * (activeIconPosition.y + 1));
        panel_IconSelect.offsetMax = Vector2.zero;
        panel_IconSelect.offsetMin = Vector2.zero;

        // Update the text name
        text_DialogueName.color = Color.white;
        text_DialogueName.text = activeConversation.conversation[(int)(activeIconPosition.x + (row * activeIconPosition.y))].setName;
    }

    void SetVariables()
    {
        // Calculate row length
        row = (int)Mathf.Ceil(Mathf.Sqrt(activeConversation.conversation.Length));
        arrayOf_SpeechIcons = new Image[row*row];

        // Set the speaker name
        text_SpeakerName.text = Enum.GetName(typeof(CharacterName), (int)activeConversation.characterName);
    }

    // Setup the Selectable grid
    public void Setup_UI_SelectableGrid()
    {
        // Step value
        stepValue = 1f/row;

        // Create a grid of icons
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < row; j++)
            {
                // Create a panel
                arrayOf_SpeechIcons[i + j*row] = Instantiate(object_SelectIcon, panel_SelectSection.transform).GetComponent<Image>();
                // Reposition the icons inside the grid
                arrayOf_SpeechIcons[i + j*row].GetComponent<RectTransform>().anchorMin = new Vector2(stepValue * i, stepValue * j);
                arrayOf_SpeechIcons[i + j*row].GetComponent<RectTransform>().anchorMax = new Vector2(stepValue * (i + 1), stepValue * (j + 1));
                arrayOf_SpeechIcons[i + j*row].GetComponent<RectTransform>().offsetMax = Vector2.zero;
                arrayOf_SpeechIcons[i + j*row].GetComponent<RectTransform>().offsetMin = Vector2.zero;
                // If there isn't a speech for this int value, just don't load an image
                if (i + row*j < activeConversation.conversation.Length)
                {
                    arrayOf_SpeechIcons[i + row*j].color = Color.white;
                    arrayOf_SpeechIcons[i + row*j].sprite = activeConversation.conversation[i + row*j].icon;
                }
                else
                {
                    // Make it invisible
                    arrayOf_SpeechIcons[i + row*j].color = new Color(0f, 0f, 0f, 0f);
                }
            }
        }
    }

    public void GreyOut_Selectables(bool isGreyOut)
    {
        for (int i = 0; i < activeConversation.conversation.Length; i++)
        {
            if (isGreyOut)
            {
                arrayOf_SpeechIcons[i].color = new Color(0.25f, 0.25f, 0.25f, 1f);
            }
            else if (!isGreyOut)
            {
                arrayOf_SpeechIcons[i].color = Color.white;
            }
        }
    }

    public void Run_Conversation()
    {
        string[] lines = new string[2] {"Uh oh, this dialogue is empty!", "Better send a formal complaint"};
        // Split the speech up into lines
        if (activeConversation != null)
        {
            lines = activeConversation.conversation[activeConversation.speechIndex].speech.Split('\n');
        }
        else
        {
            lines = bark.speech.Split('\n');
        }
        // Play the conversation
        doOnce = true;
        cr_Speech = StartCoroutine(PlaySpeech(lines));
    }

    IEnumerator<string> PlaySpeech(string[] lines)
    {
        isConversation = true;
        if (activeConversation != null && activeConversation.isSelectable)
        {
            // Grey out the icons window, so the player knows someone is talking and that they can't select anything right now
            GreyOut_Selectables(true);
        }
        int index = -1;

        while (true)
        {
            if (sI.select || doOnce)
            {
                doOnce = false;

                // If the text typewriter is occuring, jump to the end
                if (dialogueVertexAnimator.textAnimating)
                {
                    dialogueVertexAnimator.SkipToEndOfCurrentMessage();
                }
                else
                {
                    index += 1;

                    if (index < lines.Length)
                    {
                        PlayDialogue(lines[index]);
                    }
                    else
                    {
                        // Leave this while loop
                        break;
                    }
                }
            }
            else if (sI.undo)
            {
                // Leave the conversation early, do not increment the speechIndex
                break;
            }
            
            yield return null;
        }

        // Cancel the animations
        SwitchOff_Animation_State(AnimationMode.None);

        // Conversation has ended
        isConversation = false;
        if (activeConversation != null && activeConversation.isSelectable)
        {
            // Bring back the selectables
            GreyOut_Selectables(false);
        }

        // Should I keep the last line of dialogue up?

        // In selectable mode
        if (activeConversation != null && activeConversation.isSelectable)
        {
            // We just return to selecting the next dialogue segment
            // Do nothing?
        }
        // In non-selectable mode
        else
        {
            // We increment the speech index up to the last line of speech
            if (index == lines.Length)
            {
                if (activeConversation != null && (activeConversation.speechIndex + 1 < activeConversation.conversation.Length))
                {
                    activeConversation.speechIndex += 1;
                }
                else
                {
                    // Record that we've reached the end of the dialogue tree
                    active_NPCManager.hasReachedLastLine = true;
                }
            }

            // End the dialogue
            dialogueVertexAnimator.FinishAnimating(OnDeactivate);
        }
                
    }

    // Change the animation state of the NPC based on what the state is set to in the Speech section
    public void SwitchOff_Animation_State(AnimationMode animationMode)
    {
        if (activeAnim != null)
        {
            // Switch off all animations
            foreach (AnimatorControllerParameter parameter in activeAnim.parameters)
            {
                activeAnim.SetBool(parameter.name, false);
            }
        }
    }
    
    public void PlayDialogue(string message) {
        this.EnsureCoroutineStopped(ref cr_Line);
        dialogueVertexAnimator.textAnimating = false;
        dialogueVertexAnimator.anim = activeAnim;
        List<DialogueCommand> commands = DialogueUtility.ProcessInputString(message, out string totalTextMessage);
        cr_Line = StartCoroutine(dialogueVertexAnimator.AnimateTextIn(commands, totalTextMessage, null));
    }


}
