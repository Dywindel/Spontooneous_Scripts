using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Used to update the mesh of a floor or cover object based on its neighbours

public class Sc_Obj_Mesh : MonoBehaviour
{
    // World references
    public RL_M rlm;

    // Reference to the main mesh object
    public MeshFilter objMesh;
    public MeshRenderer objMat;
    public bool isFloor = true;
    [Tooltip("This object will not merge with items that don't share the same parent with it")]
    public bool sameParent = false;
    public MeshType meshType;
    public enum MeshLength {Short, Med, Long};
    public MeshLength meshLength;
    public Material reference_Material;     // User can supply their own material instead of having to use an enum.
    public CoverType coverType;

    // Because I use OnSceneGUI to update my mesh, I want to make sure I'm always
    // Updating all the meshes around me before and after I move
    // Here, I record these meshes before and after movement, so I can update them
    private List<Sc_Obj_Mesh> listOf_Ref_NeighbourMeshes = new List<Sc_Obj_Mesh>();
    private List<Sc_Obj_Mesh> listOf_Pre_NeighbourMeshes = new List<Sc_Obj_Mesh>();

    void Start()
    {
        rlm = GameObject.FindGameObjectWithTag("GM").GetComponent<RL_M>();
        RL.DeclareFunction();
    }

    // Test, running this as an update method
    void Update_OFF()
    {
        UpdateMesh_Slab_Perpare(0);
    }

    public void UpdateMesh(int chainIndex)
    {
        Start();

        // Because we rotate the centre mesh sometimes, we need to reset it's rotation here
        objMesh.gameObject.transform.rotation = Quaternion.Euler(10f, 50f, 40f);

        int[] meshIndex = UpdateMesh_Slab_Perpare(chainIndex);

        MeshDictionary(meshIndex[0], meshIndex[1]);
    }

    // This update alters the mesh regularly, without creating a chain of updates
    // Unless the passed boolean is true
    public int[] UpdateMesh_Slab_Perpare(int chainIndex)
    {
        // What if I passed a number as well as a boolean? This way the chain
        // Of checking that occurs could happen X number of times (Maybe twice?)

        // First, update the meshes from the previous cycle, incase we're moving away from them
        if (chainIndex > 0)
        {
            // First, I want to update the previous meshes I was next to
            foreach (Sc_Obj_Mesh temp_Script in listOf_Ref_NeighbourMeshes)
            {
                // If they are exist
                if (temp_Script != null)
                    temp_Script.UpdateMesh(chainIndex - 1);
            }
        }

        // Now, we can reset the neighbours
        listOf_Ref_NeighbourMeshes = new List<Sc_Obj_Mesh>();

        // We store the surrounding block value using a decimal conveted from binary
        int bin_BlockNeighbours = 0;

        // We store the surrounding block value using a decimal conveted from binary
        // Where '1' represents a valid placement
        int[] mesh_Binary = new int[8];
        
        // First, we record what floor tiles are around it using raycasting
        // We check the four cardinal directions around the shape
        for (int n = 0; n < 4; n++)
        {
            // Then the the additonal corners
            for (int m = 0; m < 2; m++)
            {
                RaycastHit[] hitInfo;
                // In the regular case, we just draw a line from the transform to a neighbouring block
                if (m == 0)
                {
                    hitInfo = Physics.RaycastAll(transform.position, RL.dir_To_Vect[(DirType)(n + 1)], 1.0f);
                }
                // In the odd case, we have to stagger the movement and starting position of the ray
                else
                {
                    hitInfo = Physics.RaycastAll(transform.position + RL.dir_To_Vect[(DirType)(n + 1)],  RL.dir_To_Vect[(DirType)(((n + 1) % 4) + 1)] , 1.0f);
                }

                // This is my theory at least
                // Then, we check what we've found. If any of the objects we find have the tag ("Appropriate name")
                // Then we add a binary multiple to the overal value and go straight to the next number in the forloop

                // prepare the doOnce boolean
                bool checkOnce = false;

                // As long as an object is hit
                foreach (RaycastHit item in hitInfo)
                {
                    if (item.collider.gameObject.GetComponent<Sc_Obj_Mesh>() != null)
                    {
                        // For reference
                        Sc_Obj_Mesh item_Script = item.collider.gameObject.GetComponent<Sc_Obj_Mesh>();

                        if (isFloor)
                        {
                            if (item_Script.isFloor && item_Script.meshType == meshType)
                            {
                                // Check we haven't hit this mesh before and it's not us
                                if ((!listOf_Ref_NeighbourMeshes.Contains(item_Script) && (item_Script != this)))
                                {
                                    // If we have the same parent check on, check if both items have the same parent
                                    if (!sameParent  || item_Script.transform.parent == transform.parent)
                                    {
                                        // Then we add it to our hit list
                                        listOf_Ref_NeighbourMeshes.Add(item_Script);

                                        // If we are chaining mesh updates
                                        if (chainIndex > 0)
                                        {
                                            // Update that mesh too, but don't chain it
                                            item_Script.UpdateMesh(chainIndex - 1);                                
                                        }
                                        // This means a correspending block is here and we can check off the doOnce boolean
                                        checkOnce = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Cover types can merge together, it looks better
                            if (!item_Script.isFloor && item_Script.coverType == coverType)
                            {
                                // Check we haven't hit this mesh before and it's not us
                                if ((!listOf_Ref_NeighbourMeshes.Contains(item_Script) && (item_Script != this)))
                                {
                                    // If we have the same parent check on, check if both items have the same parent
                                    if (!sameParent  || item_Script.transform.parent == transform.parent)
                                    {
                                        // Then we add it to our hit list
                                        listOf_Ref_NeighbourMeshes.Add(item_Script);

                                        // If we are chaining mesh updates
                                        if (chainIndex > 0)
                                        {
                                            // Update that mesh too, but don't chain it
                                            item_Script.UpdateMesh(chainIndex - 1);                                
                                        }
                                        // This means a correspending block is here and we can check off the doOnce boolean
                                        checkOnce = true;
                                    }
                                }
                            }
                        }
                    }
                }

                // If we have found any blocks
                if (checkOnce)
                {
                    // We update the binary value
                    bin_BlockNeighbours += (int)(Mathf.Pow(4, n) * Mathf.Pow(2, m));
                }
            }
        }

        if (chainIndex > 0)
        {
            // First, I want to update the previous meshes I was next to
            foreach (Sc_Obj_Mesh temp_Script in listOf_Ref_NeighbourMeshes)
            {
                // If they are exist
                if (temp_Script != null)
                    temp_Script.UpdateMesh(chainIndex - 1);
            }
        }

        List<int> bin_AsList = RL_EF.BinaryConversion(bin_BlockNeighbours);

        return RL_EF.Calculate_MeshIndex(bin_AsList, bin_BlockNeighbours);
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    void MeshDictionary(int b, int q = 0)
    {
        // Rotate based on surrounding items
        objMesh.gameObject.transform.rotation = Quaternion.Euler(0f, 45f*q, 0f);

        // Updating mesh
        if (isFloor)
        {
            if (meshType == MeshType.Slab)
            {
                objMesh.mesh = rlm.slab_Meshes[b];
            }
            else if (meshType == MeshType.Canyon)
            {
                objMesh.mesh = rlm.canyon_Meshes[b];
            }
            else if (meshType == MeshType.Frames)
            {
                objMesh.mesh = rlm.floor_Frames[b];
            }
            else
            {
                if (meshLength == MeshLength.Short)
                {
                    objMesh.mesh = rlm.slabFlat_Meshes[b];
                    // Update the box collider
                    BoxCollider bc = this.GetComponent<BoxCollider>();
                    bc.size = new Vector3(0.9f, 0.9f, 0.9f);
                    bc.center = new Vector3(0f, 0f, 0f);
                }
                else if (meshLength == MeshLength.Long)
                {
                    objMesh.mesh = rlm.canyon_Meshes[b];
                    // Update the box collider
                    BoxCollider bc = this.GetComponent<BoxCollider>();
                    bc.size = new Vector3(0.9f, 4.9f, 0.9f);
                    bc.center = new Vector3(0f, -2f, 0f);
                }
                
            }
        }
        else
        {
            if (coverType == CoverType.Lines)
            {
                objMesh.mesh = rlm.floor_Lines_ObjMesh[b];
            }
            else if (coverType == CoverType.Carpet)
            {
                objMesh.mesh = rlm.floor_Carpet_ObjMesh[b];
                // Special case for b = 1, object layer becomes default instead of obstacle
                if (b == 1)
                {
                    this.gameObject.layer = (int)CollLayers.Default;
                }
                else
                {
                    this.gameObject.layer = (int)CollLayers.Obstacle;
                }
            }
            else if (coverType == CoverType.Grate)
            {
                objMesh.mesh = rlm.grate_Meshes[b];
            }
            else
            {
                objMesh.mesh = rlm.floor_ObjMesh[b];
            }
        }

        // Updating material
        if (isFloor)
        {
            if (meshType == MeshType.Slab)
            {
                objMat.material = rlm.slab_Materials[b];
            }
            else if (meshType == MeshType.Island)
            {
                objMat.material = rlm.material_Island;
            }
            else if (meshType == MeshType.Canyon)
            {
                objMat.material = rlm.material_Island_Canyon;
            }
            else if (meshType == MeshType.Moss)
            {
                objMat.material = rlm.material_Island_Moss;
            }
            else if (meshType == MeshType.Onyx)
            {
                objMat.material = rlm.material_Island_Onyx;
            }
            else if (meshType == MeshType.Vines)
            {
                objMat.material = rlm.material_Island_Vines;
            }
            else if (meshType == MeshType.Reference)
            {
                objMat.material = reference_Material;
            }
            else if (meshType == MeshType.Frames)
            {
                objMat.material = reference_Material;
            }
        }
        else
        {
            if (coverType == CoverType.Grass)
            {
                objMat.material = rlm.material_Cover_Grass;
            }
            else if (coverType == CoverType.Grass_Light)
            {
                objMat.material = rlm.material_Cover_Grass_Light;
            }
            else if (coverType == CoverType.Grass_Dark)
            {
                objMat.material = rlm.material_Cover_Grass_Dark;
            }
            else if (coverType == CoverType.Dirt)
            {
                objMat.material = rlm.material_Cover_Dirt;
            }
            else if (coverType == CoverType.Lines)
            {
                objMat.material = rlm.material_Cover_Lines;
            }
            else if (coverType == CoverType.Carpet)
            {
                objMat.material = rlm.carpet_Materials[b];
            }
            else if (coverType == CoverType.Grate)
            {
                if ((q % 4) == 0)
                {
                    objMat.material = rlm.material_Cover_Grate;
                }
                else
                {
                    objMat.material = rlm.material_Cover_Grate_Rotated;
                }
            }
            else if (coverType == CoverType.Reference)
            {
                objMat.material = reference_Material;
            }
        }
    }
}
