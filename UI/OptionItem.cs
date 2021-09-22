using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class OptionItem : MonoBehaviour
{
    Animator animator;
    public TextMeshProUGUI tMPro;

    public void Update_Animator(bool isHighlighted)
    {
        if (animator == null)
        {
            animator = this.GetComponent<Animator>();
        }
        animator.SetBool("isHighlighted", isHighlighted);
    }

    public virtual void Input_Action()
    {
        // For selecting the item

        // Animation
        if (!animator.GetBool("isSelected"))
        {
            StartCoroutine(Animate_IsSelected());
        }
    }

    public virtual void Input_Direction(bool isPositive)
    {
        // For changing the state of the item

        // Could add a jiggle animation
    }

    // Perform the isSelected animation only for a few seconds
    IEnumerator Animate_IsSelected()
    {
        animator.SetBool("isSelected", true);

        yield return new WaitForSeconds(1f);

        animator.SetBool("isSelected", false);

        yield return null;
    }
}
