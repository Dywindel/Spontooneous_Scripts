using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Editor scripting for the runeObj

public class RuneObj_Editor : MonoBehaviour
{
    //////////
    // DATA //
    //////////

    // Pass any relevent information into the RiftObj, rather than having to grab it each time upon startup?
    public void Update_RiftData()
    {
        
    }

    // For editor scripts, setting Rune loadout
    public void Unpack_RuneSettings(RuneData_AsSer runeData_AsSer)
    {
        RuneObj runeObj = this.GetComponent<RuneObj>();

        runeObj.runeType = runeData_AsSer.runeType;
        runeObj.dirType = runeData_AsSer.dirType;
        runeObj.numberType = runeData_AsSer.numberType;
        runeObj.tapaType = runeData_AsSer.tapaType;
        runeObj.pinType = runeData_AsSer.pinType;
        runeObj.powerType = runeData_AsSer.powerType;
        runeObj.powerMode = runeData_AsSer.powerMode;
        try
        {
            runeObj.ignoreIcon = runeData_AsSer.ignoreIcon;
        }
        catch (Exception e)
        {
            print("RuneObj class not uptodate: " + e);
            runeObj.ignoreIcon = false;
        }
    }

    /////////////
    // VISUALS //
    /////////////

    public void Update_Rune()
    {
        // We manually grab references when inside the editor
        #if UNITY_EDITOR

        RL.DeclareFunction();

        // Grab the reference library for textures and meshes
        RL_M rl_m = GameObject.FindObjectOfType<GM>().GetComponent<RL_M>();
        // Grab the runeObj script
        RuneObj runeObj = this.GetComponent<RuneObj>();
        // Grab the sprite renderer
        SpriteRenderer sr = this.GetComponentInChildren<SpriteRenderer>();

        // Change the gameobject name, so it's easier for me to work with
        runeObj.name = "Rune_" + runeObj.runeType.ToString();

        if (runeObj.runeType == RuneType.Museum)
        {
            runeObj.name = "Rune_" + runeObj.runeType.ToString() + "_" + runeObj.museumType.ToString();
        }
        else if (runeObj.runeType == RuneType.Boss)
        {
            runeObj.name = "Rune_" + runeObj.runeType.ToString() + "_" + runeObj.bossType.ToString();
        }

        Update_RuneSprite(rl_m, sr, runeObj);

        // Translation
        // For most cases, the rune will remain at Zero
        if (runeObj.runeType == RuneType.Mirror)
        {
            // Move the rune in the direction of its dirType
            sr.transform.localPosition = 0.5f*RL.dir_To_Vect[runeObj.dirType] + 0.48f*Vector3.down;
        }
        else
        {
            sr.transform.localPosition = Vector3.zero + 0.48f*Vector3.down;
        }

        // Rotation
        
        // For arrow, mirror and chevron, the rune needs to rotate based on the dirType
        if (runeObj.runeType == RuneType.Arrow || runeObj.runeType == RuneType.Chevron || runeObj.runeType == RuneType.Mirror ||
                    runeObj.runeType == RuneType.ArrowSingle)
        {
            sr.gameObject.transform.rotation = Quaternion.Euler(90f, 90f * ((int)runeObj.dirType - 1), 0f);
        }
        // For most cases, the rune rotation will stay as the identity
        else
        {
            sr.gameObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        #endif
    }

    // Update sprite based on runedata
    public void Update_RuneSprite(RL_M rl_m, SpriteRenderer sr, RuneObj runeObj)
    {
        if (runeObj.runeType == RuneType.Museum)
        {
            sr.sprite = rl_m.runeMuseumTextures[(int)runeObj.museumType];
        }
        else if (runeObj.runeType == RuneType.Boss)
        {
            sr.sprite = rl_m.runeBossTextures[(int)runeObj.bossType];
        }
        else if (runeObj.runeType == RuneType.Tapa)
        {
            sr.sprite = rl_m.runeTextures_Tapa[(int)runeObj.tapaType];
        }
        else if (runeObj.runeType == RuneType.Pin)
        {
            sr.sprite = rl_m.runeTextures_Pin[(int)runeObj.pinType];
        }
        else if (runeObj.runeType == RuneType.Power)
        {
            sr.sprite = rl_m.runeTextures_Power[(int)runeObj.powerType];
        }
        else if (runeObj.runeType == RuneType.Exits)
        {
            // For numbers 1 - 4 inclusive
            if (runeObj.numberType >= 1 && runeObj.numberType <= 4)
            {
                sr.sprite = rl_m.runeTextures_Exits[runeObj.numberType - 1];
            }
        }
        else
        {
            sr.sprite = rl_m.runeTextures[(int)runeObj.runeType];
        }
    }
}
