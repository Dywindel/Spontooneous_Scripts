using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This update is used for generic trigger types
// It allows a user to control whether a trigger activates automatically or on a button press

public class UM_Triggers : MonoBehaviour
{
    # region Singleton

    public static UM_Triggers Instance {get; private set; }
    private Sc_SortInput sI;

    private void Awake()
    {
        Instance = this;
    }
        
    # endregion

    private void Start()
    {
        sI = GM.Instance.gameObject.GetComponent<Sc_SortInput>();
    }

    // This boolean stops the trigger reactivating until the player has left
    [HideInInspector] public bool isActive = false;
    private bool blockPeriod = false;

    // Active trigger
    [HideInInspector] public GenericTrigger activeTrigger;

    void Update()
    {
        if (activeTrigger != null && !isActive)
        {
            // Activate the trigger upon player input
            if (sI.select || activeTrigger.isAuto || activeTrigger.isAutoOnce)
            {
                // We block interactions for a short period after quiting out of an interaction
                if (!blockPeriod)
                {
                    // Change the state of the isAutoOnce token
                    if (activeTrigger.isAutoOnce)
                    {
                        activeTrigger.isAutoOnce = false;
                    }

                    // Activate the trigger event
                    activeTrigger.onEnter_Event.Invoke();
                    isActive = true;
                }
            }
        }
    }

    // Wait a second before allowing the player to interact with an item again
    public IEnumerator Wait_BeforeReInteraction()
    {
        blockPeriod = true;
        yield return new WaitForSeconds(0.1f);
        blockPeriod = false;

        yield return null;
    }

    public void Deactivate_ActiveInteraction()
    {
        // Deactivate
        isActive = false;
    }

    public void Deactivate_TriggerReference()
    {
        activeTrigger = null;
    }
}
