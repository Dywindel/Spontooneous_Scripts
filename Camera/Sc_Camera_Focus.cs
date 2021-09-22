using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This object, when it's trigger is entered by the player
// Pulls the camera to a focus point placed by the user

public class Sc_Camera_Focus : MonoBehaviour
{

    GM gM;
    Sc_Camera_Brain cameraBrain;
    public Transform focusPos;      // The position of the camera focus point

    // Start is called before the first frame update
    void Start()
    {
        gM = GM.Instance;
        cameraBrain = gM.cameraBrain;
    }

    public void OnFocus()
    {
        cameraBrain.Set_Camera_Focus(focusPos);
    }

    public void OnFocusExit()
    {
        cameraBrain.Set_Camera_Player();
    }
}
