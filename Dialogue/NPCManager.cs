using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This script is given to an NPC
// It manages conversations, camera, and updates certain states=

public class NPCManager : MonoBehaviour
{
    public Animator anim;
    public ConversationSO conversationSO;       // This is the conversationsSO that will run for this character
    public SpeechSO bark;                       // Some NPCs will just have single conversation barks
    public Transform focusPoint;
    public GameObject hat;
    public Transform head;         // Can have idle head animations, or look at character
    public enum HeadAnimation {None, LookAround, Music, Reading};
    public HeadAnimation headAnimation;
    private Quaternion head_StartRotation;
    public bool repeat_LastLine = true;    // Either repeat the last bit on conversation, or switch off talking alltogether
    public bool lookAtPlayer = false;
    [HideInInspector] public bool hasReachedLastLine = false;     // Useful for running events

    // Event that plays once at the start of a conversation
    public UnityEvent event_StartConversation;
    // Event that plays once at the very end of a conversation
    public UnityEvent event_EndConversation;

    // When the game starts, animate the head, if it's in animation mode and there is a head
    public void Start()
    {
        if (head != null && headAnimation != HeadAnimation.None)
        {
            // Store the start rotation
            head_StartRotation = head.rotation;

            StartCoroutine(AnimateHead());
        }
    }

    public IEnumerator AnimateHead()
    {
        while (true)
        {
            if (!lookAtPlayer)
            {
                if (headAnimation == HeadAnimation.LookAround)
                {
                    // Randomly look around
                    Quaternion randomRotation = head_StartRotation * Quaternion.Euler(Random.Range(-30f, 30f), Random.Range(-30f, 30f), 0f);
                    // Slerp towards item
                    for (float t = 0; t < 1f; t += 0.5f*Time.deltaTime)
                    {
                        Quaternion slerpRotation = Quaternion.Slerp(head.rotation, randomRotation, f_Sigmoid(t));
                        head.rotation = slerpRotation;

                        yield return null;
                    }
                    // Hold for a random time
                    yield return new WaitForSeconds(Random.Range(1f, 4f));
                }
                else if (headAnimation == HeadAnimation.Music)
                {
                    // Look slightly to the left and down
                    Quaternion newRotation = head_StartRotation * Quaternion.Euler(-10f, -20f, 0f);
                    // Slerp towards item
                    for (float t = 0; t < 1f; t += 4*Time.deltaTime)
                    {
                        Quaternion slerpRotation = Quaternion.Slerp(head.rotation, newRotation, f_Sigmoid(t));
                        head.rotation = slerpRotation;

                        yield return null;
                    }
                    // Back to centre and up
                    newRotation = head_StartRotation * Quaternion.Euler(10f, 0f, 0f);
                    // Slerp towards item
                    for (float t = 0; t < 1f; t += 4*Time.deltaTime)
                    {
                        Quaternion slerpRotation = Quaternion.Slerp(head.rotation, newRotation, f_Sigmoid(t));
                        head.rotation = slerpRotation;

                        yield return null;
                    }
                    // Look slightly to the right and down
                    newRotation = head_StartRotation * Quaternion.Euler(-10f, 20f, 0f);
                    // Slerp towards item
                    for (float t = 0; t < 1f; t += 4*Time.deltaTime)
                    {
                        Quaternion slerpRotation = Quaternion.Slerp(head.rotation, newRotation, t);
                        head.rotation = slerpRotation;

                        yield return null;
                    }
                    // Back to centre and up
                    newRotation = head_StartRotation * Quaternion.Euler(10f, 0f, 0f);
                    // Slerp towards item
                    for (float t = 0; t < 1f; t += 4*Time.deltaTime)
                    {
                        Quaternion slerpRotation = Quaternion.Slerp(head.rotation, newRotation, f_Sigmoid(t));
                        head.rotation = slerpRotation;

                        yield return null;
                    }
                }
                else if (headAnimation == HeadAnimation.Reading)
                {
                    float side = 1f;
                    if (Random.Range(0f, 1f) < 0.3f)
                    {
                        side = -1f;
                    }

                    // Make a small movement from right to left
                    Quaternion rightRotation = head_StartRotation * Quaternion.Euler(-20f, side*20f, 0f);
                    Quaternion leftRotation = head_StartRotation * Quaternion.Euler(-20f, 0, 0f);
                    // Slerp towards start
                    for (float t = 0; t < 1f; t += 1*Time.deltaTime)
                    {
                        Quaternion slerpRotation = Quaternion.Slerp(head.rotation, rightRotation, f_Sigmoid(t));
                        head.rotation = slerpRotation;

                        yield return null;
                    }
                    // Slerp towards item
                    for (float t = 0; t < 1f; t += 1*Time.deltaTime)
                    {
                        Quaternion slerpRotation = Quaternion.Slerp(rightRotation, leftRotation, f_Sigmoid(t));
                        head.rotation = slerpRotation;

                        yield return null;
                    }
                    for (float t = 0; t < 1f; t += 2.5f*Time.deltaTime)
                    {
                        Quaternion slerpRotation = Quaternion.Slerp(leftRotation, rightRotation, f_Sigmoid(t));
                        head.rotation = slerpRotation;

                        yield return null;
                    }
                }
            }
            else
            {
                Vector3 direction = (head.position - GameObject.FindGameObjectWithTag("Player").GetComponent<Sc_Player>().player_Animator.transform.position).normalized;
                Quaternion lookatRotation = Quaternion.LookRotation(direction);
                Quaternion slerpRotation = Quaternion.Slerp(head.rotation, lookatRotation, 1f * Time.deltaTime);
                head.rotation = slerpRotation;


                yield return null;
            }
        }

        yield return null;
    }

    // When the character conversation is activated, send this conversationSO to the DM and allow it to sort through the conversation
    public void OnActivate()
    {
        // We don't activate this trigger if we've reached the end of the conversation
        if (!(!repeat_LastLine && hasReachedLastLine))
        {
            // Activate look at player
            lookAtPlayer = true;

            DM.Instance.active_NPCManager = this;
            if (conversationSO != null)
            {
                DM.Instance.activeConversation = conversationSO;
            }
            else
            {
                DM.Instance.bark = bark;
            }
            DM.Instance.activeAnim = anim;
            if (focusPoint != null)
            {
                DM.Instance.activeFocus = focusPoint;
            }
            else
            {
                DM.Instance.activeFocus = this.gameObject.transform;
            }
            DM.Instance.OnActivate();
        }
    }

    public void OnDeactivate()
    {
        lookAtPlayer = false;
    }

    float f_Sigmoid(float t)
    {
        float newT = (t * 2f - 1f) * 6f;

        float val = 1f/(1 + (Mathf.Exp(-newT)));

        return val;
    }
}