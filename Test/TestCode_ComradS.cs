using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class TestCode_ComradS : MonoBehaviour
{
    private void LoadAndSetUpNewScene(string scenePath_)
    {
        //If there is a scene open, close that first
        if (mapManager.mapMasterOne != null)
        {
            EditorSceneManager.CloseScene(mapManager.mapMasterOne.gameObject.scene, true);
        }
        else
        {
            Debug.Log("No mapMasterOne found to close");
        }

        EditorSceneManager.OpenScene(scenePath_, OpenSceneMode.Additive);

        //Re-initializee the mapManager, which will find the new MapMaster and initialize that, then initialize all the objects
        mapManager.Initialize();
        for (int i = 0; i < mapManager.mapMasterOne.ObjectList.Count; i++)
        {
            mapManager.mapMasterOne.ObjectList[i].Initialize();
        }
        mapManager.mapMasterOne.Setup();

        fallingObjectList = FindObjectsOfType<FeelsGravity>().ToList();
        hiddenObjectsList.Clear();
        for (int i = 0; i < mapManager.mapMasterOne.ObjectList.Count; i++)
        {
            if (!mapManager.mapMasterOne.ObjectList[i].active)
            {
                hiddenObjectsList.Add(mapManager.mapMasterOne.ObjectList[i]);
            }
        }

        //Find the new mapMaster to parent new objects
        objectHolder = GameObject.FindWithTag("ObjectHolder").transform;
    }

    //Adapted from: https://kpprt.de/code-snippet/in-editor-screenshot-script-for-unity/
    public void TakeScreenshot()
    {
        var camera = GetCamera();
        camera.aspect = 1;
        //Image size if usefull the multipliers can be extracted later
        int width = GetImageSize();
        int height = GetImageSize();
        bool ensureTransparentBackground = true; //Sets a transparent background, setup like this for easy toggling / extraction if ever needed

        PrepareForScreenshot();

        RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        camera.targetTexture = rt;
        CameraClearFlags clearFlags = camera.clearFlags;
        Color backgroundColor = camera.backgroundColor;

        if (ensureTransparentBackground)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(); //alpha is zero
        }

        camera.Render();

        if (ensureTransparentBackground)
        {
            camera.clearFlags = clearFlags;
            camera.backgroundColor = backgroundColor;
        }

        RenderTexture currentRT = RenderTexture.active; //Save the currently active Render Texture so we can override it.
        RenderTexture.active = camera.targetTexture;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);

        //PNGs should be sRGB so convert to sRGB color space when rendering in linear.
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            Color[] pixels = screenshot.GetPixels();
            for (int p = 0; p < pixels.Length; p++)
            {
                pixels[p] = pixels[p].gamma;
            }
            screenshot.SetPixels(pixels);
        }

        screenshot.Apply(false);

        string filenameAndPath = ScreenshotNameFromMapMasterOne();
        Directory.CreateDirectory(Path.GetDirectoryName(filenameAndPath)); //Create the directory if it doesn't exist

        byte[] png = screenshot.EncodeToPNG();
        File.WriteAllBytes(filenameAndPath, png);

        camera.targetTexture = null; //Remove the reference to the Target Texture so our Render Texture is garbage collected.
        RenderTexture.active = currentRT;

        RevertScreenshotPreperations();

        Debug.Log(string.Format("Took screenshot to: {0}", filenameAndPath));
    }
}*/
