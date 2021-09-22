using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

// This artifact script is used to load the right text in the right
// Place when a player interacts with the artifact trigger or triggers

public class Sc_Artifact : MonoBehaviour
{
    // World references
    AD aD;
    Sc_Camera_Brain camera_Brain;
    MenuController menuController;
    Sc_SortInput sI;

    public CameraAngle_Distance cameraAngle_Distance;
    public CameraAngle_Side cameraAngle_Side;
    public CameraAngle_Pitch cameraAngle_Pitch;
    public Vector3 cameraOffset;

    // The user selects the state of this artifact
    public ArtifactState artifactState;
    public ArtifactSO artifactSO;
    public GameObject artifactObj; // Reference to instantiated artifact
    public Animator artifactAnim; // If the artifact has an animator component
    public Sc_Cont_Anim animCont;

    public Vector3 artifact_StartPos;
    public bool isMuseumActive = false;

    // For certain items that are interactive
    public UnityEvent action_South;
    public UnityEvent action_OnDeactivate;
    public bool isActive = false;

    void Start()
    {
        // World References
        aD = AD.Instance;
        camera_Brain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Sc_Camera_Brain>();
        menuController = MenuController.Instance;
        sI = GM.Instance.GetComponent<Sc_SortInput>();

        // Give self to the AD
        aD.arrayOf_Artifacts[artifactSO.id] = this;

        animCont = this.GetComponent<Sc_Cont_Anim>();
        artifact_StartPos = artifactObj.transform.position;
        if (artifactObj.GetComponent<Animator>() != null)
        {
            artifactAnim = artifactObj.GetComponent<Animator>();
        }

        // Disable the game object until it's been found
        if (artifactState == ArtifactState.World)
        {
            artifactObj.SetActive(false);
        }
    }

    // What happens when the player enters the trigger area
    public void OnActivate()
    {
        // First, we check the states match, otherwise this means there's nothing here
        if (artifactSO.artifactState == artifactState)
        {
            // If in the museum
            if (artifactState == ArtifactState.Museum)
            {
                // Tell the artifact controller we're the active artifact
                aD.activeArtifact = this;

                // Let the object animate
                animCont.Switch(true);

                // Update museum canvas
                menuController.Active_Museum_Sign(artifactSO);

                // Update camera focus point
                camera_Brain.Set_Camera_Artifact(artifactObj.transform.position, cameraAngle_Distance, cameraAngle_Side, cameraAngle_Pitch, cameraOffset);
            }
        }
    }

    // When the player leaves the trigger area
    public void OnDeactivate()
    {
        // Some artifacts may update other items in the world
        if (action_OnDeactivate != null)
            action_OnDeactivate.Invoke();

        // Tell the Artifact controller it's no longer active
        aD.activeArtifact = null;

        // Disable animation
        animCont.Switch(false);

        // Disable canvas text
        menuController.Deactivate_Museum_Sign();
        
        // Return to regular camera
        camera_Brain.Set_Camera_Player();
    }

    // Editor functionality

    # if UNITY_EDITOR

    // Load the artifact into the scene
    // Artifacts will always appear in the editor, but become disabled if their states don't match
    public void Setup_Artifact()
    {
        Clear_Artifact();

        // Instantiate the gameobj from the SO
        artifactObj = Instantiate(artifactSO.obj, transform);
        Undo.RecordObject(transform.gameObject, "Record new gameobj of artifact.");
        // Change this name to relate to artifacts name
        transform.gameObject.name = "ArtifactObj - " + artifactSO.name;
    }

    public void Clear_Artifact()
    {
        // Clear away anything that already existed
        RL.SafeDestroy<GameObject>(artifactObj);
        // Remove the reference
        artifactObj = new GameObject();

        // Reset name
        transform.gameObject.name = "ArtifactObj";
    }

    # endif

    // List of functions (Because I can't pull events from out of scene?)
    public void RTF_Action_Clap()
    {
        aD.RTF_Action_Clap();
    }

    public void RTF_Action_RotateBridge()
    {
        aD.RTF_Action_UpdateBridgeRotation(animCont.animPlat_Target.transform.rotation.eulerAngles.z);
    }
}
