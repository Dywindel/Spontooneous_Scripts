using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rift", menuName = "Rift")]
public class RiftSO : ScriptableObject
{
    public int uniqueID;
    public string riftName;
    [TextArea]
    public string description;

    [Range(1, 10)]
    public int gsly = 1;     // Layers
    [Range(1, 25)]
    public int gsx = 5;     // Width
    [Range(1, 25)]
    public int gsy = 5;     // Length

    // Stores how many of each rune appears in these puzzles
    // 0 if there are none
    public int[] runeMap;

    public Rift_LevelData rift_LevelData;
    
    public Texture2D riftImage;

    //////////////////////
    // EDITOR FUNCTIONS //
    //////////////////////

    public void Update_RiftSO()
    {
        Setup_RiftSO();

        Collect_Elements();
    }

    public void New_RiftSO(Rift_LevelData pRift_LevelData)
    {
        rift_LevelData = pRift_LevelData;

        Setup_RiftSO();

        Populate_RiftSO();

        Collect_Elements();
    }

    public void Setup_RiftSO()
    {
        // Set the size of the runeMap to that of the enum
        runeMap = new int[System.Enum.GetValues(typeof(RuneType)).Length];
        for (int i = 0; i < runeMap.Length; i++)
        {
            // Zero out all elements
            runeMap[i] = 0;
        }
    }

    // Populate the RiftSO from the level data
    public void Populate_RiftSO()
    {
        uniqueID = rift_LevelData.riftData.uniqueID;
        riftName = rift_LevelData.riftData.riftName;
        description = rift_LevelData.riftData.comment;
        gsly = rift_LevelData.riftData.gs[0];
        gsx = rift_LevelData.riftData.gs[1];
        gsy = rift_LevelData.riftData.gs[2];
    }

    // Go through each element in the puzzle and mark it's contents
    public void Collect_Elements()
    {
        RuneData_AsSer[] arrayOf_RuneData_AsSer = rift_LevelData.arrayOf_RuneData;

        // Count Runes
        for (int i = 0; i < arrayOf_RuneData_AsSer.Length; i++)
        {
            runeMap[(int)arrayOf_RuneData_AsSer[i].runeType] += 1;
        }
    }
}
