using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Artifact SO

[CreateAssetMenu(fileName = "New Artifact", menuName = "Artifact")]
public class ArtifactSO : ScriptableObject
{
    public string name; // For easier sorting
    public int id; // Each artifact must have a unique ID.

    [Header("Item Details")]
    public ArtifactState artifactState;
    public GameObject obj;
    [TextArea(10, 50)]
    public string description;
}
