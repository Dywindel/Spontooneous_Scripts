using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A list of actions that menu items can take
public class ActionList : MonoBehaviour
{
    // World references
    public MenuController menuController;

    // Starts the game
    public void Action_Start()
    {

    }

    // Change the current active option
    public void Side_OptionsSubgroup(OptionGroup pOptionGroup)
    {
        // SFX
        AC.Instance.OneShot_UI_Enter();

        menuController.Side_OptionsSubgroup(pOptionGroup);
    }

    // Go deeper into the options submenus
    public void Lower_OptionsSubgroup(OptionGroup pOptionGroup)
    {
        // SFX
        AC.Instance.OneShot_UI_Enter();

        menuController.Lower_OptionsSubgroup(pOptionGroup);
    }

    // Back out of the options submenus
    public void Higher_OptionsSubgroup()
    {
        // SFX
        AC.Instance.OneShot_UI_Enter();

        menuController.Higher_OptionsSubgroup();
    }

    // Allows the player to use the back button or resume button
    // When inside the pause menu
    public void BackOut_PauseMenu()
    {
        // SFX
        AC.Instance.OneShot_UI_Enter();
        
        GM.Instance.sI.Action_Pause();
    }

    // Close the game
    public void Action_Quit()
    {
        // Disable player menu interaction
        menuController.isQuit = true;

        // Play SFX
        AC.Instance.AMB_Computer_Hum(false);

        // Wait a second or so before shutting down the program
        StartCoroutine(Action_Quit_Delay());
    }

    IEnumerator Action_Quit_Delay()
    {
        yield return new WaitForSeconds(1.5f);

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif

        yield return null;
    }

    public void Action_LoadSave(int saveID)
    {
        // SFX for loading computer sound
        AC.Instance.AMB_Computer_Hum(false);

        GM.Instance.Action_SetupLoad(saveID);        
    }

    // Saves menu preferences
    public void Action_SaveGame_Prefs()
    {
        Persistance.Save_GameData_Prefs(IGD.Instance.inGameDataPrefs);
    }
}
