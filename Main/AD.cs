using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Artifact Database
// Used for storing and sorting Artifact components

public class AD : MonoBehaviour
{
    #region Singleton

    public static AD Instance {get; private set; }

    private void Awake()
    {
        Instance = this;

        InitialLists();
    }
        
    #endregion
    
    // World References
    Sc_SortInput sI;

    public int totalArtifacts = 10;

    // Self references
    private Coroutine cr;

    [HideInInspector] public Sc_Artifact[] arrayOf_Artifacts;
    [HideInInspector] public Sc_Artifact activeArtifact;
    public GameObject[] arrayOf_RTF_Additional;

    // Particular items
    int clapTotal = 0;

    void Start()
    {
        sI = GM.Instance.GetComponent<Sc_SortInput>();
    }

    void InitialLists()
    {
        arrayOf_Artifacts = new Sc_Artifact[totalArtifacts];
    }

    // Update is called once per frame
    void Update()
    {
        if (activeArtifact != null)
        {
            if (activeArtifact.action_South != null)
            {
                if (sI.select)
                {
                    activeArtifact.action_South.Invoke();
                }
            }
        }
    }

    /////////////////////
    // List of Actions //
    /////////////////////

    public void RTF_Action_Clap()
    {
        // Causes the artifact to animate once
        if (activeArtifact.artifactAnim != null)
        {
            activeArtifact.artifactAnim.SetTrigger("isAnimate");
        }

        // Plays a clapping noise
        AC.Instance.SFX_RTF_Clap();

        // If a clap is performed in quick succession, then activate the laser artifact
        // If the coroutine is null, start a quick countdown
        if (cr == null)
        {
            clapTotal += 1;
            cr = StartCoroutine(Clapper_Countdown());
        }
        else
        {
            clapTotal += 1;

            if (clapTotal == 4)
            {
                // We successfully clapped at the right interval
                RTF_Action_Laser_Switch();
            }
        }
    }

    // Update the angle of the bridge item
    public void RTF_Action_UpdateBridgeRotation(float rot)
    {
        // Play SFX
        AC.Instance.SFX_RTF_Bridge_Turning();

        // Set the bridge rotation value to be equal to the send value of rotation (From the painting)
        arrayOf_RTF_Additional[0].transform.rotation = Quaternion.Euler(0f, -rot, 0f);
    }

    public IEnumerator Clapper_Countdown()
    {
        yield return new WaitForSeconds(0.6f);

        // Reset the coroutine
        cr = null;
        // Reset the clap total
        clapTotal = 0;

        yield return null;
    }

    public void RTF_Action_Laser_Switch()
    {
        // Switch the state of the laser
        arrayOf_Artifacts[1].artifactObj.GetComponent<Sc_Artifact_Attachments>().arrayOf_GameObjects[0].SetActive(!arrayOf_Artifacts[1].artifactObj.GetComponent<Sc_Artifact_Attachments>().arrayOf_GameObjects[0].activeSelf);
    }
}
