using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// These Reference Library values are used throughout
// The entire game project

// This is a library for values specific to the game

public static class RL_V
{
    // Memory
    public static int maxBuffer = 300;

    // Input
    public static float timer_ButtonRepeat = 0.25f;

    // Raycasting
    public static float cardUnit = 1f;
    public static float vertUnit = 1f;      // May want to update this later

    // Visuals
    public static float waterLevel = 0f;
    public static float SandLevel = -2f;
    public static float laserMaxDistance = 100f;

    // Colours
    public static Color iconColor_On = Color.white;
    public static Color iconColor_On_Museum = new Color(1f, 0.5f, 0f, 1f);
    public static Color iconColor_On_Boss = Color.red;
    public static Color iconColor_Off = Color.gray;
    public static Color iconColor_Off_Museum = new Color(0.5f, 0.25f, 0f, 1f);
    public static Color iconColor_Off_Boss = new Color(0.5f, 0f, 0f, 1f);
}
