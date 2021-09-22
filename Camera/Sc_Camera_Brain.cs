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
    Sc_SortInput sI;
    
    /////////////////////
    // SELF REFERENCES //
    /////////////////////

    public CinemachineVirtualCamera cm_TitleMenu;
    public CinemachineVirtualCamera cm_Player;
    public CinemachineVirtualCamera cm_Rift;
    public CinemachineVirtualCamera cm_Focus;
    public CinemachineVirtualCamera cm_Artifact;
    public CinemachineVirtualCamera cm_Warp;
    public CinemachineVirtualCamera cm_NPC;

    public Transform cm_TitleMenuTarget;
    public Transform cm_RiftTarget;
    public Transform cm_FocusTarget;
    public Transform cm_ArtifactTarget;
    public Transform cm_WarpTarget;
    public Transform cm_NPCTarget;

    // Update the rift pitch
    private float rift_Pitch_Live = 0;

    [HideInInspector] public Transform audio_3DListenerTarget;

    public enum CameraType {TitleMenu, Player, Rift, Focus, Artifact, Warp, NPC};

    void Start()
    {
        WorldReferences();

        //Set_Camera_Player();
    }

    void WorldReferences()
    {
        pD = PD.Instance;
        sI = GM.Instance.GetComponent<Sc_SortInput>();
    }

    ////////////////
    // Camera Pan //
    ////////////////

    void FixedUpdate()
    {
        Update_CM_CameraPan();
    }

    ///////////////////////////
    // Switching Camera Mode //
    ///////////////////////////

    // Switch the active camera. The CameraBrain will perform smoothing
    public void Switch_Camera(CameraType cameraName)
    {
        // Switch off all cameras
        cm_TitleMenu.gameObject.SetActive(false);
        cm_Player.gameObject.SetActive(false);
        cm_Rift.gameObject.SetActive(false);
        cm_Focus.gameObject.SetActive(false);
        cm_Artifact.gameObject.SetActive(false);
        cm_Warp.gameObject.SetActive(false);
        cm_NPC.gameObject.SetActive(false);

        if (cameraName == CameraType.TitleMenu)
        {
            cm_TitleMenu.gameObject.SetActive(true);
        }
        else if (cameraName == CameraType.Player)
        {
            cm_Player.gameObject.SetActive(true);

            // Update the audio target
            audio_3DListenerTarget = Sc_Player.Instance.player_Animator.transform;
        }
        else if (cameraName == CameraType.Rift)
        {
            cm_Rift.gameObject.SetActive(true);

            audio_3DListenerTarget = cm_RiftTarget;
        }
        else if (cameraName == CameraType.Focus)
        {
            cm_Focus.gameObject.SetActive(true);

            audio_3DListenerTarget = cm_FocusTarget;
        }
        else if (cameraName == CameraType.Artifact)
        {
            cm_Artifact.gameObject.SetActive(true);

            audio_3DListenerTarget = cm_ArtifactTarget;
        }
        else if (cameraName == CameraType.Warp)
        {
            cm_Warp.gameObject.SetActive(true);

            audio_3DListenerTarget = cm_WarpTarget;
        }
        else if (cameraName == CameraType.NPC)
        {
            cm_NPC.gameObject.SetActive(true);
        }
    }

    public void Set_Camera_TitleMenu()
    {
        Switch_Camera(CameraType.TitleMenu);
    }

    // Additional functionality for setting Rift camera
    public void Set_Camera_Rift(CameraAngle_Pitch tPitch)
    {
        Update_CM_RiftTarget(tPitch);

        // Switch to the rift camera
        Switch_Camera(CameraType.Rift);
    }

    // Additional functionality for setting Player camera
    public void Set_Camera_Player()
    {
        Switch_Camera(CameraType.Player);
    }

    // Additional functionality for setting Focus camera
    public void Set_Camera_Focus(Transform t_Trans)
    {
        Update_CM_FocusTarget_Pos(t_Trans);

        Switch_Camera(CameraType.Focus);
    }

    // For Artifact focusing
    public void Set_Camera_Artifact(Vector3 tPos, CameraAngle_Distance tCameraDistance, CameraAngle_Side tCameraSide, CameraAngle_Pitch tCameraPitch, Vector3 tCameraOffset)
    {
        Update_CM_Artifact(tPos, tCameraDistance, tCameraSide, tCameraPitch, tCameraOffset);

        Switch_Camera(CameraType.Artifact);
    }

    // For the warp screen
    public void Set_Camera_WarpScreen(Transform t_Trans)
    {
        Update_CM_Warp(t_Trans);

        Switch_Camera(CameraType.Warp);
    }

    public void Set_Camera_NPC(Vector3 tPos, CameraAngle_Distance tCameraDistance, CameraAngle_Side tCameraSide, CameraAngle_Pitch tCameraPitch)
    {
        Update_CM_NPC(tPos, tCameraDistance, tCameraSide, tCameraPitch);

        Switch_Camera(CameraType.NPC);
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    // This function centres the target position for the CM Rift camera
    public void Update_CM_RiftTarget(CameraAngle_Pitch tPitch)
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
        if (pD.activeRift.overrideCamera == null)
        {
            // Calculate target
            cm_RiftTarget.position =    pD.activeRift.transform.position +
                                    new Vector3(pD.activeRift.gsx/2f - 0.5f, pD.activeRift.gsly/2f - 0.5f, pD.activeRift.gsy/2f - 0.5f);
        }
        else
        {
            // Target set by user for cinematic effects
            cm_RiftTarget.position = pD.activeRift.overrideCamera.transform.position;
        }

        // Set the angle
        Vector3[] rCamera = RL_F.Select_CameraAngle(CameraAngle_Distance.Close, CameraAngle_Side.Centre, tPitch);
        rift_Pitch_Live = rCamera[0].x;

        cm_Rift.transform.rotation = Quaternion.Euler(rCamera[0].x, cm_Rift.transform.rotation.x, cm_Rift.transform.rotation.z);
        
    }

    // This function places the focus position based on an input transform
    public void Update_CM_FocusTarget_Pos(Transform t_Trans)
    {
        cm_FocusTarget.transform.position = t_Trans.position;
    }

    public void Update_CM_Artifact(Vector3 tPos, CameraAngle_Distance tCameraDistance, CameraAngle_Side tCameraSide, CameraAngle_Pitch tCameraPitch, Vector3 tCameraOffset)
    {
        // First, grab the offset and angle values based on the settings
        Vector3[] rCamera = RL_F.Select_CameraAngle(tCameraDistance, tCameraSide, tCameraPitch);

        // Now update the camera's component values
        CinemachineComponentBase componentBase = cm_Artifact.GetCinemachineComponent(CinemachineCore.Stage.Body);
        (componentBase as CinemachineFramingTransposer).m_CameraDistance = rCamera[0].z;
        cm_Artifact.transform.rotation = Quaternion.Euler(rCamera[0].x, rCamera[0].y, cm_Artifact.transform.rotation.z);

        // If the camera offset is zero, just use the calculated one, otherwise use the value as has been set
        if (tCameraOffset != Vector3.zero)
        {
            rCamera[1] = tCameraOffset;
        }

        // We want the artifact to appear side-by-side with a text box
        // Let's slightly shift the focus a little back and to the right
        cm_ArtifactTarget.transform.position = tPos + rCamera[1];
    }

    public void Update_CM_Warp(Transform t_Trans)
    {
        cm_WarpTarget.transform.position = t_Trans.position + new Vector3(0f, 1.1f, 1f);
    }

    // For NPC focused conversation
    public void Update_CM_NPC(Vector3 tPos, CameraAngle_Distance tCameraDistance, CameraAngle_Side tCameraSide, CameraAngle_Pitch tCameraPitch)
    {
        // First, grab the offset and angle values based on the settings
        Vector3[] rCamera = RL_F.Select_CameraAngle(tCameraDistance, tCameraSide, tCameraPitch);

        // Now update the camera's component values
        CinemachineComponentBase componentBase = cm_NPC.GetCinemachineComponent(CinemachineCore.Stage.Body);
        (componentBase as CinemachineFramingTransposer).m_CameraDistance = rCamera[0].z;
        cm_NPC.transform.rotation = Quaternion.Euler(rCamera[0].x, rCamera[0].y, cm_NPC.transform.rotation.z);

        // We want the artifact to appear side-by-side with a text box
        // Let's slightly shift the focus a little back and to the right
        cm_NPCTarget.transform.position = tPos + 0.5f*rCamera[1] + new Vector3(0f, 0f, -0.5f);
    }

    // This function slightly changes the angle of the active camera based on the right thumbstick
    public void Update_CM_CameraPan()
    {
        // Take the active camera and change it's base angle rotation based on the right thumbstick
        cm_Player.transform.rotation = Quaternion.Lerp(cm_Player.transform.rotation, Quaternion.Euler(60f - 20f*sI.cameraPan.y, 0f + 15f*sI.cameraPan.x, 0f), 4f*Time.deltaTime);
        cm_Rift.transform.rotation = Quaternion.Lerp(cm_Rift.transform.rotation, Quaternion.Euler(rift_Pitch_Live + 10f*((sI.cameraPan.y - 0.5f*Mathf.Abs(sI.cameraPan.y)) + 0.5f*sI.cameraPan.y), 0f, 0f), 4f*Time.deltaTime);
        cm_Focus.transform.rotation = Quaternion.Lerp(cm_Focus.transform.rotation, Quaternion.Euler(60f - 20f*sI.cameraPan.y, 0f + 20f*sI.cameraPan.x, 0f), 4f*Time.deltaTime);
    }

    ////////////////
    // ANIMATIONS //
    ////////////////

    public IEnumerator Warp_Zoom(bool isZoomIn)
    {
        CinemachineComponentBase componentBase = cm_Warp.GetCinemachineComponent(CinemachineCore.Stage.Body);
        float tf = 1f;
        if (!isZoomIn)
        {
            tf = 0f;
        }

        for (float t = 0; t < 1f; t += Time.deltaTime * 2f)
        {
            (componentBase as CinemachineFramingTransposer).m_CameraDistance = Mathf.Abs(tf - t)*0.9f + 0.5f;
            yield return null;
        }

        (componentBase as CinemachineFramingTransposer).m_CameraDistance = Mathf.Abs(tf - 1f)*0.9f + 0.5f;
        yield return null;
    }
}
