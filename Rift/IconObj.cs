using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Icon object that is place inside a puzzle grid

public class IconObj : MonoBehaviour
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

    ///////////////////
    // USER SETTINGS //
    ///////////////////

    public IconData iconData;
    
    public IconType iconType = IconType.None;
    public DirectionType directionType = DirectionType.N;
    public TapaType tapaType = TapaType.T1;
    public PinType pinType = PinType.Start;

    public void PassStart(RiftObj pParent)
    {
        // Find this Icon's position relative to the Rift parent
        int[] intArray = RL_F.Return_IntArray_Difference(transform.position, pParent.transform.position);

        // Update self iconData
        iconData = new IconData(intArray[0], intArray[1], intArray[2], pParent, this);
    }
}
