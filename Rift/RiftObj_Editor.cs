using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Editor scripting for the Rift Object

public class RiftObj_Editor : MonoBehaviour
{

    /////////////
    // VISUALS //
    /////////////

    public void Update_Grid()
    {
        // Change the box collider trigger size
        RiftObj riftObj = this.GetComponent<RiftObj>();
        riftObj.areaCollider.center = new Vector3(riftObj.gsx/2f - 0.5f, riftObj.gsly/2f - 0.5f, riftObj.gsy/2f - 0.5f);
        riftObj.areaCollider.size = new Vector3(riftObj.gsx, riftObj.gsly, riftObj.gsy);
    }

    /////////////////
    // PERSISTANCE //
    /////////////////

    public void Save_Rift_Layout()
    {

    }

    public void Load_Rift_Layout()
    {
        
    }
}
