using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using TMPro;

public class GM : MonoBehaviour
{
    # region Singleton

    public static GM Instance {get; private set; }

    [HideInInspector] public Sprite[] arrayOf_Sprites;

    private void Awake()
    {
        Instance = this;
        
        // Make sure the RL dictionaries are loaded
        RL.DeclareFunction();

        WorldReferences();

        // Load the sprites from the resource folder
        object[] arrayOf_Objects = Resources.LoadAll("FakeRunes", typeof(Sprite));
        arrayOf_Sprites = new Sprite[arrayOf_Objects.Length];
        for (int i = 0; i < arrayOf_Sprites.Length; i++)
        {
            arrayOf_Sprites[i] = (Sprite)arrayOf_Objects[i];
        }
    }
        
    # endregion

    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    [HideInInspector] public Sc_Camera_Brain cameraBrain;
    [HideInInspector] public Sc_SortInput sI;
    bool isPaused = false;
    [HideInInspector] public MenuController activeMenu;
    public TextMeshProUGUI[] saveData_TMPro;

    void WorldReferences()
    {
        cameraBrain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Sc_Camera_Brain>();

        sI = this.GetComponent<Sc_SortInput>();
    }

    public UnityEngine.Video.VideoPlayer videoPlayer;

    //////////////////////
    // GAMEPLAY OPTIONS //
    //////////////////////

    // The player can leave a puzzle whenever they want
    [Tooltip("Specifies if the player can leave a puzzle in any direction")]
    public bool noExitRestrictions = true;
    // Allows loading of all zones at once, for easier gameplay testing
    [Tooltip("Will load all zones at once")]
    public bool loadAllZones = false;
    [Tooltip("NoClip the player")]
    public bool isNoClip = false;

    //////////
    // SELF //
    //////////

    public OptionGroup startUp_OptionsGroup;
    public OptionGroup inGame_OptionsGroup;

    private bool isSaving = false; // This is here so we don't accidentally try to save the game twice
    public bool isTitleCards = true; // Toggles title cards. This will be true in release
    public bool isTitleMenu = false; // This slightly alters how the start menu works

    ///////////
    // START //
    ///////////

    void Start()
    {
        if (isTitleCards)
        {
            StartCoroutine(TitleCards());
        }
        else
        {
            if (isTitleMenu)
            {
                TitleMenuStart();

            }
            else
            {
                GameSceneStart();
            }
        }
        
        // Some functions need a frame to get started
        StartCoroutine(DelayedStart());
    }

    // Used for opening title videos
    IEnumerator TitleCards()
    {
        videoPlayer.Play();

        yield return null;
    }

    // Here we will decide what happens when the game is first loaded
    // Load the main menu, with a blank screen behind it (That will be our main menu for now)
    void TitleMenuStart()
    {
        // We start in 'Paused' mode
        // We can't quit pause mode until we start the game
        sI.isOptions = true;
        sI.isPlayer = false;
        isPaused = true;

        // First, let's set the MainMenu as the active menu component and load that in first
        MenuController.Instance.active_OptionGroup = startUp_OptionsGroup;
        MenuController.Instance.Activate_Menu(true);
        MenuController.Instance.active_OptionGroup.Update_ItemSelect(0);    // Make sure the first item is highlighted
        // We can then load in the active menu

        // Set the camera to be focused on the titlemenu target
        cameraBrain.Set_Camera_TitleMenu();

        // Update the GameData and Prefs during startup
        Update_GameData();
    }

    // This startup happens in debug. It doesn't require going through a start menu
    void GameSceneStart()
    {
        // Enable player controls
        sI.isPlayer = true;

        cameraBrain.Set_Camera_Player();

        Create_GameData();
    }

    // Delayed start
    IEnumerator DelayedStart()
    {
        yield return null;

        if (isTitleMenu)
        {
            AC.Instance.AMB_Computer_Hum(true);
        }
        else
        {

        }

        yield return null;
    }

    ///////////////
    // GAME DATA //
    ///////////////

    // For the purposes of debug, just create a blank GameData object
    public void Create_GameData()
    {
        // For debug, we won't be able to load or save this correctly, but it will work for a blank game
        GameData temp_GameData = new GameData(-1);

        IGD.Instance.inGameData = temp_GameData;
    }

    /////////////
    // LOADING //
    /////////////

    // Update the GameData and Prefs during startup
    // Loading the prefs for the savegame menus
    // We do this with a thread
    public void Update_GameData()
    {
        // Grab the persistant datapath
        Persistance.Get_PersistanceDataPath();

        // Grab the GameData_Prefs file
        GameData_Prefs gameData_Prefs = Persistance.Load_GameData_Prefs();

        // If it doesn't exist, create a new one and grab that
        if (gameData_Prefs == null)
        {
            gameData_Prefs = new GameData_Prefs();
            Persistance.Save_GameData_Prefs(gameData_Prefs);
            gameData_Prefs = Persistance.Load_GameData_Prefs();
        }

        // Store a reference to the game data prefs in the IGD
        IGD.Instance.inGameDataPrefs = gameData_Prefs;

        // Go through each item and update the savegame text
        for (int i = 0; i < saveData_TMPro.Length; i++)
        {
            if (!gameData_Prefs.newSav_Val[i])
            {
                string saveGame_Text = "  New Save";
                saveData_TMPro[i].text = saveGame_Text;
            }
            else
            {
                string saveGame_Text = "  Load Save: " + gameData_Prefs.gamCom_Per[i] + " | " + gameData_Prefs.artCol_Per[i] + " | " + gameData_Prefs.somEls_Per[i];
                saveData_TMPro[i].text = saveGame_Text;
            }
        }
    }

    // Perform all the necessary actions to load the game and transition into gameplay mode
    public void Action_SetupLoad(int saveID)
    {
        
        // Play SFX

        // Perform load (Might take a second)
        // Here we load the game and sort through the save files.
        // The bulk of this function is perform in the GM
        GM.Instance.Action_LoadSave(saveID);

        // Show loading screen

        // Layout the game
        Action_LayoutScene();
        
        // Start scene
        Action_StartScene();
    }

    // Load the game, either a new game or a current save
    public void Action_LoadSave(int saveID)
    {
        // Parts of this function will eventually be threaded. However, until then, I will just run everything together

        // First, we load the GameData for this saveID, checking if it exists
        GameData load_GameData = Persistance.Load_GameData(saveID);

        // If the GameData doesn't exist, we create a new GameData and also a new GameData_Prefs for that ID
        if (load_GameData == null)
        {
            load_GameData = new GameData(saveID);
            
            // Can't save and load game data in the same frame, this won't work

            // Update the gameData_Prefs
            // Grab the GameData_Prefs file
            GameData_Prefs gameData_Prefs = Persistance.Load_GameData_Prefs();

            // If it doesn't exist, create a new one and grab that
            if (gameData_Prefs == null)
            {
                gameData_Prefs = new GameData_Prefs();
                
                // Can't save and load data in the same frame
            }

            // Then update the gameData_Prefs value for this saveID
            gameData_Prefs.newSav_Val[saveID] = true;
            // And save it
            Persistance.Save_GameData_Prefs(gameData_Prefs);
        }

        // Set this as the IGD
        IGD.Instance.inGameData = load_GameData;
    }

    public void Action_LayoutScene()
    {
        // Next we place the player object and let the scenes load in (I don't know how long this will take)
        // We'll need to keep the screen obfuscated while those scenes are loading in

        // Place player
        if (IGD.Instance.inGameData.relTrans == null)
        {
            Sc_Player.Instance.Update_PlayerPos_Instant(RL_F.IA_V3(IGD.Instance.inGameData.playerPos), DirType.N);
        }
        else
        {
            // Update player position
            Sc_Player.Instance.Update_PlayerPos_Instant(RL_F.IA_V3(IGD.Instance.inGameData.playerPos), DirType.N);
            // Update player relative position
            Sc_Player.Instance.player_Relative.parent = IGD.Instance.inGameData.relTrans;
        }

        // Go through each rift and drop a bridge in places where there are bridges to be placed
        // Additionally, check if the rift is solved after placing the bridges
        // Would be useful to have a function like this that doesn't create sound upon solving
        PD.Instance.Layout_Rifts();
    }

    public void Action_StartScene()
    {
        // Disable the main menu
        isTitleMenu = false;
        sI.isOptions = false;

        // Set the camera to the player's position
        cameraBrain.Set_Camera_Player();

        // Allow the player to move
        sI.isPlayer = true;
    }

    // This function loads an image of the game onto the computer screen, then switches to that scene instantaneously
    public IEnumerator LoadInto_Scene()
    {
        yield return null;
    }

    // When we quit back to the main menu, remember to save the game

    ////////////
    // UPDATE //
    ////////////

    void Update()
    {
        // We behave slightly different when we're on the title screen
        if (isTitleMenu)
        {
            if (activeMenu != null)
            {
                activeMenu.Activate_Menu(true);
            }
        }
        else
        {
            if (sI.isOptions != isPaused)
            {
                isPaused = sI.isOptions;

                // Perform menu popup animation and make the menu active
                if (isPaused)
                {
                    if (activeMenu != null)
                    {
                        activeMenu.Activate_Menu(true);
                    }
                }
                else
                {
                    // Do the reverse
                    if (activeMenu != null)
                    {
                        activeMenu.Activate_Menu(false);
                    }
                }
            }
        }
    }

    ////////////////////
    // SAVE LOAD GAME //
    ////////////////////

    // Saves the active GameData in the corresponding game file
    // This will be run in a separate thread
    public bool Thread_SaveGame()
    {
        // Store the player's current position
        if (C_MT.relativeTransform_Entering != null)
        {
            IGD.Instance.Update_PlayerRelPos(Sc_Player.Instance.player_Collider.transform.position, Sc_Player.Instance.player_Relative.localPosition, Sc_Player.Instance.transform);
        }
        else
        {
            IGD.Instance.Update_PlayerPos(Sc_Player.Instance.player_Collider.transform.position);
        }
        

        // Don't try and save the game if we're already saving
        if (isSaving)
        {
            return false;
        }

        isSaving = true;

        // Start the saving process
        Persistance.Save_GameData(IGD.Instance.inGameData.saveID, IGD.Instance.inGameData);
        
        isSaving = false;

        return true;
    }

    // Loads a GameData file
    public bool Thread_LoadGame()
    {
        return true;
    }
}