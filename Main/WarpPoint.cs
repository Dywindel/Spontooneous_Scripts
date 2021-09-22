using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a controller for an individual warp point

public class WarpPoint : MonoBehaviour
{
    // World objects
    WC wC;
    Sc_Camera_Brain camera_Brain;

    public int index;   // The index of where we're going to

    void Start()
    {
        wC = WC.Instance;
        camera_Brain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Sc_Camera_Brain>();

        wC.arrayOf_WarpPoints[index] = this;
    }

    public void OnActivate()
    {
        // Send and activate the Warp Menu
        wC.OnActivate(index);

        // Reposition the camera
        camera_Brain.Set_Camera_WarpScreen(transform);
    }
}
