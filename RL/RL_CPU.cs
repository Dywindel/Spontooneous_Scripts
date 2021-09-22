using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls much of the operation speed of items in the game world
// As the player powers up the Mountain/Machine further through the game
// The speed at which animations player, increases

public class RL_CPU : MonoBehaviour
{
    [Range(1f, 100f)]
    public float cpu_Speed_Mulitplier = 1f;
}
