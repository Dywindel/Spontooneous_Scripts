using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Main/Parent component script for Player controls

public class Sc_Player : MonoBehaviour
{
    //////////////////////
    // WORLD REFERENCES //
    //////////////////////

    GM gM;
    PD pD;
    Sc_SortInput sI;

    /////////////////////
    // SELF REFERENCES //
    /////////////////////

    public GameObject player_Collider;
    public GameObject player_Animator;

    // private MoveCard faceDir = MoveCard.N;      // This will need to match the starting direction the player is facing at the beginning of the game

    ///////////////
    // Animation //
    ///////////////

    private AnimationMode animationMode = AnimationMode.Idle;

    // Store the movement values as a list
    public List<MoveCard> listOf_MoveCard;
    bool isMovementAnimating = false;   // This is used to check that the coroutine is currently running through the list of movement animations
    public float animationSpeed = 2f;

    void Start()
    {
        WorldReferences();

        InitialiseLists();
    }

    void WorldReferences()
    {
        gM = GameObject.FindGameObjectWithTag("GM").GetComponent<GM>();
        pD = GameObject.FindGameObjectWithTag("PD").GetComponent<PD>();

        sI = gM.GetComponent<Sc_SortInput>();
    }

    void InitialiseLists()
    {
        listOf_MoveCard = new List<MoveCard>();
    }

    void Update()
    {
        // Check for movement input
        if (sI.isMove)
        {
            Prepare_Move(sI.moveCard);
        }

        // Check for undo
        if (sI.undo)
        {
            Load_Action();
        }

        // We constantly check the animation mode
        Update_Animation_Mode();
    }

    //////////
    // DATA //
    //////////

    // This is the bulk of movement for the player in the game
    // And what effects the player has on moving in the world
    void Prepare_Move(MoveCard pMoveCard)
    {
        // Check if movement is possible based on game rules and return what type of movement to perform
        MoveType moveType = Check_MoveType(pMoveCard);
        if (moveType == MoveType.Step)
        {
            // Perform movement and store action
            Perform_Action(pMoveCard, false);
            Store_Action(false);
        }
        else if (moveType == MoveType.Bridge)
        {
            Perform_Action(pMoveCard, true);
            Store_Action(true);
        }
        else if (moveType == MoveType.None)
        {
            // Perform animation showing why movement isn't allowed
        }
    }

    // Check if movement is possible, based on game rules
    // And, what type of movement that might be
    MoveType Check_MoveType(MoveCard pMoveCard)
    {
        // 1. Check for a rock
        // A rock in front of the player will block player's movement
        // Use raycasting (For now)
        Ray ray_Forward = new Ray(player_Collider.transform.position, RL.card_To_Vect[pMoveCard]);
        RaycastHit[] hit_Rock;
        LayerMask mask_Rock = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Rock));
        LayerMask mask_Floor = LayerMask.GetMask(LayerMask.LayerToName((int)CollLayers.Rock), LayerMask.LayerToName((int)CollLayers.Bridge));

        hit_Rock = Physics.RaycastAll(ray_Forward, 1f, mask_Rock);

        // If we hit something, we can't move
        if (hit_Rock.Length > 0)
        {
            return MoveType.None;
        }

        // 2. Check for a floor (rock that is below and in front of us)
        // When the player is not inside a puzzle
        Ray ray_DownInFront = new Ray(player_Collider.transform.position + RL.card_To_Vect[pMoveCard], Vector3.down);
        RaycastHit[] hit_Floor;

        hit_Floor = Physics.RaycastAll(ray_DownInFront, 1f, mask_Floor);

        // If there is no floor, when not inside a rift, we can't move onto that space (Currently)
        if (hit_Floor.Length == 0 && pD.activeRift == null)
        {
            return MoveType.None;
        }

        // 3. Are we inside a puzzle?
        if (pD.activeRift != null)
        {
            // 4. Is there a floor?
            if (hit_Floor.Length == 0)
            {
                // 5. Do we have enough bridge pieces left?
                // For now, we have infinite
                if (pD.activeRift.bridgePiecesRemaining > 0)
                {
                    return MoveType.Bridge;
                }
                else
                {
                    return MoveType.None;
                }
            }

            // 1. Inside the grid
            // The player cannot move outside the grid unless there is a floor item there
            // And they are leaving in the direction I allow them to (Without solving the puzzle yet)
            // Or if the puzzle has been solved

        }

        return MoveType.Step;
    }

    ////////////////
    // MOVE TYPES //
    ////////////////

    // Perform the disgnated player action
    void Perform_Action(MoveCard pMoveCard, bool placeBridge)
    {
        // Update the player position inside the active rift, if one is active
        if (pD.activeRift != null)
        {
            pD.activeRift.Update_PlayerRift_Pos(pMoveCard);
        }

        // Add the movement direction to the animation list and check what animation mode we're in
        listOf_MoveCard.Add(pMoveCard);

        // Perform the list of movements, if they're not already running
        if (!isMovementAnimating)
        {
            isMovementAnimating = true;
            StartCoroutine(Perform_Player_Movements());
        }

        // If we're placing a bridge, do that here
        if (placeBridge)
        {
            // Place a bridge piece into the movement target location, where the player is currently
            pD.activeRift.Place_Bridge(pD.activeRift.player_RiftPos);
        }
    }

    // Store action
    // Stores what action the player just took and what information needs to be stored in the DB
    void Store_Action(bool placeBridge)
    {
        gM.Execute_Action(player_Collider.transform.position, placeBridge);
    }

    void Load_Action()
    {
        // Load the first item from the action list
        if (gM.listOf_PlayerActions.Count > 0)
        {
            PlayerAction playerAction = gM.listOf_PlayerActions.Last();
            gM.listOf_PlayerActions.Remove(playerAction);

            // Place the player there
            player_Collider.transform.position = playerAction.playerPos;
            player_Animator.transform.position = playerAction.playerPos;
            // Remove the most recent bridge
            if (playerAction.isBridge)
            {
                // Come back to this later, it's pretty complicated
            }
        }
    }

    ///////////////
    // ANIMATION //
    ///////////////

    IEnumerator Perform_Player_Movements()
    {
        while (listOf_MoveCard.Count > 0)
        {
            // Teleport the collider
            player_Collider.transform.Translate(RL.card_To_Vect[listOf_MoveCard.First()]);

            // Wait for the animation to finish playing
            yield return Animate_Player_Translate(listOf_MoveCard.First());

            // Remove the first item from the list
            listOf_MoveCard.Remove(listOf_MoveCard.First());
        }

        isMovementAnimating = false;
        yield return null;
    }

    // Translate the player towards the collider object.
    IEnumerator Animate_Player_Translate(MoveCard pMoveCard)
    {
        Vector3 startPos = player_Animator.transform.position;
        Vector3 endPos = startPos + RL.card_To_Vect[pMoveCard];

        for (float t = 0f; t < 1f; t += Time.deltaTime*animationSpeed)
        {
            player_Animator.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Player's final position should be specific
        player_Animator.transform.position = endPos;

        yield return null;
    }

    // Item animation states that update depending on player actions
    void Update_Animation_Mode()
    {
        // Setup animation mode
        if (listOf_MoveCard.Count == 0)
        {
            animationMode = AnimationMode.Idle;
        }
        if (listOf_MoveCard.Count == 1)
        {
            animationMode = AnimationMode.Walking;
        }
        else if (listOf_MoveCard.Count == 2)
        {
            animationMode = AnimationMode.Running;
        }

        // Perform animation mode
        if (animationMode == AnimationMode.Idle)
        {

        }
        else if (animationMode == AnimationMode.Walking)
        {
            animationSpeed = 4f;
        }
        else if (animationMode == AnimationMode.Running)
        {
            animationSpeed = 11f;
        }
        else if (animationMode == AnimationMode.Blocked)
        {

        }
    }
}
