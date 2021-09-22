using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a little animated object that switches on a machine when all the
// Rift icons that are in the area are switched on

public class RiftBox : MonoBehaviour
{
    RL_M rl_m;

    
    float rowLength;
    float scaleFactor;
    float offsetValue;

    public Transform riftBox_Screen;

    // List of Animated Trigger
    public List<Sc_Cont_Anim> listOf_SolvedState_Anim_Trigger;

    // List of RiftObjs
    public RiftObj[] arrayOf_RiftObj;
    // List of runes
    public List<RuneData> listOf_RuneData;
    // List of sprites to display
    private SpriteRenderer[] arrayOf_Sprites;

    bool isSolvedOnce = false;
    bool isSolvedNow = false;

    public int faultyRuneCount;


    // Start is called before the first frame update
    void Start()
    {
        WorldReferences();

        InitialiseLists();

        // Delays this setup until after rifts have been setup
        StartCoroutine(DelayStart());
    }

    void WorldReferences()
    {
        rl_m = GameObject.FindObjectOfType<GM>().GetComponent<RL_M>();
    }

    void InitialiseLists()
    {
        listOf_RuneData = new List<RuneData>();
    }

    void Calculate_Scale()
    {
        rowLength = Mathf.Ceil(Mathf.Sqrt(listOf_RuneData.Count));
        scaleFactor = riftBox_Screen.localScale.x/rowLength;
        offsetValue = 0.5f - (0.5f * 1/rowLength);
    }

    IEnumerator DelayStart()
    {
        yield return null;

        // Get all the riftObjs with the same parent as this
        arrayOf_RiftObj = this.transform.parent.GetComponentsInChildren<RiftObj>();

        // And all the icons
        foreach (RiftObj riftObj in arrayOf_RiftObj)
        {
            // Pass yourself to the riftObj
            riftObj.riftBox = this;

            foreach (RuneData runeData in riftObj.listOf_RuneData)
            {
                print ("Added RuneData");
                listOf_RuneData.Add(runeData);
            }
        }

        Calculate_Scale();

        // Create the sprite array
        Initiate_Sprites();

        // Check solutions for all RiftObjs
        foreach (RiftObj riftObj in arrayOf_RiftObj)
        {
            PD.Instance.Check_IfRift_Solved(riftObj);
        }
        
        // Update yourself
        Update_RuneData();

        yield return null;
    }

    // Update onscreen runes
    public void Update_RuneData()
    {
        Check_IfSolved();

        Update_Sprite_Display();

        Update_Anim();
    }

    // Create an array of sprites
    void Initiate_Sprites()
    {
        arrayOf_Sprites = new SpriteRenderer[listOf_RuneData.Count];

        for (int i = 0; i < arrayOf_Sprites.Length; i++)
        {
            // Create a rune sprite for each runeData
            GameObject gO = new GameObject("Rune Sprite");
            gO.transform.parent = riftBox_Screen;
            // Shrink the icons a little
            gO.transform.localScale = Vector3.one * scaleFactor/riftBox_Screen.localScale.x;
            gO.transform.localPosition = new Vector3((i % rowLength)/rowLength, Mathf.Floor(i/rowLength)/rowLength, -1f) - new Vector3(offsetValue, offsetValue, 0f);
            gO.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            gO.AddComponent<SpriteRenderer>();
            arrayOf_Sprites[i] = gO.GetComponent<SpriteRenderer>();

            // Set the sprite icon based on the runeData
            listOf_RuneData[i].runeObj.GetComponent<RuneObj_Editor>().Update_RuneSprite(rl_m, arrayOf_Sprites[i], listOf_RuneData[i].runeObj);
        }
    }

    // Display the sprites in red or white depending on if they have satisfied conditions
    void Update_Sprite_Display()
    {
        for (int i = 0; i < listOf_RuneData.Count; i++)
        {
            RuneData runeData = listOf_RuneData[i];

            if (runeData.runeState == RuneState.Fail || runeData.runeState == RuneState.BecomeFail)
            {
                if (runeData.runeType == RuneType.Boss)
                {
                    arrayOf_Sprites[i].color = RL_V.iconColor_Off_Boss;
                }
                else if (runeData.runeType == RuneType.Museum)
                {
                    arrayOf_Sprites[i].color = RL_V.iconColor_Off_Museum;
                }
                else
                {
                    arrayOf_Sprites[i].color = RL_V.iconColor_Off;
                }
            }
            else if (runeData.runeState == RuneState.Succeed || runeData.runeState == RuneState.BecomeSucceed)
            {
                // Icons are different colours depending on what type they are
                if (runeData.runeType == RuneType.Boss)
                {
                    arrayOf_Sprites[i].color = RL_V.iconColor_On_Boss;
                }
                else if (runeData.runeType == RuneType.Museum)
                {
                    arrayOf_Sprites[i].color = RL_V.iconColor_On_Museum;
                }
                else
                {
                    arrayOf_Sprites[i].color = RL_V.iconColor_On;
                }
                
            }
        }
    }

    void Check_IfSolved()
    {
        isSolvedNow = true;

        foreach (RiftObj riftObj in arrayOf_RiftObj)
        {
            // If a single riftObj has not been solved, this riftBox is also unsolved
            if (!riftObj.isSolvedNow)
            {
                isSolvedNow = false;
            }
        }

        if (isSolvedNow)
        {
            if (!isSolvedOnce)
            {
                isSolvedOnce = true;
            }
        }
    }

    // When solved, activate cont animations
    void Update_Anim()
    {
        foreach (Sc_Cont_Anim animTrigger in listOf_SolvedState_Anim_Trigger)
        {
            animTrigger.Switch(isSolvedNow);
        }
    }
}
