using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script generates and moves a water plane such that a 2x2 plane of water
// Always appears near where the player is.

public class Sc_WaterPlane : MonoBehaviour
{
    Transform player_Collider;

    public GameObject pre_WaterPlane;
    public GameObject pre_WaterPlane_Med;
    public GameObject pre_WaterPlane_Low;
    public GameObject pre_SandPlane;
    public GameObject pre_SandPlane_Med;
    public GameObject pre_SandPlane_Low;

    public int arrayLength = 2;     // Size of the gameObject array
    public float objectWidth = 50f;  // Size of gameObject

    private GameObject[] arrayOf_WaterPlanes;
    private Transform array_Of_WaterPlanes_Med_Group;
    private GameObject[] arrayOf_WaterPlanes_Med;
    private Transform waterplanes_Low_Group;

    
    void Start()
    {
        // Generate four water tiles for us
        Generate_WaterPlanes();
        // Generate eight medium detail tiles
        Generate_WaterPlane_Med();
        // Generate the low detail tiles
        Generate_WaterPlane_Low();
    
        // Grab the player
        player_Collider = Sc_Player.Instance.GetComponent<Sc_Player>().player_Collider.transform;
    }

    void Generate_WaterPlanes()
    {
        arrayOf_WaterPlanes = new GameObject[arrayLength*arrayLength];

        for (int i = 0; i < arrayLength; i++)
        {
            for (int j = 0; j < arrayLength; j++)
            {
                // Create the four water planes
                Vector3 newPos_Water = new Vector3(i*objectWidth, RL_V.waterLevel, j*objectWidth);

                arrayOf_WaterPlanes[i + j*arrayLength] = Instantiate(pre_WaterPlane, transform.position + newPos_Water, Quaternion.identity, transform) as GameObject;

                // Create the four sand planes
                Vector3 newPos_Sand = new Vector3(i*objectWidth, RL_V.SandLevel, j*objectWidth);

                // Set the parent transform to the water plane we just created
                GameObject temp_SandPlane = Instantiate(pre_SandPlane, transform.position + newPos_Sand, Quaternion.identity, arrayOf_WaterPlanes[i + j*arrayLength].transform) as GameObject;
            }
        }
    }

    // This water plane is medium detail and consists of one moving transform and 8 planes that surround the already place planes
    void Generate_WaterPlane_Med()
    {
        array_Of_WaterPlanes_Med_Group = new GameObject("WaterPlaneMed_Transform").transform;
        array_Of_WaterPlanes_Med_Group.parent = this.transform;

        arrayOf_WaterPlanes_Med = new GameObject[arrayLength*arrayLength*arrayLength*arrayLength];

        for (int i = 0; i < arrayLength*arrayLength; i++)
        {
            for (int j = 0; j < arrayLength*arrayLength; j++)
            {
                // We only want the outside edges
                if (j == 0 || i == 0 || j == (arrayLength*arrayLength - 1) || i == (arrayLength*arrayLength - 1))
                {
                    // Create the eight med water planes
                    // There's an offset for the medium detail planes
                    Vector3 newPos_Water = new Vector3(i*objectWidth, RL_V.waterLevel, j*objectWidth) - new Vector3(2*objectWidth, 0f, 2*objectWidth);

                    // Set their parent as the med_Group transform
                    arrayOf_WaterPlanes_Med[i + j*arrayLength] = Instantiate(pre_WaterPlane_Med, transform.position + newPos_Water, Quaternion.identity, array_Of_WaterPlanes_Med_Group) as GameObject;

                    // Create the eight sand planes
                    // There's an offset for the medium detail planes
                    Vector3 newPos_Sand = new Vector3(i*objectWidth, RL_V.SandLevel, j*objectWidth) - new Vector3(2*objectWidth, 0f, 2*objectWidth);

                    // Set the parent transform to the water plane we just created
                    GameObject temp_SandPlane = Instantiate(pre_SandPlane_Med, transform.position + newPos_Sand, Quaternion.identity, arrayOf_WaterPlanes_Med[i + j*arrayLength].transform) as GameObject;
                }
            }
        }
    }

    void Generate_WaterPlane_Low()
    {
        waterplanes_Low_Group = new GameObject("WaterPlanesLow_Transform").transform;
        waterplanes_Low_Group.parent = this.transform;

        Vector3 newPos_Water = waterplanes_Low_Group.transform.position + Vector3.up*RL_V.waterLevel - new Vector3(0.5f*objectWidth, 0f, 0.5f*objectWidth);

        // Create the water plane
        GameObject waterPlane_Low = Instantiate(pre_WaterPlane_Low, newPos_Water, Quaternion.identity, waterplanes_Low_Group.transform) as GameObject;

        Vector3 newPos_Sand = waterplanes_Low_Group.transform.position + Vector3.up*RL_V.SandLevel - new Vector3(0.5f*objectWidth, 0f, 0.5f*objectWidth);

        // Create the sand plane
        GameObject sandPlane_Low = Instantiate(pre_SandPlane_Low, newPos_Sand, Quaternion.identity, waterplanes_Low_Group.transform) as GameObject;
    }

    void Update()
    {
        // If a water plane is too far from the player, jump by double the size of the plane
        for (int i = 0; i < arrayLength; i++)
        {
            for (int j = 0; j < arrayLength; j++)
            {
                // For X direction
                float xPos = player_Collider.transform.position.x - arrayOf_WaterPlanes[i + j*arrayLength].transform.position.x;
                if (Mathf.Abs(xPos) > objectWidth)
                {
                    arrayOf_WaterPlanes[i + j*arrayLength].transform.position = new Vector3 (   arrayOf_WaterPlanes[i + j*arrayLength].transform.position.x + (2 * objectWidth * (xPos/Mathf.Abs(xPos))),
                                                                                                arrayOf_WaterPlanes[i + j*arrayLength].transform.position.y,
                                                                                                arrayOf_WaterPlanes[i + j*arrayLength].transform.position.z);
                }
                // For Z direction
                float zPos = player_Collider.transform.position.z - arrayOf_WaterPlanes[i + j*arrayLength].transform.position.z;
                if (Mathf.Abs(zPos) > objectWidth)
                {
                    arrayOf_WaterPlanes[i + j*arrayLength].transform.position = new Vector3 (   arrayOf_WaterPlanes[i + j*arrayLength].transform.position.x,
                                                                                                arrayOf_WaterPlanes[i + j*arrayLength].transform.position.y,
                                                                                                arrayOf_WaterPlanes[i + j*arrayLength].transform.position.z + (2 * objectWidth * (zPos/Mathf.Abs(zPos))));
                }
            }
        }

        // Update the group of med water planes
        // These can jump instantly because they are lower detail
        // For X direction
        float xPos_Med = player_Collider.transform.position.x - array_Of_WaterPlanes_Med_Group.position.x + 0.5f*objectWidth;
        if (Mathf.Abs(xPos_Med) > 0.5f*objectWidth)
        {
            array_Of_WaterPlanes_Med_Group.position = new Vector3 ( array_Of_WaterPlanes_Med_Group.position.x + (objectWidth * (xPos_Med/Mathf.Abs(xPos_Med))),
                                                                    array_Of_WaterPlanes_Med_Group.position.y,
                                                                    array_Of_WaterPlanes_Med_Group.position.z);
        }
        // For Z direction
        float zPos_Med = player_Collider.transform.position.z - array_Of_WaterPlanes_Med_Group.position.z + 0.5f*objectWidth;
        if (Mathf.Abs(zPos_Med) > 0.5f*objectWidth)
        {
            array_Of_WaterPlanes_Med_Group.position = new Vector3 ( array_Of_WaterPlanes_Med_Group.position.x,
                                                                    array_Of_WaterPlanes_Med_Group.position.y,
                                                                    array_Of_WaterPlanes_Med_Group.position.z + (objectWidth * (zPos_Med/Mathf.Abs(zPos_Med))));
        }

        // Update the group of low water planes
        float xPos_Low = player_Collider.transform.position.x - waterplanes_Low_Group.position.x + 0.5f*objectWidth;
        if (Mathf.Abs(xPos_Med) > 0.5f*objectWidth)
        {
            waterplanes_Low_Group.position = new Vector3 (  waterplanes_Low_Group.position.x + (objectWidth * (xPos_Low/Mathf.Abs(xPos_Low))),
                                                            waterplanes_Low_Group.position.y,
                                                            waterplanes_Low_Group.position.z);
        }
        // For Z direction
        float zPos_Low = player_Collider.transform.position.z - waterplanes_Low_Group.position.z + 0.5f*objectWidth;
        if (Mathf.Abs(zPos_Med) > 0.5f*objectWidth)
        {
            waterplanes_Low_Group.position = new Vector3 (  waterplanes_Low_Group.position.x,
                                                            waterplanes_Low_Group.position.y,
                                                            waterplanes_Low_Group.position.z + (objectWidth * (zPos_Low/Mathf.Abs(zPos_Low))));
        }
    }
}
