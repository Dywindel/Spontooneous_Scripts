using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Rune object that is placed inside a puzzle grid

public class RuneObj : MonoBehaviour
{
    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    [HideInInspector]
    public GM gM;
    [HideInInspector]
    public PD pD;
    [HideInInspector]
    public AC aC;

    [HideInInspector]
    public SpriteRenderer sr;

    ///////////////////
    // USER SETTINGS //
    ///////////////////

    public RuneData runeData;
    
    public RuneType runeType = RuneType.None;
    public DirType dirType = DirType.E;
    [Range(0, 8)]
    public int numberType;
    public TapaType tapaType = TapaType.T1;
    public PinType pinType = PinType.Start;
    public PowerType powerType = PowerType.Source;
    public MuseumType museumType;
    public BossType bossType;
    // If the source of the power is currently on or off
    // Usually starts in OFF mode
    public bool powerMode = false;
    [Tooltip("This icon will show up in the editor, but not appear in the game.")] 
    public bool ignoreIcon = false;

    /////////////////////
    // SELF REFERENCES //
    /////////////////////

    public int[] arrPos;

    public void PassStart(RiftObj pParent)
    {
        // World variables
        pD = PD.Instance;

        // Self references
        SpriteRenderer sr = this.GetComponentInChildren<SpriteRenderer>();

        // Find this Rune's position relative to the Rift parent
        arrPos = RL_F.V3DFV3_IA(transform.position, pParent.transform.position);

        // Update self runeData
        runeData = new RuneData(arrPos, pParent, this);
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    // Used for switching the On/Off state of Power Sources
    public void Switch (bool isOn)
    {
        // Need to update the database of this object
        // Can also update it's object component for reference
        powerMode = isOn;
        runeData.powerMode = isOn;

        // Check if the rift belonging to this rune's parent, is solved
        pD.Check_IfRift_Solved(runeData.parent);
    }
}
