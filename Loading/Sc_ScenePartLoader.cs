using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// This script is used to trigger scene loading when the player enters an area

public class Sc_ScenePartLoader : MonoBehaviour
{
    private Transform player;

    // Scene State
    private bool isLoaded;
    private bool shouldLoad;

    public bool alwaysLoad;

    void Start()
    {
        // Grab the player transform
        player = Sc_Player.Instance.GetComponent<Sc_Player>().player_Collider.transform;

        // Check if the scene is already open to avoid loading it twice
        if (SceneManager.sceneCount > 0)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == gameObject.name)
                {
                    isLoaded = true;
                }
            }
        }
    }

    void Update()
    {
        TriggerCheck();
    }



    // Add the scene to the game
    void LoadScene()
    {
        if (!isLoaded)
        {
            // Add the scene to our current game
            SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            // Set that this scene is now loaded
            isLoaded = true;
        }
    }

    // Remove the scene from the game
    void UnLoadScene()
    {
        if (isLoaded)
        {
            SceneManager.UnloadSceneAsync(gameObject.name);
            isLoaded = false;
        }
    }
    
    // Check if the player transform is inside the loading zone
    void TriggerCheck()
    {
        if (shouldLoad || GM.Instance.loadAllZones || alwaysLoad)
        {
            LoadScene();
        }
        else
        {
            UnLoadScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shouldLoad = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shouldLoad = false;
        }
    }
}
