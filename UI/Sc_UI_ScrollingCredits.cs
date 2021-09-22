using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_UI_ScrollingCredits : MonoBehaviour
{
    public float scrollSpeed;
    private float inputScrollSpeed;
    public float scrollTop; // When the scrolling should end
    public float scrollBottom; // When the scrolling should start
    public RectTransform textToScroll;

    private float anchorYMin;

    void Start()
    {
        anchorYMin = scrollBottom;
    }

    // Update is called once per frame
    void Update()
    {
        // Change the Top value of the rect transform once per frame
        // When it hits the limit, reset it
        anchorYMin = anchorYMin + (Time.deltaTime * scrollSpeed) + (Time.deltaTime * GM.Instance.sI.cameraPan.y);
        textToScroll.anchorMin = new Vector2(textToScroll.anchorMin.x, anchorYMin);
        textToScroll.anchorMax = new Vector2(textToScroll.anchorMax.x, anchorYMin + 1);
        textToScroll.offsetMax = new Vector2(textToScroll.offsetMax.x, 0);

        // If we reach the scroll top, reset the anchor positions
        if (anchorYMin > scrollTop)
        {
            anchorYMin = scrollBottom;
        }
        // If we reach the stoll bottm, reset to the top
        if (anchorYMin < scrollBottom)
        {
            anchorYMin = scrollTop;
        }
    }
}
