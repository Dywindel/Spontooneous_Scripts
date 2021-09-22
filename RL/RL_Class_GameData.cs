using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// All the game progress and data stored in one place
// Any info that is needed of the player will be stored in this data class

[System.Serializable]
public class GameData_RiftObj
{
    public int[] l_Pzz;

    public GameData_RiftObj(int[] gridSize)
    {
        l_Pzz = new int[gridSize[0] * gridSize[1] * gridSize[2]];
        for (int i = 0; i < l_Pzz.Length; i++)
        {
            l_Pzz[i] = 0;
        }
    }
}

[System.Serializable]
public class GameData
{
    // Cap on array values
    int cap = 100;
    int maxPzz = 200;

    // Meta states
    public int saveID;

    // Player states
    public int[] playerPos;
    public int[] playerRelPos;
    // Sometimes the player may have been sitting on a relative transform, we'll need to note that
    // And store the player's relative position
    public Transform relTrans = null;

    // Puzzle states
    // If the puzzle has been solved Once already
    public bool[] b_Pzz;
    // array of where bridges have been placed
    // And whether they're bridges or stairs (Use int)
    public GameData_RiftObj[] l_Pzz;

    // Artifact states
    public int[] s_RTF;

    // To throw people off
    // This number randomly gets larger as you play the game
    // It literally means nothing and is an anagram of Red Herring
    int herdingErr;

    ///////////////////
    // FUNCTIONALITY //
    ///////////////////

    // When creating a new GameData object, zero out all elements
    public GameData(int newSaveID)
    {
        // Meta States
        // Set this to -1, so we don't accidentally overwrite an existing save
        saveID = newSaveID;

        // Player states
        // Grab the player's start location
        playerPos = RL_F.V3_IA(Sc_Player.Instance.player_Collider.transform.position);
        playerRelPos = RL_F.V3_IA(Vector3.zero);
        relTrans = null;

        // Puzzles states
        b_Pzz = new bool[maxPzz];
        l_Pzz = new GameData_RiftObj[maxPzz];

        for (int i = 0; i < maxPzz; i++)
        {
            b_Pzz[i] = false;
            l_Pzz[i] = new GameData_RiftObj(new int[3] {0, 0, 0});
        }

        // Artifacts
        s_RTF = new int[cap];
        for (int i = 0; i < cap; i++)
        {
            s_RTF[i] = 0;
        }
    }

    // When passed another GameData object, duplicate it's values
    public GameData(GameData pGameData)
    {
        // Meta Data
        saveID = pGameData.saveID;

        // For Player
        playerPos = pGameData.playerPos;
        playerRelPos = pGameData.playerRelPos;
        relTrans = pGameData.relTrans;

        // For puzzles
        b_Pzz = new bool[maxPzz];
        l_Pzz = new GameData_RiftObj[maxPzz];
        for (int i = 0; i < maxPzz; i++)
        {
            // Puzzle states
            b_Pzz[i] = pGameData.b_Pzz[i];

            // Puzzle bridge setup
            l_Pzz[i] = pGameData.l_Pzz[i];
        }

        // Artifacts
        s_RTF = new int[cap];
        for (int i = 0; i < cap; i++)
        {
            // Artifacts
            s_RTF[i] = pGameData.s_RTF[i];
        }
    }

    // Store the player position in world space
    public void Update_PlayerPos(Vector3 worldPos)
    {
        playerPos = RL_F.V3_IA(worldPos);
        relTrans = null;
    }
    // Store if the player was riding a relative transform
    // We'll need to store local position in this case
    public void Update_PlayerRelPos(Vector3 worldPos, Vector3 relPos, Transform pRelTrans)
    {
        playerPos = RL_F.V3_IA(worldPos);
        playerRelPos = RL_F.V3_IA(relPos);
        relTrans = pRelTrans;
    }

    // Update puzzle state
    public void Update_PuzzleState(RiftObj riftObj)
    {
        // Set if the bridge has been solved once before
        b_Pzz[riftObj.uniqueID] = riftObj.isSolvedOnce;
        // Sort the bridge array into storeable data
        l_Pzz[riftObj.uniqueID] = new GameData_RiftObj(new int[3] {riftObj.gsly, riftObj.gsx, riftObj.gsy});
        for (int ly = 0; ly < riftObj.gsly; ly++)
        {
            for (int i = 0; i < riftObj.gsx; i++)
            {
                for (int j = 0; j < riftObj.gsy; j++)
                {
                    l_Pzz[riftObj.uniqueID].l_Pzz[ly + (i * riftObj.gsly) + (j * riftObj.gsx * riftObj.gsly)] = (int)riftObj.arrayOf_BridgeData[ly, i, j].bridgeType;
                }
            }
        }
    }

    // Update artifact state
    public void Update_ArtifactState(Sc_Artifact sc_Artifact)
    {
        s_RTF[sc_Artifact.artifactSO.id] = (int)sc_Artifact.artifactState;
    }
}

// This is a slightly smaller class that stores information for saved games
// It its one class object that stores info for all 4 saveslots
[System.Serializable]
public class GameData_Prefs
{
    public bool[] newSav_Val;   // Marks if this save file has been used before
    public float[] gamCom_Per;
    public float[] artCol_Per;
    public float[] somEls_Per;

    // Options Settings
    public int[] options;

    // Create a blank GameData_Prefs
    public GameData_Prefs()
    {
        newSav_Val = new bool[4];
        gamCom_Per = new float[4];
        artCol_Per = new float[4];
        somEls_Per = new float[4];

        for (int i = 0; i < 4; i++)
        {
            newSav_Val[i] = false;
            gamCom_Per[i] = 0;
            artCol_Per[i] = 0;
            somEls_Per[i] = 0;
        }

        // Options
        options = new int[5];

        // Initial values
        options[0] = 1;
        options[1] = -1;    // Let's the system know this is the first time we're using resolution, then pick the highest value
        options[2] = 10;
        options[3] = 10;
        options[4] = 10;
    }

    public void Update_Save(int saveID, float pGamCom_Per, float pArtCol_Per, float pSomEls_Per)
    {
        newSav_Val[saveID] = true;
        gamCom_Per[saveID] = pGamCom_Per;
        artCol_Per[saveID] = pArtCol_Per;
        somEls_Per[saveID] = pSomEls_Per;
    }

    public void Update_Options(int pFullscreen, int pResolution, int pMUS, int pSFX, int pAMB)
    {
        options[0] = pFullscreen;
        options[1] = pResolution;
        options[2] = pMUS;
        options[3] = pSFX;
        options[4] = pAMB;
    }
}
