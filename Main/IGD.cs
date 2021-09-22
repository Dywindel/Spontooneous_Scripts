using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Store the GameData as an In-GameData element
// This way all the elements are accessible through scripting

public class IGD : MonoBehaviour
{
    # region Singleton

    public static IGD Instance {get; private set; }

    private void Awake()
    {
        Instance = this;
    }
        
    # endregion

    // This is the master gameData variable that stores the state of everything
    // That's happening in the game right now
    [HideInInspector]
    public GameData inGameData;
    public GameData_Prefs inGameDataPrefs;

    public void Update_PlayerPos(Vector3 worldPos)
    {
        inGameData.Update_PlayerPos(worldPos);
    }

    public void Update_PlayerRelPos(Vector3 worldPos, Vector3 relPos, Transform relTrans)
    {
        inGameData.Update_PlayerRelPos(worldPos, relPos, relTrans);
    }

    public void Update_PuzzleState(RiftObj riftObj)
    {
        inGameData.Update_PuzzleState(riftObj);
    }

    public void Update_ArtifactState(Sc_Artifact sc_Artifact)
    {
        inGameData.Update_ArtifactState(sc_Artifact);
    }
}
