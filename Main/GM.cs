using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GM : MonoBehaviour
{
    void Start()
    {
        // Make sure the RL dictionaries are loaded
        RL.DeclareFunction();

        InitialiseLists();
    }

    void InitialiseLists()
    {
        listOf_PlayerActions = new List<PlayerAction>();
    }

    //////////////////////////
    // STORE PLAYER ACTIONS //
    //////////////////////////

    // List of actions
    public List<PlayerAction> listOf_PlayerActions;

    // Storing an action
    public void Execute_Action(Vector3 pPlayerPos, bool pIsBridge)
    {
        listOf_PlayerActions.Add(new PlayerAction(pPlayerPos, pIsBridge));
    }

    // Undoing an action
    public PlayerAction Undo_Action()
    {
        PlayerAction recent_PlayerAction = listOf_PlayerActions.First();
        listOf_PlayerActions.Remove(recent_PlayerAction);
        return recent_PlayerAction;
    }
}