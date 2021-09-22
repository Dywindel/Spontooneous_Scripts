using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This script is dedicated to updating the mesh and material
// Appearance of the bridgeObj


public class BridgeObj_Mesh : MonoBehaviour
{
    // World references
    public RL_M rlm;
    [HideInInspector] public BridgeObj bridgeObj;
    public BridgeType bridgeType;
    public string bugCheck = "";

    // Reference to the main mesh object
    public MeshFilter bridge_ObjMesh;
    public MeshRenderer bridge_ObjMat;

    // Because I use OnSceneGUI to update my mesh, I want to make sure I'm always
    // Updating all the meshes around me before and after I move
    // Here, I record these meshes before and after movement, so I can update them
    private List<BridgeObj_Mesh> listOf_Ref_NeighbourMeshes = new List<BridgeObj_Mesh>();

    // This needs to be an Awake function or the bridgeObj will not be generated in time
    void Awake()
    {
        bridgeObj = this.GetComponent<BridgeObj>();
        rlm = GM.Instance.GetComponent<RL_M>();
    }

    // Test, running this as an update method
    void Update_SWITCHEDOFF()
    {
        UpdateMesh_Bridge_Perpare(0);
    }

    public void UpdateMesh(int chainIndex)
    {
        // Grab relevent information about this component
        bridgeObj = this.GetComponent<BridgeObj>();
        bridgeType = bridgeObj.bridgeType;
        PlankDir plankDir = bridgeObj.plankDir;

        // ERROR - THIS START FUNCTION ISN'T BEING CALLED BECAUSE THE CHAIN DOESN'T CHECK ABOVE
        //Start();

        // Because we rotate the centre mesh sometimes, we need to reset it's rotation here
        bridge_ObjMesh.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    
        // For planks, we do something different
        if (bridgeType == BridgeType.Plank)
        {
            // Here we only need one mesh to be used
            UpdateMesh_Plank(plankDir);
        }
        // For bridges and stairs, we basically do the same thing
        else
        {
            UpdateMesh_Bridge_Perpare(chainIndex);
            // For stairs, we include an extra step
            /*
            if (bridgeType == BridgeType.Stairs)
            {
                UpdateMesh_Stairs();
            }*/
        }
    }

    // This update alters the mesh regularly, without creating a chain of updates
    // Unless the passed boolean is true
    public void UpdateMesh_Bridge_Perpare(int chainIndex)
    {
        // What if I passed a number as well as a boolean? This way the chain
        // Of checking that occurs could happen X number of times (Maybe twice?)

        // Grab the parent rift
        RiftObj riftObj = bridgeObj.parent;

        // First, we reset the list of neighbours
        listOf_Ref_NeighbourMeshes = new List<BridgeObj_Mesh>();

        // We store the surrounding block value using a decimal conveted from binary
        // Where '1' represents a valid placement
        int[] mesh_Binary = new int[8];
        List<int> bin_AsList =  new List<int>();
        int bin_AsVal = 0;

        // Collect the bridge tiles around us by checking the array of bridge data inside our current rift
        int[] pos = RL_F.V3DFV3_IA(bridgeObj.transform.position, bridgeObj.parent.transform.position);
        
        // Go through nearby layers
        for (int k = pos[0] - 1; k <= pos[0] + 1; k++)
        {
            // Go through each row, x
            for (int i = pos[1] - 1; i <= pos[1] + 1; i ++)
            {
                // And each column, y
                for (int j = pos[2] - 1; j <= pos[2] + 1; j++)
                {
                    // Make sure we're inside trellis space grid
                    if (RL_F.C_IG(riftObj.trellis_gs, new int[3] {k + riftObj.trellisOffset[0], i + riftObj.trellisOffset[1], j + riftObj.trellisOffset[2]}))
                    {
                        // Ignore the centre square
                        if (!(k == pos[0] && i == pos[1] && j == pos[2]))
                        {
                            // If a Path exists here, we add it to the binary
                            if (riftObj.arrayOf_GridElement_TrellisSpace[k + riftObj.trellisOffset[0], i + riftObj.trellisOffset[1], j + riftObj.trellisOffset[2]].isPath)
                            {
                                // When concerning neighbours for our bridge mesh, we only care about our planar neighbours
                                if (k == pos[0])
                                {
                                    // Add to the int list
                                    bin_AsList.Add(RL_F.T_PS(new int[3] 
                                        {pos[0], i - pos[1] + 1, j - pos[2] + 1}));
                                    // Collect the total for the binary value
                                    bin_AsVal += (int)Mathf.Pow(2f, RL_F.T_PS(new int[3] {pos[0], i - pos[1] + 1, j - pos[2] + 1}));
                                }

                                // Store a reference to this neighbour, if this neighbour is a bridgeObj
                                // To do this, use a reverse lookup script for the trellis space setup
                                foreach (RiftObj tRiftObj in riftObj.listOf_AdjoiningRifts)
                                {
                                    // Adjust positioning by leaving one riftspace and entering another
                                    int[] posNS = new int[3]{k + riftObj.trellisOffset[0] - tRiftObj.trellisOffset[0], i + riftObj.trellisOffset[1] - tRiftObj.trellisOffset[1], j + + riftObj.trellisOffset[2] - tRiftObj.trellisOffset[2]};
                                    // Go through each rift, until you find the one that this position belongs to
                                    if (RL_F.C_IG(new int[3] {tRiftObj.gsly, tRiftObj.gsx, tRiftObj.gsy}, posNS))
                                    {
                                        // Does the bridgeData in that array have a type that is Bridge or Stairs?
                                        if (tRiftObj.arrayOf_BridgeData[posNS[0], posNS[1], posNS[2]].bridgeType == BridgeType.Bridge ||
                                                tRiftObj.arrayOf_BridgeData[posNS[0], posNS[1], posNS[2]].bridgeType == BridgeType.Stairs)
                                        {
                                            // Add that bridge mesh to our list to update
                                            listOf_Ref_NeighbourMeshes.Add(tRiftObj.arrayOf_BridgeData[posNS[0], posNS[1], posNS[2]].bridgeObj.GetComponent<BridgeObj_Mesh>());
                                        }
                                        // Once we've found what we need, we can break out of the this foreach loop
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (chainIndex > 0)
        {
            // First, I want to update the previous meshes I was next to
            foreach (BridgeObj_Mesh temp_Script in listOf_Ref_NeighbourMeshes)
            {
                // If they are exist
                if (temp_Script != null)
                    temp_Script.UpdateMesh(chainIndex - 1);
            }
        }

        // Put the int list in numerical order
        bin_AsList.Sort();

        int[] returnInt = RL_EF.Calculate_MeshIndex(bin_AsList, bin_AsVal);

        MeshDictionary(returnInt[0], returnInt[1]);

    }

    // For updating planks
    public void UpdateMesh_Plank(PlankDir plankDir)
    {
        // Set the centre mesh to a plank component based on it's type
        if (plankDir == PlankDir.Hor)
        {
            // Update mesh
            bridge_ObjMesh.mesh = rlm.bridge_PlankMeshes[0];
            // Set rotation
            bridge_ObjMesh.gameObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else if (plankDir == PlankDir.Ver)
        {
            // Update mesh
            bridge_ObjMesh.mesh = rlm.bridge_PlankMeshes[0];
            // Set rotation - No need for rotation as Ver is already in the right direction
        }
        else if (plankDir == PlankDir.Crs)
        {
            bridge_ObjMesh.mesh = rlm.bridge_PlankMeshes[1];
        }
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    void MeshDictionary(int b, int q = 0)
    {
        // Set the mesh based on the dictionary value
        // We have a slightly different mesh for stairs
        if (bridgeType == BridgeType.Bridge)
        {
            bridge_ObjMesh.mesh = rlm.bridge_BridgeMeshes[b];
        }
        else if (bridgeType == BridgeType.Stairs)
        {
            bridge_ObjMesh.mesh = rlm.bridge_StairsMeshes[b];
        }

        BridgeMaterial bridgeMaterial = bridgeObj.parent.bridgeMaterial;
        if (bridgeMaterial == BridgeMaterial.Wood && bridgeType == BridgeType.Bridge)
        {
            bridge_ObjMesh.mesh = rlm.bridge_BridgeMesh_Wooden[Random.Range(0, rlm.bridge_BridgeMesh_Wooden.Length)];
        }
        else if (bridgeMaterial == BridgeMaterial.City && bridgeType == BridgeType.Bridge)
        {
            bridge_ObjMesh.mesh = rlm.bridge_BridgeMesh_City[0];
        }

        // Rotate based on surrounding items
        if (bridgeType == BridgeType.Bridge)
        {
            bridge_ObjMesh.gameObject.transform.rotation = Quaternion.Euler(0f, 45f*q, 0f);
        }

        // For wooden bridge, rotate randomly
        if (bridgeMaterial == BridgeMaterial.Wood && bridgeType == BridgeType.Bridge)
        {
            bridge_ObjMesh.gameObject.transform.rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 4), 0f);
        }
    }

    public void MaterialDictionary(bool isElectric)
    {

        BridgeMaterial bridgeMaterial = bridgeObj.parent.bridgeMaterial;

        /*
        if (bridgeObj.parent != null)
            bridgeMaterial = bridgeObj.parent.bridgeMaterial;
        */

        // If Metal
        if (bridgeMaterial == BridgeMaterial.Metal)
        {
            // If electric
            if (isElectric)
                bridge_ObjMat.material = rlm.bridge_Materials[1];
            else
            {
                bridge_ObjMat.material = rlm.bridge_Materials[0];
            }
        }

        // If stone
        else if (bridgeMaterial == BridgeMaterial.Stone)
        {
            bridge_ObjMat.material = rlm.bridge_Materials[2];
        }

        // If Wood
        else if (bridgeMaterial == BridgeMaterial.Wood)
        {
            bridge_ObjMat.material = rlm.bridge_Materials[3];
        }

        // Block
        else if (bridgeMaterial == BridgeMaterial.City)
        {
            bridge_ObjMat.material = rlm.bridge_Materials[4];
        }
    }
}
