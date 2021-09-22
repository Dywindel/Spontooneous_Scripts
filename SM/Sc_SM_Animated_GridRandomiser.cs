using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This item randomises a list of icons onto a grid
// At some sort of speed
// Chance of icon change is a percentage

public class Sc_SM_Animated_GridRandomiser : MonoBehaviour
{
    public int gs;
    public float changeSpeed = 0.01f;
    public float changePer = 0.1f;
    public Sprite[] arrayOf_Sprites_Picker;
    SpriteRenderer[,] arrayOf_Sprite_Ref;

    // Start is called before the first frame update
    void Start()
    {
        object[] arrayOf_Objects = Resources.LoadAll("FakeRunes", typeof(Sprite));
        arrayOf_Sprites_Picker = new Sprite[arrayOf_Objects.Length];
        for (int i = 0; i < arrayOf_Sprites_Picker.Length; i++)
        {
            arrayOf_Sprites_Picker[i] = (Sprite)arrayOf_Objects[i];
        }

        // Initilise arrays
        arrayOf_Sprite_Ref = new SpriteRenderer[gs, gs];
        for (int i = 0; i < gs; i++)
        {
            for (int j = 0; j < gs; j++)
            {
                GameObject newGO = new GameObject("Sprite");
                // Parent
                newGO.transform.parent = transform;
                // Position
                newGO.transform.position = transform.position +  new Vector3(i, j, 0) - new Vector3(gs/2, gs/2f, 0f);
                newGO.AddComponent<SpriteRenderer>();
                arrayOf_Sprite_Ref[i, j] = newGO.GetComponent<SpriteRenderer>();

                // Initially randomise the sprites
                arrayOf_Sprite_Ref[i, j].sprite = arrayOf_Sprites_Picker[Random.Range(0, arrayOf_Sprites_Picker.Length)];
            }
        }

        // Start the changing animation
        StartCoroutine(UpdateIcons());

    }

    IEnumerator UpdateIcons()
    {
        while(true)
        {
            for (int i = 0; i < gs; i++)
            {
                for (int j = 0; j < gs; j++)
                {
                    // Change of randomness
                    if (Random.Range(0f, 1f) < changePer)
                    {
                        // Randomise every sprite each frame?
                        arrayOf_Sprite_Ref[i, j].sprite = arrayOf_Sprites_Picker[Random.Range(0, arrayOf_Sprites_Picker.Length)];
                        // And the rotation
                        arrayOf_Sprite_Ref[i, j].gameObject.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0, 4) * 90f);

                    }
                }
            }
            
            yield return new WaitForSeconds(changeSpeed);
        }
    }
}
