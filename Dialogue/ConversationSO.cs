using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A collection of speeches
[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversation")]
public class ConversationSO : ScriptableObject
{
    // This conversation is the one that needs to keep track of SpeechIndex
    [System.NonSerialized] public int speechIndex = 0;

    public CharacterName characterName;
    public string name;
    [Tooltip ("Is This a queue of speeches or are these speeches selectable?")]
    public bool isSelectable;
    [Tooltip ("This will not show an NPC name when the dialogue pops up")]
    public bool isNonNPC;
    [Tooltip ("This speech goes before anything else, if it's not blank")]
    public SpeechSO prewarm;
    // Camera Angles
    public CameraAngle_Distance cameraAngle_Distance;
    public CameraAngle_Side cameraAngle_Side;
    public CameraAngle_Pitch cameraAngle_Pitch;
    public SpeechSO[] conversation;
}
