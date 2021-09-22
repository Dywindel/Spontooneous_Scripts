using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

// Editor scripting for the Rift Object

public class RiftObj_Editor : MonoBehaviour
{
    // These are Unity Editor specific scripts
    #if UNITY_EDITOR

    // Store the gridheight for placing gridlines
    int gridLy = 0;

    ///////////////
    // UNIQUE ID //
    ///////////////

    // I need a numver that increments independent of Unity
    // I will store a file with that number in (Which is a little annoying, but hey)

    public int Generate_UniqueID()
    {
        // Load the unique ID
        UniqueID uniqueID = Persistance.Load_UniqueID();
        // Increment it
        uniqueID.Increment();
        // Store that value to be returned
        int val = uniqueID.uniqueIDTotal;
        // Save it
        Persistance.Save_UniqueID(uniqueID);

        return val;
    }

    /////////////
    // VISUALS //
    /////////////

    public void Update_Grid()
    {
        RiftObj riftObj = this.GetComponent<RiftObj>();

        // For the area collier grid
        // Change the box collider trigger size
        riftObj.areaCollider.center = Calculate_Centre(riftObj);
        Undo.RecordObject(riftObj, "Update Area Collider Size");
        // Add an extra 1 unit border for the collider so that the player has some breathing room when solving the puzzle
        riftObj.areaCollider.size = new Vector3(riftObj.gsx + 2, riftObj.gsly * RL_V.vertUnit, riftObj.gsy + 2);


        // For the gridlines
        // First, we check if the height of the gridmap has changed
        if (gridLy != riftObj.gsly)
        {
            // We delete and empty the current arrayOf_Gridlines
            for (int ly = 0; ly < riftObj.arrayOf_GridLines.Length; ly++)
            {
                // Safely destroy them
                RL.SafeDestroy<GameObject>(riftObj.arrayOf_GridLines[ly]);
            }

            // Empty any references to those objects
            riftObj.arrayOf_GridLines = new GameObject[riftObj.gsly];
            riftObj.arrayOf_GridLines_MR = new MeshRenderer[riftObj.gsly];

            // Then, we need to instantiate new gridlines into the scene
            for (int ly = 0; ly < riftObj.arrayOf_GridLines.Length; ly++)
            {
                Undo.RecordObject(riftObj, "Generate Grid");
                riftObj.arrayOf_GridLines[ly] = Instantiate(riftObj.obj_GridLines, riftObj.transform);
                Undo.RecordObject(riftObj, "Generate Grid Mesh");
                riftObj.arrayOf_GridLines_MR[ly] = riftObj.arrayOf_GridLines[ly].GetComponent<MeshRenderer>();
            }
        }

        // Now we can go back to position each element of the grid

        // For each layer
        for (int ly = 0; ly < riftObj.arrayOf_GridLines.Length; ly++)
        {
            // Get a temp reference
            Transform temp_GridPlane = riftObj.arrayOf_GridLines[ly].transform;

            // Change the grid size
            temp_GridPlane.localPosition = Calculate_Centre(riftObj);
            // Add height based on ly value
            temp_GridPlane.localPosition = new Vector3(temp_GridPlane.localPosition.x, -0.49f + ly, temp_GridPlane.localPosition.z);
            temp_GridPlane.localScale = new Vector3(riftObj.gsx, riftObj.gsly * RL_V.vertUnit, riftObj.gsy);
        }
    }

    public Vector3 Calculate_Centre(RiftObj pRiftObj)
    {
        Vector3 returnVec3 = new Vector3((float)pRiftObj.gsx/2f - 0.5f, (float)pRiftObj.gsly/2f - 0.5f, (float)pRiftObj.gsy/2f - 0.5f);

        return returnVec3;
    }

    // Rotates the Rift and all it's elements clockwise by 90 degrees
    // Doesn't actually rotate anything, just repositions everything as if it were rotated
    // Will be really useful when placing puzzles in the world later on
    public void Rotate_Rift_Clockwise()
    {
        RiftObj riftObj = this.GetComponent<RiftObj>();

        // Flip the gsx and gsy
        int temp_gsy = riftObj.gsy;
        riftObj.gsy = riftObj.gsx;
        riftObj.gsx = temp_gsy;

        // Take all the RuneObjs, RockObjs and the Start and End Points
        List<Transform> listOf_TransformsObjs = new List<Transform>();
        RuneObj[] arrayOf_RuneObjs = GetComponentsInChildren<RuneObj>();
        RockObj[] arrayOf_RockObjs = GetComponentsInChildren<RockObj>();
        foreach (RuneObj runeObj in arrayOf_RuneObjs)
        {
            listOf_TransformsObjs.Add(runeObj.transform);
            // Rotate the dirType of each rune (Just the runeObj, runeData hasn't been created yet as we're in edit mode)
            runeObj.dirType = RL_F.R_DT(runeObj.dirType);
        }
        foreach (RockObj rockObj in arrayOf_RockObjs)
        {
            listOf_TransformsObjs.Add(rockObj.transform);
        }
        listOf_TransformsObjs.Add(riftObj.startPoint);
        listOf_TransformsObjs.Add(riftObj.endPoint);
        // And place them in a new position based on some maths

        foreach (Transform trans in listOf_TransformsObjs)
        {
            // For the case where we literally rotate around the Rift's origin (At 0, 0)
            // First, take the objects coord and move them to the 0, 0 point
            int[] runeObj_Coord = RL_F.V3_IA(trans.localPosition);

            // Second, from 0, 0, add the y value to the x position, take away the x value from the y position
            // Z position stays the same
            int[] runeObj_NewCoord = new int[3] {runeObj_Coord[0], runeObj_Coord[2], -runeObj_Coord[1]};

            // Finally, as our origin point will always stay at 0,0, we shift the entire grid up by it's rotate Y length, gsy
            // Take away a value of gsy from their positions
            runeObj_NewCoord = new int[3] {runeObj_NewCoord[0], runeObj_NewCoord[1], runeObj_NewCoord[2] + (riftObj.gsy - 1)};

            trans.localPosition = RL_F.IA_V3(runeObj_NewCoord);
        }
    }

    /////////////////
    // PERSISTANCE //
    /////////////////

    // Performs the same save commands, but generates a new ID, regardless
    public void Save_New_Rift_Layout()
    {
        // Get a reference to the attached RiftObj
        RiftObj riftObj = this.GetComponent<RiftObj>();

        // We perform the undo command to save this as it's actual value
        Undo.RecordObject(riftObj, "Update Unique ID");
        riftObj.uniqueID = Generate_UniqueID();

        Save_Rift_Layout();
    }

    public void Save_Rift_Layout()
    {
        // Get a reference to the attached RiftObj
        RiftObj riftObj = this.GetComponent<RiftObj>();

        // Check rift's unique ID
        if (riftObj.uniqueID == -1)
        {
            // We perform the undo command to save this as it's actual value
            Undo.RecordObject(riftObj, "Update Unique ID");
            riftObj.uniqueID = Generate_UniqueID();
        }

        // Collect the data
        Rift_LevelData rift_LevelData = Pack_RiftData();

        // Save an image of this Rift
        RiftThumbnail riftThumbnail = GetComponent<RiftThumbnail>();
        riftThumbnail.TakeSnapshot();
        
        // Create and populate the riftSO asset
        RiftSO riftSO = ScriptableObject.CreateInstance<RiftSO>();
        riftSO.name = "RiftSO_" + riftObj.uniqueID;
        riftSO.New_RiftSO(rift_LevelData);

        // Save the RiftSO asset
        string fileName = "Assets/Levels/Levels_SO/" + riftSO.name + ".asset";
        AssetDatabase.CreateAsset(riftSO, fileName);

        // Refresh the asset database
        AssetDatabase.Refresh();
    }

    public void Load_Rift_Layout()
    {
        Rift_LevelData rift_LeveData = Persistance.Load_Rift<Rift_LevelData>("/RiftData/");

        // Check to ensure it's not null
        if (rift_LeveData != null)
        {
            // First, clear the entire level
            Clear_Level();

            // Unpack the information stored inside the rift_LevelData
            // Set the relevent graphical features
            Unpack_And_Set_RiftData(rift_LeveData);

            // Finally, setup the data inside the RiftObj. We can use the editorWindow script, which has this function
            RL_EF.Setup_RiftData();
        }
    }

    public void Load_RiftSO_Layout(string puzNum)
    {
        string filePath = "Assets/Levels/Levels_SO/RiftSO_" + puzNum + ".asset";

        RiftSO riftSO = (RiftSO)AssetDatabase.LoadAssetAtPath(filePath, typeof(RiftSO));

        Rift_LevelData rift_LeveData = riftSO.rift_LevelData;

        // Check to ensure it's not null
        if (rift_LeveData != null)
        {            
            // First, clear the entire level
            Clear_Level();

            // Unpack the information stored inside the rift_LevelData
            // Set the relevent graphical features
            Unpack_And_Set_RiftData(rift_LeveData);

            // Finally, setup the data inside the RiftObj. We can use the editorWindow script, which has this function
            RL_EF.Setup_RiftData();
        }
    }

    public Rift_LevelData Pack_RiftData()
    {
        // Get a reference to the attached RiftObj
        RiftObj riftObj = this.GetComponent<RiftObj>();

        // RiftData
        RiftData riftData = new RiftData(riftObj.uniqueID, riftObj.riftState, riftObj.boardType, new int[3] {riftObj.gsly, riftObj.gsx, riftObj.gsy},
                            new int[3]{riftObj.bridgePieces_Max, riftObj.plankPieces_Max, riftObj.minePieces_Max},
                            riftObj.resetPoint.localPosition, riftObj.startPoint.localPosition, riftObj.endPoint.localPosition,
                            riftObj.riftName,
                            riftObj.exitDir,
                            riftObj.comment);

        // RuneData
        RuneObj[] arrayOf_RuneObj = gameObject.GetComponentsInChildren<RuneObj>();
        RuneData_AsSer[] arrayOf_RuneData_AsSer = new RuneData_AsSer[arrayOf_RuneObj.Length];
        for (int i = 0; i < arrayOf_RuneObj.Length; i++)
        {
            // Create the rune data using it's start function
            arrayOf_RuneObj[i].PassStart(riftObj);
            // Grab the RuneData and store it
            arrayOf_RuneData_AsSer[i] = new RuneData_AsSer(arrayOf_RuneObj[i]);
        }

        // RockData
        RockObj[] arrayOf_RockObj = gameObject.GetComponentsInChildren<RockObj>();
        RockData_AsSer[] arrayOf_RockData_AsSer = new RockData_AsSer[arrayOf_RockObj.Length];
        // We can collect the position using an equation, no need to pass variables to the script
        for (int i = 0; i < arrayOf_RockObj.Length; i++)
        {
            arrayOf_RockData_AsSer[i] = new RockData_AsSer(RL_F.V3DFV3_IA(arrayOf_RockObj[i].transform.position, riftObj.transform.position), arrayOf_RockObj[i]);
            int[] temp = RL_F.V3DFV3_IA(arrayOf_RockObj[i].transform.position, riftObj.transform.position);
        }
        
        // Finally, store this data as a single class
        return new Rift_LevelData(riftData, arrayOf_RuneData_AsSer, arrayOf_RockData_AsSer);
    }

    public void Unpack_And_Set_RiftData(Rift_LevelData rift_LevelData)
    {
        // Get a reference to the RL_M
        RL_M rl_m = GameObject.FindObjectOfType<GM>().GetComponent<RL_M>();
        // Get a reference to the attached RiftObj
        RiftObj riftObj = this.GetComponent<RiftObj>();

        // RiftData
        RiftData riftData = rift_LevelData.riftData;
        // RuneData
        RuneData_AsSer[] arrayOf_RuneData_AsSer = rift_LevelData.arrayOf_RuneData;
        // RockData
        RockData_AsSer[] arrayOf_RockData = rift_LevelData.arrayOf_RockData;

        // Set rift values
        Undo.RecordObject(riftObj, "Load riftSO layout");
        riftObj.uniqueID = riftData.uniqueID;
        riftObj.riftName = riftData.riftName;
        riftObj.riftState = riftData.riftState;
        riftObj.boardType = riftData.boardType;
        riftObj.exitDir = riftData.exitDir;
        riftObj.gsly = riftData.gs[0];
        riftObj.gsx = riftData.gs[1];
        riftObj.gsy = riftData.gs[2];
        riftObj.bridgePieces_Max = riftData.pieces[0];
        riftObj.bridgePieces_Cur = riftData.pieces[0];
        riftObj.plankPieces_Max = riftData.pieces[1];
        riftObj.plankPieces_Cur = riftData.pieces[1];
        riftObj.minePieces_Max = riftData.pieces[2];
        riftObj.minePieces_Cur = riftData.pieces[2];
        try
        {
            riftObj.resetPoint.localPosition = RL_F.IA_V3(riftData.resetPos);
        }
        catch (Exception e)
        {
            print(": RiftObj class not uptodate: " + e);
            riftObj.resetPoint.localPosition = Vector3.zero;
        }
        riftObj.startPoint.localPosition = RL_F.IA_V3(riftData.startPos);
        riftObj.endPoint.localPosition = RL_F.IA_V3(riftData.endPos);
        riftObj.comment = riftData.comment;

        // Update the rift's grid appearance
        Update_Grid();

        // Set Runes
        for (int i = 0; i < arrayOf_RuneData_AsSer.Length; i++)
        {
            RuneData_AsSer newRuneData_AsSer = arrayOf_RuneData_AsSer[i];
            // Calculate the Rune's position vector
            Vector3 vecPos = RL_F.IADFV3_V3(newRuneData_AsSer.pos, riftObj.transform.position);
            // Create the RuneObj item
            GameObject newRuneObj = Instantiate(rl_m.runeObj, vecPos, Quaternion.Euler(0f, 0f, 0f));
            // Set the parent
            newRuneObj.transform.parent = transform;

            // Update the RuneObj's type settings
            newRuneObj.GetComponent<RuneObj_Editor>().Unpack_RuneSettings(newRuneData_AsSer);
            // Update the mesh and material visuals
            newRuneObj.GetComponent<RuneObj_Editor>().Update_Rune();
        }

        // Set Rocks
        for (int i = 0; i < arrayOf_RockData.Length; i++)
        {
            RockData_AsSer newRockData = arrayOf_RockData[i];
            // Calculate the Rock's position vector
            Vector3 vecPos = RL_F.IADFV3_V3(arrayOf_RockData[i].pos_RS, riftObj.transform.position);
            // Create the RockObj item
            GameObject newRockObj = Instantiate(rl_m.rockObj, vecPos, Quaternion.identity);
            // Set the parent
            newRockObj.transform.parent = transform;

            // Update the RockObj's collider
            newRockObj.GetComponent<BoxCollider>().size = new Vector3(newRockData.size[0], newRockData.size[1], newRockData.size[2]);
        }
    }

    // First, we empty the level of it's relevent game objects
    public void Clear_Level()
    {
        // Collect all Runes and Rocks
        RuneObj[] arrayOf_RuneObj = gameObject.GetComponentsInChildren<RuneObj>();
        RockObj[] arrayOf_RockObj = gameObject.GetComponentsInChildren<RockObj>();
        
        GameObject tempGO;

        // Get a reference to the attached RiftObj
        RiftObj riftObj = this.GetComponent<RiftObj>();
        // Delete the gridlines
        for (int ly = 0; ly < riftObj.arrayOf_GridLines.Length; ly++)
        {
            RL.SafeDestroy(riftObj.arrayOf_GridLines[ly]);
            riftObj.arrayOf_GridLines[ly] = null;
        }

        // Delete all of it
        for (int i = 0; i < arrayOf_RuneObj.Length; i++)
        {
            tempGO = arrayOf_RuneObj[i].gameObject;
            tempGO = RL.SafeDestroy(tempGO);
        }
        for (int i = 0; i < arrayOf_RockObj.Length; i++)
        {
            tempGO = arrayOf_RockObj[i].gameObject;
            tempGO = RL.SafeDestroy(tempGO);
        }

        // For safety, delete all references to game object lists
        arrayOf_RuneObj = new RuneObj[0];
        arrayOf_RockObj = new RockObj[0];
    }

    // Delete from the database the RiftSO and PNG of this current puzzleID (If it exists)
    public void Delete_From_Database()
    {
        // Grab this puzzleID


        //AssetDatabase.DeleteAsset()
    }

    #endif
}

