using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores the initial state of a warp point

[CreateAssetMenu(fileName = "New Warp Point", menuName = "Warp Point")]
public class WarpSO : ScriptableObject
{
    [HideInInspector] public int index;

    public string name;

    public Sprite icon;
}
