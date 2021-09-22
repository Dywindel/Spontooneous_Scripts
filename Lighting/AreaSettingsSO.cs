using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Area Settings", menuName = "Area Settings")]
public class AreaSettingsSO : ScriptableObject
{
    public string areaName;
    
    [Header("Sound")]
    [FMODUnity.EventRef]
    public string fmodEvent_Atmosphere;
    [FMODUnity.EventRef]
    public string fmodEvent_Music;

    [Header("Lighting")]
    public Color colourA;
    public Color colourB;
    public float lightIntensity;

    [Header("Fog")]
    public Color fogColour;
    public Vector2 fogLinear = Vector2.zero;
}
