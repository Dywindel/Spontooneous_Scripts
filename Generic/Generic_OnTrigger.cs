using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Generic_OnTrigger : GenericTrigger
{
    void OnTriggerEnter(Collider collider)
    {
        // The collider for the player is attached as a child. Make sure to look at the parent.
        if (collider.transform.parent.tag == tagName)
        {
            // Pass yourself to the UM_Triggers controller
            UM_Triggers.Instance.activeTrigger = this;
            // Invoke event
            //onEnter_Event.Invoke();
        }
    }

    void OnTriggerExit(Collider collider)
    {
        // The collider for the player is attached as a child. Make sure to look at the parent.
        if (collider.transform.parent.tag == tagName)
        {
            // Run the onExit event
            if (onExit_Event != null)
            {
                onExit_Event.Invoke();
            }

            // Reset the UM_Trigger controller
            UM_Triggers.Instance.Deactivate_ActiveInteraction();
            UM_Triggers.Instance.Deactivate_TriggerReference();
        }
    }
}
