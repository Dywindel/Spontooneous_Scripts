using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// An empty generic trigger parent
public class GenericTrigger : MonoBehaviour
{
    protected string tagName = "Player";
    public bool isAuto = false;     // Some triggers will happen automatically every time
    public bool isAutoOnce = false; // Some triggers will happen automatically on the first interaction with a character
    public UnityEvent onEnter_Event;
    public UnityEvent onExit_Event;
}
