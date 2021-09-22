using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

// Coda donated by EricH2000
// Attache this to a camera component and, on Start, it will create a PNG
public class RiftThumbnail : MonoBehaviour
{
    #if UNITY_EDITOR

    // Local references
    GameObject cam_GO;
    Camera cam;
    GameObject plane_GO;
    MeshFilter plane_MF;
    MeshRenderer plane_MR;
    float resoultionUnits = 1f;

    public void TakeSnapshot()
    {
        // Calculate the resolution size of the image (Large puzzles need a bigger resolution)
        RiftObj riftObj = GetComponent<RiftObj>();
        if (riftObj.gsx > riftObj.gsy)
        {
            resoultionUnits = riftObj.gsx;
        }
        else
        {
            resoultionUnits = riftObj.gsy;
        }
        resoultionUnits = Mathf.Clamp(resoultionUnits, 5f, 30f);

        // Create a camera component that is orthoganol, facing downwards and of the right size to see the game grid
        CreateCamera();

        // Create the draft backing colour
        CreateDraftBacking();

        // Take a snapshot of the level and save an image of it as the level's name
        SavePng();

        // Finally, delete the camera and riftState components and wipe any memory of using it
        RL.SafeDestroy<GameObject>(cam_GO);
        RL.SafeDestroy<GameObject>(plane_GO);
        cam_GO = null;
        cam = null;
        plane_GO = null;
        plane_MF = null;
        plane_MR = null;
    }

    // Create and place the camera
    void CreateCamera()
    {
        // Create a new empty gameObject
        cam_GO = new GameObject("SnapshowCamera");
        cam_GO.transform.parent = transform;
        // Add a camera component
        cam = cam_GO.AddComponent(typeof(Camera)) as Camera;
        // Set it to orthographic
        cam.orthographic = true;

        // Grab the riftObj and riftObj_Editor
        RiftObj riftObj = GetComponent<RiftObj>();
        RiftObj_Editor riftObj_Editor = GetComponent<RiftObj_Editor>();

        // Find its centre point
        Vector3 centre = riftObj_Editor.Calculate_Centre(riftObj);
        // Make sure we move the camera up be the number of layers it has
        centre = centre + (Vector3.up*(riftObj.gsly + 1f));
        // Place the camera there (localPosition)
        cam_GO.transform.localPosition = centre;

        // Set the orthographic size roughly based on the largest grid size component
        // A multiplier of 0.54f works well for this conversion
        float orthoSize = resoultionUnits * 0.54f;
        // Limit the camera max and min to 5 and 20
        orthoSize = Mathf.Clamp(orthoSize, 5f, 20f);
        cam.orthographicSize = orthoSize;
        // Rotate the camera to face downwards
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // Create Drafting state colour backing
    void CreateDraftBacking()
    {
        // Basically, this creates a plane the size of the puzzle of a specific colour based on the puzzle's drafting state.
        // This way, it will appear as this colour in the database and let me know what puzzles need improving

        RiftObj riftObj = GetComponent<RiftObj>();
        RiftObj_Editor riftObj_Editor = GetComponent<RiftObj_Editor>();

        // Create a new empty GameObject
        plane_GO = new GameObject("Draft Colour Plane");
        plane_GO.transform.parent = transform;
        // Add a mesh filter and renderer
        plane_MF = plane_GO.AddComponent(typeof(MeshFilter)) as MeshFilter;
        plane_MR = plane_GO.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

        // Set the mesh filter
        Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Art/FBX/Bln_Mesh_Plane.fbx", typeof(Mesh));
        plane_MF.mesh = mesh;

        // Set the mesh colour
        Material material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/Editor/Mat_Thumbnail.mat", typeof(Material));
        
        if (riftObj.riftState == RiftState.Concept)
        {
            material.color = new Color(0f, 1f, 1f, 0f);
        }
        else if (riftObj.riftState == RiftState.Rough)
        {
            material.color = new Color(1f, 0.5f, 0f, 0f);
        }
        else if (riftObj.riftState == RiftState.Drafted)
        {
            material.color = new Color(1f, 1f, 0f, 0f);
        }
        else if (riftObj.riftState == RiftState.Done)
        {
            material.color = new Color(0f, 1f, 0f, 0f);
        }
        else if (riftObj.riftState == RiftState.Rejected)
        {
            material.color = new Color(1f, 0f, 0f, 0f);
        }

        plane_MR.material = material;

        // Place the material in the scene in the centre
        plane_GO.transform.localPosition = riftObj_Editor.Calculate_Centre(riftObj);
        plane_GO.transform.localScale = new Vector3(40f, 1f, 40f);
        // Move it below the active puzzle area
        plane_GO.transform.localPosition = plane_GO.transform.localPosition + (Vector3.down * riftObj.gsly);
    }

    static string _GetThumbnailFilepath(int uniqueID)
    {
        string filename = "RiftPNG_" + uniqueID + ".png";
        return Path.Combine(Application.dataPath + "\\Levels\\Levels_PNG", filename); // Note that loading is through addressable resource, saving is filesystem.
    }

    public void SavePng()
    {
        Camera thumbnailCamera = cam;

        // Render current thumbnail camera view to a texture.
        int RENDER_SIZE = 128 * (int)resoultionUnits;
        float RENDER_ASPECT = 1f;
        thumbnailCamera.aspect = RENDER_ASPECT;
        RenderTexture renderTexture = new RenderTexture(RENDER_SIZE, RENDER_SIZE, 24 );
        thumbnailCamera.targetTexture = renderTexture;
        thumbnailCamera.Render();
    
        // Read bytes from render texture into a thumbnail texture.
        RenderTexture.active = renderTexture;
        Texture2D thumbnail =
            new Texture2D(RENDER_SIZE,RENDER_SIZE, TextureFormat.RGB24, false); // false, meaning no need for mipmaps
        thumbnail.ReadPixels( new Rect(0, 0, RENDER_SIZE,RENDER_SIZE), 0, 0);
        RenderTexture.active = null; //can help avoid errors 
        thumbnailCamera.targetTexture = null;

        // Save as PNG.
        byte[] bytes;
        bytes = thumbnail.EncodeToPNG();
        // Grab the riftObj unique ID, when saving an image of the puzzle
        RiftObj riftObj = GetComponent<RiftObj>();
        int uniqueID = riftObj.uniqueID;
        string thumbnailFilepath = _GetThumbnailFilepath(uniqueID);
        File.WriteAllBytes(thumbnailFilepath, bytes );
    }

    public static void Save()
    { 
        RiftThumbnail[] cams = Resources.FindObjectsOfTypeAll<RiftThumbnail>();
        if (cams.Length == 0) throw new Exception("Where is your thumbnail cam?");
        RiftThumbnail thumbnailCam = cams[0];
        thumbnailCam.gameObject.SetActive(true);
        thumbnailCam.SavePng();
        thumbnailCam.gameObject.SetActive(false);
    }

    #endif
}
