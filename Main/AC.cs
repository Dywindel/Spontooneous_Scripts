using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Audio Controller for in-game audio
public class AC : MonoBehaviour
{
    #region Singleton

    public static AC Instance {get; private set; }

    private void Awake()
    {
        Instance = this;
    }
        
    #endregion

    private Sc_Camera_Brain camera_Brain;
    public Transform audio_3DListenerTarget;

    // Area settings
    [HideInInspector] public AreaSettingsSO areaSettingsSO;

    // SFX Instances
    FMOD.Studio.EventInstance characterMovement;
    [HideInInspector] public FMOD.Studio.EventInstance sfx_Machinery_Pulley;

    // AMB Instances
    FMOD.Studio.EventInstance amb_Atmosphere;
    FMOD.Studio.EventInstance amb_Computer_Hum;
    FMOD.Studio.EventInstance amb_ComputerHum_Shutdown;

    // Music Instances
    FMOD.Studio.EventInstance mus_Music;

    void Start()
    {
        World_References();

        Create_AudioInstances();
    }

    void World_References()
    {
        camera_Brain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Sc_Camera_Brain>();
    }

    // Setup audio instances as the game starts
    void Create_AudioInstances()
    {
        // SFX Instances
        characterMovement = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Character/Character_Movement");
        sfx_Machinery_Pulley = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Machinery/Machinery_ChainPulley");

        // AMB Instances
        amb_Atmosphere = FMODUnity.RuntimeManager.CreateInstance("event:/AMB/RGN/AMB_RGN_Canopy");

        amb_Computer_Hum = FMODUnity.RuntimeManager.CreateInstance("event:/AMB/Computer/AMB_Computer_Hum");
        amb_ComputerHum_Shutdown = FMODUnity.RuntimeManager.CreateInstance("event:/AMB/Computer/AMB_Computer_Shutdown");

        // MUS Instances
        mus_Music = FMODUnity.RuntimeManager.CreateInstance("event:/MUS/MUS_Museum");
    }

    ///////////////
    // 3D Target //
    ///////////////

    void LateUpdate()
    {
        Update_3DListenerTarget_Position();
    }

    // The 3D audio listener target will follow the CM targets inside the cameraBrain script
    // It will update it's target along with the camera
    void Update_3DListenerTarget_Position()
    {
        // For now, this has an issue, so I'm just going to check that the camera brain is not null
        if (camera_Brain.audio_3DListenerTarget != null)
        {
            audio_3DListenerTarget.position = Vector3.Lerp(audio_3DListenerTarget.position, camera_Brain.audio_3DListenerTarget.position, 0.9f * Time.deltaTime);
        }
    }

    /////////
    ///////// SFX
    /////////

    //////////////////
    // Audio for UI //
    //////////////////

    public void OneShot_UI_Keys()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Computer/Select_Keys", GetComponent<Transform>().position);
    }

    public void OneShot_UI_Enter()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Computer/Select_Enter", GetComponent<Transform>().position);
    }

    //////////////////////////
    // Audio for Characters //
    //////////////////////////

    public void Audio_Character_Movement()
    {
        // Set the sound parameter label
        characterMovement.setParameterByName("Ground Type", 0);
        // Play the sound
        characterMovement.start();
    }
    
    ///////////////////////
    // Audio for Puzzles //
    ///////////////////////

    public void SFX_Bridge_Place()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Bridges/Bridge_Place", GetComponent<Transform>().position);
    }

    public void SFX_Icon_Switch(bool isOn)
    {
        if (isOn)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Icons/Icon_On", GetComponent<Transform>().position);
        }
        else
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Icons/Icon_Off", GetComponent<Transform>().position);
        }
    }

    /////////////////////////
    // Audio for Artifacts //
    /////////////////////////
    
    public void SFX_RTF_Clap()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/RTF/RTF_Clap", GetComponent<Transform>().position);
    }

    public void SFX_RTF_Bridge_Turning()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/RTF/RTF_Bridge_Turning", GetComponent<Transform>().position);
    }

    ///////////////////////
    // SFX for Machinery //
    ///////////////////////

    public void Update_Parameter(FMOD.Studio.EventInstance eventInstance, string parameter, float val)
    {
        eventInstance.setParameterByName(parameter, val);
    }

    public void SFX_Machinery_Pulley(bool isOn)
    {
        if (isOn)
        {
            sfx_Machinery_Pulley.start();
        }
        else
        {
            sfx_Machinery_Pulley.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    //////////
    ////////// AMB & MUS
    //////////

    // Play Atmosphere/Music
    public void Activate_AreaSound(AreaSettingsSO pAreaSettingsSO)
    {
        // Only update if the passed area settings are different to the current ones
        if (pAreaSettingsSO != areaSettingsSO)
        {
            areaSettingsSO = pAreaSettingsSO;

            // Stop the currently playing ambience
            amb_Atmosphere.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            // Stop the currently playing Music
            mus_Music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            // Ambience
            // If the current sound is not empty
            if (areaSettingsSO.fmodEvent_Atmosphere != null)
            {
                amb_Atmosphere = FMODUnity.RuntimeManager.CreateInstance(areaSettingsSO.fmodEvent_Atmosphere);
                amb_Atmosphere.start();
            }

            // Music
            // If the current sound is not empty
            if (areaSettingsSO.fmodEvent_Music != null)
            {
                mus_Music = FMODUnity.RuntimeManager.CreateInstance(areaSettingsSO.fmodEvent_Music);
                mus_Music.start();
            }
        }
    }

    public void AMB_Computer_Hum(bool isHum)
    {
        // Start the sounds
        if (isHum)
        {
            amb_Computer_Hum.start();
        }
        else
        {
            amb_Computer_Hum.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            amb_ComputerHum_Shutdown.start();
        }
    }
}
