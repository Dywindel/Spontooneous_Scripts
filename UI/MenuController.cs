using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

// Menu master controller
// Controls what is selected and what the player is interacting with

public class MenuController : MonoBehaviour
{
    # region Singleton

    public static MenuController Instance {get; private set; }

    private void Awake()
    {
        Instance = this;
    }
        
    # endregion

    [HideInInspector] public bool isQuit = false;

    // World refences
    Sc_SortInput sI;

    [Header("Main Menus")]
    public OptionGroup active_OptionGroup;
    public List<OptionGroup> listOf_OptionGroup;

    [Header("Museum Texts")]
    public GameObject menu_Museum;
    public TextMeshProUGUI text_Museum;
    public DM dM;
    public Animator anim_Museum;

    [Header("Warp")]
    public GameObject menu_Warp;
    public Animator anim_Warp;

    // Save game one-shot function
    public bool doOnce_Save = false;

    void Start()
    {
        // Send self to the GM
        GM gM = GM.Instance;
        gM.activeMenu = this;
        sI = gM.GetComponent<Sc_SortInput>();

        listOf_OptionGroup.Add(active_OptionGroup);
    }

    // Activate the menu system
    public void Activate_Menu(bool activate)
    {
        if (activate)
        {
            // Have the currently active options menu appear
            active_OptionGroup.gameObject.SetActive(true);
        }
        else
        {
            // Clear everything up and close the menus
            while (listOf_OptionGroup.Count > 1)
            {
                Higher_OptionsSubgroup();
            }

            Higher_OptionsSubgroup();
        }
    }

    void Update()
    {
        // We don't interact with the menu when we're outside of it
        // Or when we're quitting the game
        if (sI.isOptions && !isQuit) // Menu bool goes here
        {
            // Save game one-shot function
            if (!doOnce_Save && !GM.Instance.isTitleMenu)
            {
                GM.Instance.Thread_SaveGame();
                doOnce_Save = true;
            }

            // Get a reference to the sort input
            if (sI.moveCard_Menu != DirType.None &&
                active_OptionGroup.arrayOf_OptionItems.Length > 0)
            {
                // Change active option menu index
                if ((active_OptionGroup.isMenuVertical && sI.moveCard_Menu == DirType.S)
                    || (!active_OptionGroup.isMenuVertical && sI.moveCard_Menu == DirType.E))
                {
                    // Increment the option item index
                    active_OptionGroup.Update_ItemSelect(true);
                }
                else if ((active_OptionGroup.isMenuVertical && sI.moveCard_Menu == DirType.N)
                    || (!active_OptionGroup.isMenuVertical && sI.moveCard_Menu == DirType.W))
                {
                    // Decrement the option item index
                    active_OptionGroup.Update_ItemSelect(false);
                }

                // Respond to user directional input on menu item
                // Direction will be opposite of the options direction
                if ((!active_OptionGroup.isMenuVertical && sI.moveCard_Menu == DirType.N)
                    || (active_OptionGroup.isMenuVertical && sI.moveCard_Menu == DirType.E))
                {
                    active_OptionGroup.arrayOf_OptionItems[active_OptionGroup.index].Input_Direction(true);
                }
                else if((!active_OptionGroup.isMenuVertical && sI.moveCard_Menu == DirType.S)
                    || (active_OptionGroup.isMenuVertical && sI.moveCard_Menu == DirType.W))
                {
                    active_OptionGroup.arrayOf_OptionItems[active_OptionGroup.index].Input_Direction(false);
                }
            }

            // If the sort input is a button press
            if (sI.select &&
                active_OptionGroup.arrayOf_OptionItems.Length > 0)
            {
                active_OptionGroup.arrayOf_OptionItems[active_OptionGroup.index].Input_Action();
            }

            // If we're going back
            if (sI.undo)
            {
                active_OptionGroup.Input_Back();
            }
        }
        else
        {
            doOnce_Save = false;
        }
    }

    // Change the current active option
    public void Side_OptionsSubgroup(OptionGroup pOptionGroup)
    {
        // Grab the current index
        int tIndex = active_OptionGroup.index;

        // Disable current options group
        active_OptionGroup.gameObject.SetActive(false);

        // Remove it from the list
        listOf_OptionGroup.Remove(listOf_OptionGroup.Last());

        // Add this options to the group list
        listOf_OptionGroup.Add(pOptionGroup);

        // Set it as the new active group
        active_OptionGroup = listOf_OptionGroup.Last();

        // Enable new group
        active_OptionGroup.gameObject.SetActive(true);

        // Update the index and active selection
        active_OptionGroup.index = tIndex;
        active_OptionGroup.Update_ItemSelect(active_OptionGroup.index);
    }

    // Go deeper into the options menus
    public void Lower_OptionsSubgroup(OptionGroup optionGroup)
    {
        // Disable current options group
        active_OptionGroup.gameObject.SetActive(false);

        // Change the active group
        active_OptionGroup = optionGroup;

        // Add this options to the group list
        listOf_OptionGroup.Add(active_OptionGroup);

        // Enable new group
        active_OptionGroup.gameObject.SetActive(true);

        // Update the active selection (If there are items avialable to update)
        if (active_OptionGroup.arrayOf_OptionItems.Length > 0)
        {
            active_OptionGroup.Update_ItemSelect(active_OptionGroup.index);
        }
    }

    // Move back out of the options menus
    public void Higher_OptionsSubgroup()
    {
        // Disable the current options group
        active_OptionGroup.gameObject.SetActive(false);

        // We don't want to do anything else if we're at the top of the menu group
        if (listOf_OptionGroup.Count > 1)
        {
            // Remove it from the list
            listOf_OptionGroup.Remove(listOf_OptionGroup.Last());

            // Active the last options in the list
            active_OptionGroup = listOf_OptionGroup.Last();

            // ACtivate it
            active_OptionGroup.gameObject.SetActive(true);

            // Update the active selection
            if (active_OptionGroup.arrayOf_OptionItems.Length > 0)
            {
                active_OptionGroup.Update_ItemSelect(active_OptionGroup.index);
            }
        }
    }

    // Museum Texts
    // Active museum sign and update text
    public void Active_Museum_Sign(ArtifactSO artifactSO)
    {
        menu_Museum.SetActive(true);
        //dM.dialogue1 = artifactSO.description;
        text_Museum.text = artifactSO.description;
        //dM.PlayDialogue(artifactSO.description);
        anim_Museum.SetBool("isOpen", true);
    }

    // Deactivate museum sign and text
    public void Deactivate_Museum_Sign()
    {
        //menu_Museum.SetActive(false);
        //text_Museum.text = "";
        anim_Museum.SetBool("isOpen", false);
    }

    // Warp Menu
    // Activate Warp menu
    public void Activate_Warp_Menu()
    {
        // Switch on menu
        //menu_Warp.SetActive(true);
        anim_Warp.SetBool("isOpen", true);
    }

    // Deactivate the warp menu
    public void Deactivate_Warp_Menu()
    {
        //menu_Warp.SetActive(false);
        anim_Warp.SetBool("isOpen", false);
    }
}
