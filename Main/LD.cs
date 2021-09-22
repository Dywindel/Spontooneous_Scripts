using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The lighting controller for the game
public class LD : MonoBehaviour
{
    #region Singleton

    public static LD Instance {get; private set; }

    private void Awake()
    {
        Instance = this;
    }
        
    #endregion

    // World objects
    public Light lightA;
    public Light lightB;

    // List of areaLightingSO that will be added to as the player triggers them
    List<AreaSettingsSO> listOf_LightingChanges = new List<AreaSettingsSO>();
    bool isLightingChanging = false;

    // When requesting a change in lighting, we check if a lighting change is already occurring
    // Either we start the change coroutine (And add lighting change to the list), or we just add the lighting change to the list
    public void Activate_AreaLighting(AreaSettingsSO areaLightingSO)
    {
        // Add this to the list if the list if of size 2 or smaller
        if (listOf_LightingChanges.Count <= 2)
        {
            listOf_LightingChanges.Add(areaLightingSO);
        }
        else
        {
            // Else, remove the last item from the list and add this one
            listOf_LightingChanges.Remove(listOf_LightingChanges.Last());
            listOf_LightingChanges.Add(areaLightingSO);
        }
        
        // Run the lighting coroutine if it's not running
        if (!isLightingChanging)
        {
            isLightingChanging = true;
            StartCoroutine(Update_AreaLighting());
        }
    }

    // This loop allows items to add themselves to the lighting change animation
    // Such that nothing is missed
    IEnumerator Update_AreaLighting()
    {
        AreaSettingsSO currentSetting = null;
        while(listOf_LightingChanges.Count > 0)
        {
            // Get the item from the list
            AreaSettingsSO areaLightingSO = listOf_LightingChanges.First();
            
            // Make sure we're not changing it to its current setting
            if (currentSetting != areaLightingSO)
            {
                // Animate the lighting change
                // Wait until the animation has finished
                yield return Animate_AllChanges(areaLightingSO);
            }
            currentSetting = areaLightingSO;
            
            // Remove the first element
            listOf_LightingChanges.Remove(listOf_LightingChanges.First());
        }

        // Once all the lights have finished changing, reset the boolean
        isLightingChanging = false;
        yield return null;
    }

    // All animations that change settings
    IEnumerator Animate_AllChanges(AreaSettingsSO areaSettingsSO)
    {
        Coroutine cr_Lighting = StartCoroutine(Animate_ChangeLighting(areaSettingsSO));
        Coroutine cr_Fog = StartCoroutine(Animate_ChangeFog(areaSettingsSO));

        yield return cr_Lighting;
        yield return cr_Fog;

        yield return null;
    }

    // The animation for update the lighting
    IEnumerator Animate_ChangeLighting(AreaSettingsSO areaSettingsSO)
    {
        Color temp_LightA = lightA.color;
        Color temp_LightB = lightB.color;
        float temp_IntLightA = lightA.intensity;
        float temp_IntLightB = lightB.intensity;

        // This is the main loop that will change all the elements
        for (float t = 0f; t < 1f; t += Time.deltaTime * 0.2f)
        {
            // Colour A
            Color temp_ColourA = Color.Lerp(temp_LightA, areaSettingsSO.colourA, f_Sigmoid(t));
            float tempIntA = Mathf.Lerp(temp_IntLightA, areaSettingsSO.lightIntensity, f_Sigmoid(t));
            lightA.color = temp_ColourA;
            lightA.intensity = tempIntA;

            // Colour B
            Color temp_ColourB = Color.Lerp(temp_LightB, areaSettingsSO.colourB, f_Sigmoid(t));
            lightB.color = temp_ColourB;
            lightB.intensity = tempIntA;

            yield return null;
        }

        // Finally, set everything manually
        lightA.color = areaSettingsSO.colourA;
        lightB.color = areaSettingsSO.colourB;

        yield return null;
    }

    // Animate the fog changes
    IEnumerator Animate_ChangeFog(AreaSettingsSO areaSettingsSO)
    {
        Color temp_FogColour = RenderSettings.fogColor;
        Vector2 temp_FogLinear = new Vector2 (RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);

        // This is the main loop that will change all the elements
        for (float t = 0f; t < 1f; t += Time.deltaTime * 0.2f)
        {
            Color temp_ColourA = Color.Lerp(temp_FogColour, areaSettingsSO.fogColour, f_Sigmoid(t));
            RenderSettings.fogColor = temp_ColourA;

            RenderSettings.fogStartDistance = Mathf.Lerp(temp_FogLinear.x, areaSettingsSO.fogLinear.x, f_Sigmoid(t));
            RenderSettings.fogEndDistance = Mathf.Lerp(temp_FogLinear.y, areaSettingsSO.fogLinear.y, f_Sigmoid(t));

            yield return null;
        }

        // Manually set
        RenderSettings.fogColor = areaSettingsSO.fogColour;
        RenderSettings.fogStartDistance = areaSettingsSO.fogLinear.x;
        RenderSettings.fogEndDistance = areaSettingsSO.fogLinear.y;

        yield return null;
    }

    // Functions
    float f_Sigmoid(float t)
    {
        float newT = (t * 2f - 1f) * 7f;

        float val = 1f/(1 + (Mathf.Exp(-newT)));

        return val;
    }

    float f_Linear(float t)
    {
        return t;
    }
}
