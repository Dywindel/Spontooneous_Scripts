using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Main/Parent component script for Player controls

public class Sc_Player : MonoBehaviour
{
    # region Singleton

    public static Sc_Player Instance {get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    # endregion

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
    public Transform player_Relative;
    // This target is used when the player stands on a moving platform.
    // player_Relative is updated to be a child of this target
    public Transform player_Relative_Target = null;
    // Animator Component
    public Animator anim;

    // Current facing direction
    // This will need to match the starting direction the player is facing at the beginning of the game
    public DirType cur_FaceDir;
    // The direction we want to move to
    public DirType new_FaceDir;

    ///////////////
    // Animation //
    ///////////////

    // Animation coroutine
    public Coroutine cr_PlayerAnimation;
    private AnimationMode animationMode = AnimationMode.Idle;
    [HideInInspector] public MoveType moveType;  // Stores the type of player movement
    [HideInInspector] public NonMoveType nonMoveType;       // For animations

    // Store the movement values as a list
    public List<DirType> listOf_MoveCard;
    bool isMovementAnimating = false;   // This is used to check that the coroutine is currently running through the list of movement animations
    public float animationSpeed = 2f;

    void Start()
    {
        WorldReferences();

        InitialiseLists();

        Get_FaceDir();
    }

    void WorldReferences()
    {
        gM = GM.Instance;
        pD = PD.Instance;

        sI = gM.GetComponent<Sc_SortInput>();
    }

    void InitialiseLists()
    {
        listOf_MoveCard = new List<DirType>();
    }

    void Get_FaceDir()
    {
        // Find the facing direction the player is starting at
        // Transfer the rotation of y into dirType
        int yRotation = (int)(Mathf.Round(player_Animator.transform.eulerAngles.y/90f));
        cur_FaceDir = (DirType) (yRotation + 1);
    }

    ////////////////////
    // ANIMATION PRET //
    ////////////////////

    // Run animations
    public void Run_Movements_Animation()
    {
        // Perform the list of movements, if they're not already running
        if (!isMovementAnimating)
        {
            isMovementAnimating = true;
            cr_PlayerAnimation = StartCoroutine(Perform_Player_Movements());
        }
    }

    // Prepare animations states
    public void Perform_Gradual(ActionKit actionKit, DirType moveCard)
    {
        // MoveType
        if (actionKit.moveType != MoveType.None)
        {
            // Currently, there's no need to check movetype while animating
            // But this may become important later
            if (actionKit.moveType == MoveType.Step)
            {
                // Add the movement direction to the animation list and check what animation mode we're in
                listOf_MoveCard.Add(moveCard);
            }
            else if (actionKit.moveType == MoveType.Climb)
            {
                // Add the movement direction to the animation list and check what animation mode we're in
                listOf_MoveCard.Add(moveCard);
            }
        }

        // BuildType

        // Minetype

        // MetaType
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    public void Update_PlayerPos_Instant(Vector3 pPos, DirType prv_MoveCard)
    {
        // Set player position
        player_Animator.transform.position = pPos;
        player_Collider.transform.position = pPos;
        // Set player facing direction
        player_Animator.transform.rotation = Quaternion.Euler(0f, ((int)prv_MoveCard - 1) * 90f, 0f);
    }

    ///////////////
    // ANIMATION //
    ///////////////

    // Data changes that are gradual during player actions, such as animation

    IEnumerator Perform_Player_Movements()
    {
        // The player will always rotate
        // Rotate when the movement count is zero
        if (listOf_MoveCard.Count == 0)
        {
            StartCoroutine(Animate_Player_Rotate(new_FaceDir));
            cur_FaceDir = new_FaceDir;
        }

        while (listOf_MoveCard.Count > 0)
        {
            DirType dirType = listOf_MoveCard.First();
            // Start the rotation and translation animation
            // Rotate the character here too
            StartCoroutine(Animate_Player_Rotate(dirType));
            cur_FaceDir = dirType;
            
            // Wait for the animation to finish playing
            yield return Animate_Player_Translate(dirType);

            // Remove the first item from the list
            listOf_MoveCard.Remove(listOf_MoveCard.First());
        }

        isMovementAnimating = false;
        yield return null;
    }

    // Rotate the player towards the object
    // This animation takes less time than the translation animation
    public IEnumerator Animate_Player_Rotate(DirType pMoveCard)
    {
        // If we're climbing, always set the rotation value to north
        if (moveType == MoveType.Climb)
        {
            pMoveCard = DirType.N;
        }

        Quaternion startRot = player_Animator.transform.rotation;
        Quaternion endRot = Quaternion.Euler(0f, ((int)pMoveCard - 1) * 90f, 0f);

        for (float t = 0f; t < 1f; t += Time.deltaTime*animationSpeed*3.5f)
        {
            player_Animator.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        // Set player's final rotation
        player_Animator.transform.rotation = endRot;

        yield return null;
    }

    // Translate the player towards the collider object.
    IEnumerator Animate_Player_Translate(DirType pMoveCard)
    {
        // Play SFX
        AC.Instance.Audio_Character_Movement();

        Vector3 startPos = player_Animator.transform.localPosition;
        Vector3 endPos = startPos + RL.dir_To_Vect[pMoveCard];

        for (float t = 0f; t < 1f; t += Time.deltaTime*animationSpeed)
        {
            player_Animator.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Player's final position should be specific
        player_Animator.transform.localPosition = endPos;

        yield return null;
    }

    // Item animation states that update depending on player actions
    public void Update_Animation_Mode()
    {
        // Setup animation mode
        if (nonMoveType != NonMoveType.None)
        {
            animationMode = AnimationMode.Blocked;
        }
        // In climbing region
        else if (moveType == MoveType.Climb)
        {
            if (listOf_MoveCard.Count == 0)
            {
                animationMode = AnimationMode.Hang;
            }
            if (listOf_MoveCard.Count == 1)
            {
                animationMode = AnimationMode.Climb;
            }
            else if (listOf_MoveCard.Count == 2)
            {
                animationMode = AnimationMode.Scramble;
            }
        }
        else
        {
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
        }

        // Make sure to reset animation modes first
        anim.SetBool("isWalk", false);
        anim.SetBool("isHang", false);
        anim.SetBool("isClimb", false);
        
        if (animationMode == AnimationMode.Hang)
        {
            anim.SetBool("isHang", true);
        }
        else if (animationMode == AnimationMode.Climb)
        {
            anim.SetBool("isClimb", true);
            animationSpeed = 4f;
        }
        else if (animationMode == AnimationMode.Scramble)
        {
            anim.SetBool("isClimb", true);
            animationSpeed = 11f;
        }
        else if (animationMode == AnimationMode.Walking)
        {
            anim.SetBool("isWalk", true);
            animationSpeed = 4f;
        }
        else if (animationMode == AnimationMode.Running)
        {
            anim.SetBool("isWalk", true);
            animationSpeed = 11f;
        }
        else if (animationMode == AnimationMode.Blocked)
        {
            anim.SetTrigger("isBlock");
        }

        // Reset the nonMoveType
        nonMoveType = NonMoveType.None;
    }

    // Stop coroutine animation
    public void Stop_Animation()
    {
        // Cancel the animation coroutine, if it's moving
        if (cr_PlayerAnimation != null)
        {
            // Empty the list of movements
            listOf_MoveCard = new List<DirType>();
            // Reset the animation bool
            isMovementAnimating = false;
            // Stop the animation coroutine
            StopCoroutine(cr_PlayerAnimation);
        }
    }

}
