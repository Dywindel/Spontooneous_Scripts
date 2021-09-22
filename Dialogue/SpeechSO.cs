using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// An individual unit of character speech

[CreateAssetMenu(fileName = "New Speech", menuName = "Speech")]
public class SpeechSO : ScriptableObject
{
    // If the speech is selectable, will be used by player when choosing speech
    public CharacterName characterName;
    public string setName = "Selectable";
    public Sprite icon;
    public AnimationMode animationMode;
    [TextArea(10, 100)]
    public string speech;
}
