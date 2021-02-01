using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Generic_OnTrigger : MonoBehaviour
{
    public string tagName = "Player";
    public UnityEvent onEnter_Event;
    public UnityEvent onExit_Event;

    void OnTriggerEnter(Collider collider)
    {
        // The collider for the player is attached as a child. Make sure to look at the parent.
        if (collider.transform.parent.tag == tagName)
        {
            // Invoke event
            onEnter_Event.Invoke();
        }
    }

    void OnTriggerExit(Collider collider)
    {
        // The collider for the player is attached as a child. Make sure to look at the parent.
        if (collider.transform.parent.tag == tagName)
        {
            // Invoke other event
            onExit_Event.Invoke();
        }
    }
}
