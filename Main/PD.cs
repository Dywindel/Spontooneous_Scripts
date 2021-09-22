using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is the Puzzle Database, it is used to access and
// Reference all the puzzles in the game

public class PD : MonoBehaviour
{
    #region Singleton

    public static PD Instance {get; private set; }

    private void Awake()
    {
        Instance = this;
    }
        
    #endregion

    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    GM gM;
    Sc_Camera_Brain cameraBrain;

    public List<RiftObj> listOf_RiftObjs;   // This stores all the rifts in the game
    public ZM[] arrayOf_ZM;   // This stores all the ZM in the game, they should be numbered in an ordered the same as the Biome enum
    public RiftObj activeRift;    // Currently active rift
    public bool isRiftActive = false;   // Checks if the player is currently inside a puzzle

     List<RuneData> listOf_FaultyRunes;     // For communicating to the player which items failed

    void Start()
    {
        listOf_RiftObjs = new List<RiftObj>();

        WorldReferences();
    }

    void WorldReferences()
    {
        gM = GM.Instance;
        cameraBrain = gM.cameraBrain;
    }

    ////////////////
    // LOAD SCENE //
    ////////////////

    public void Layout_Rifts()
    {
        GameData gameData = IGD.Instance.inGameData;

        // Go through every rift
        foreach (RiftObj riftObj in listOf_RiftObjs)
        {
            // Has this list been accurately constructed?
            if (gameData.l_Pzz[riftObj.uniqueID].l_Pzz.Length != 0)
            {
                for (int ly = 0; ly < riftObj.gsly; ly++)
                {
                    for (int i = 0; i < riftObj.gsx; i++)
                    {
                        for (int j = 0; j < riftObj.gsy; j++)
                        {
                            // Is there a bridge piece here?
                            if (gameData.l_Pzz[riftObj.uniqueID].l_Pzz[ly + (i * riftObj.gsly) + (j * riftObj.gsx * riftObj.gsly)] != (int)BridgeType.None)
                            {
                                riftObj.Place_GenObj(new int[3] {ly, i, j}, (BridgeType)gameData.l_Pzz[riftObj.uniqueID].l_Pzz[ly + (i * riftObj.gsly) + (j * riftObj.gsx * riftObj.gsly)], PlankDir.None);
                            }
                        }
                    }
                }

                // Recalculate trellis
                riftObj.Update_TrellisSpace();
                
                // Redo bridge visuals
                for (int ly = 0; ly < riftObj.gsly; ly++)
                {
                    for (int i = 0; i < riftObj.gsx; i++)
                    {
                        for (int j = 0; j < riftObj.gsy; j++)
                        {
                            if (riftObj.arrayOf_BridgeData[ly, i, j].bridgeType != BridgeType.None)
                            {
                                riftObj.arrayOf_BridgeData[ly, i, j].bridgeObj.GetComponent<BridgeObj_Mesh>().UpdateMesh(0);
                            }
                        }
                    }
                }

                // Update the rift's visuals
                riftObj.Update_Visuals_Bridge();

                // Check if the rift is solved
                Check_IfRift_Solved(riftObj);
            }
        }
    }

    //////////////////
    // RIFT PUZZLES //
    //////////////////

    // When changing the active rift, please use this function
    // Don't just set it
    public void Toggle_Active_Rift(RiftObj riftObj)
    {
        // We update active rift inside the PD
        activeRift = riftObj;
        if (activeRift != null)
        {
            // Store whether a rift is active or not
            isRiftActive = true;

            // Update the camera position
            cameraBrain.Set_Camera_Rift(activeRift.cameraAngle_Pitch);
        }
        else
        {
            isRiftActive = false;

            // Update camera
            cameraBrain.Set_Camera_Player();
        }
    }

    // This function sets up for checking if the puzzle is solved
    public void Check_IfRift_Solved(RiftObj pActiveRift)
    {
        // We only perform a check if there is an active rift
        if (pActiveRift != null)
        {
            // TESTING
            if (true)
            {
                // Update the state of each rune inside the riftObj
                PD_Check.PD_C(pActiveRift);

                // Update the icons, changing their colours and creating sounds if necessary
                // We also keep a tally of the unsolved icons
                int unsolvedIcons = 0;

                // Let's check to see if any runes failed
                foreach (RuneData runeData in pActiveRift.listOf_RuneData)
                {
                    if (runeData.runeState == RuneState.Fail)
                    {
                        unsolvedIcons += 1;

                        if (runeData.runeType == RuneType.Boss)
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_Off_Boss;
                        }
                        else if (runeData.runeType == RuneType.Museum)
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_Off_Museum;
                        }
                        else
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_Off;
                        }
                    }
                    else if (runeData.runeState == RuneState.BecomeFail)
                    {
                        // We update the number of unsolved icons
                        unsolvedIcons += 1;

                        if (runeData.runeType == RuneType.Boss)
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_Off_Boss;
                        }
                        else if (runeData.runeType == RuneType.Museum)
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_Off_Museum;
                        }
                        else
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_Off;
                        }
                        AC.Instance.SFX_Icon_Switch(false);
                    }
                    else if (runeData.runeState == RuneState.Succeed)
                    {
                        // Colour is based on type
                        if (runeData.runeType == RuneType.Boss)
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_On_Boss;
                        }
                        else if (runeData.runeType == RuneType.Museum)
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_On_Museum;
                        }
                        else
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_On;
                        }
                        
                    }
                    else if (runeData.runeState == RuneState.BecomeSucceed)
                    {
                        // Colour is based on type
                        if (runeData.runeType == RuneType.Boss)
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_On_Boss;
                        }
                        else if (runeData.runeType == RuneType.Museum)
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_On_Museum;
                        }
                        else
                        {
                            runeData.runeObj.GetComponentInChildren<SpriteRenderer>().color = RL_V.iconColor_On;
                        }
                        AC.Instance.SFX_Icon_Switch(true);
                    }
                }

                // Now we can check if there are any unsolved icons
                if (unsolvedIcons == 0)
                {
                    // Set the solvedNow state of the puzzle
                    pActiveRift.isSolvedNow = true;

                    // Also make sure we note that it has been solved once
                    pActiveRift.isSolvedOnce = true;
                }
                else
                {
                    // Set the solvedNow state of the puzzle
                    pActiveRift.isSolvedNow = false;
                }
                
                // The rift state has changed, alter necessary objects
                pActiveRift.Action_Upon_UpdateState();
            }
            else
            {
                // Set the solvedNow state of the puzzle
                pActiveRift.isSolvedNow = false;
            }
        }

        // Prepare animations

    }

    /////////////////////
    // RESET VARIABLES //
    /////////////////////

    public void Reset_Variables()
    {
        // If there is an active rift, reset it's variables
        if (activeRift != null)
        {
            activeRift.Reset_Variables();
        }
    }

}
