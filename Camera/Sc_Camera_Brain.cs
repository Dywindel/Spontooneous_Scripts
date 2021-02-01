using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// A brain to control which camera is active and inactive

public class Sc_Camera_Brain : MonoBehaviour
{
    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    PD pD;
    
    /////////////////////
    // SELF REFERENCES //
    /////////////////////

    public CinemachineVirtualCamera cm_Player;
    public CinemachineVirtualCamera cm_Rift;

    public Transform cm_RiftTarget;

    public enum cameaType {Player, Rift};

    void Start()
    {
        WorldReferences();
    }

    void WorldReferences()
    {
        pD = GameObject.FindGameObjectWithTag("PD").GetComponent<PD>();
    }

    // Switch the active camera. The CameraBrain will perform smoothing
    public void Switch_Camera(cameaType cameraName)
    {
        // Switch off all cameras
        cm_Player.gameObject.SetActive(false);
        cm_Rift.gameObject.SetActive(false);

        if (cameraName == cameaType.Player)
        {
            cm_Player.gameObject.SetActive(true);
        }
        else if (cameraName == cameaType.Rift)
        {
            cm_Rift.gameObject.SetActive(true);
        }
    }

    // Additional functionality for setting rift camera
    public void Set_Camera_Rift()
    {
        // Position the Cm_Rift camera target
        Update_CM_RiftTarget_Pos();

        // Switch to the rift camera
        Switch_Camera(cameaType.Rift);
    }

    // Additional functionality for setting player camera
    public void Set_Camera_Player()
    {
        Switch_Camera(cameaType.Player);
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    // This function centres the target position for the CM Rift camera
    public void Update_CM_RiftTarget_Pos()
    {
        // I took a sampling of different camera positions and sizes for different levels sizes
        // These two equations are my rough initial values for camera positions

        // Find max grid size value if larger than 8
        float maxGS = 8;
        if (pD.activeRift.gsx > maxGS)
            maxGS = pD.activeRift.gsx;
        if (pD.activeRift.gsy > maxGS)
            maxGS = pD.activeRift.gsy;

        // Setup for changing virtual camera settings
        CinemachineComponentBase componentBase = cm_Rift.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (componentBase is CinemachineFramingTransposer)
        {
            // Change camera set distance
            (componentBase as CinemachineFramingTransposer).m_CameraDistance = (maxGS * 0.8f) + 2f;
            // Change camera zOffset
            (componentBase as CinemachineFramingTransposer).m_TrackedObjectOffset = new Vector3(0f, 0f, -maxGS*0.1f);
        }

        // Set the centre position of the target
        cm_RiftTarget.position =    pD.activeRift.transform.position +
                                    new Vector3(pD.activeRift.gsx/2f - 0.5f, pD.activeRift.gsly/2f - 0.5f, pD.activeRift.gsy/2f - 0.5f);

        // Set the follow distance, which will be different for different puzzles sizes

    }
}
