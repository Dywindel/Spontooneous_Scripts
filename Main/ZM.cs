using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This is the databox for storing the relevent puzzle in each area of the game

public class ZM : MonoBehaviour
{
    // Zone
    public Biome biome;

    // List of rift items in this zone
    public List<RiftObj> listOf_Rifts;

    // Solved Rifts as positional data
    public int[] solveState_Rifts;
    public int solveState_Total = 0;

    void Start()
    {
        InitialiseLists();
    }

    void InitialiseLists()
    {
        solveState_Rifts = new int[listOf_Rifts.Count];

        for (int i = 0; i < solveState_Rifts.Length; i++)
        {
            solveState_Rifts[i] = 0;
        }
    }

    // Calculates number of solved rifts
    public void Update_SolvedRifts()
    {
        // Data
        solveState_Total = 0;
        for (int i = 0; i < listOf_Rifts.Count; i++)
        {
            // Store the solve state of the rift as a binary value in it's correct location
            solveState_Rifts[i] = listOf_Rifts[i].isSolvedOnce ? 1 : 0;
        }

        solveState_Total = solveState_Rifts.Sum();
    }
}
