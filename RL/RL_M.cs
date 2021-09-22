using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used for meshes and textures that need to be collect via reference

public class RL_M : MonoBehaviour
{
    ///////////////
    // OBSTACLES //
    ///////////////

    public GameObject rockObj;

    ///////////
    // RUNES //
    ///////////

    public GameObject runeObj;

    public Sprite[] runeTextures;
    public Sprite[] runeMuseumTextures;
    public Sprite[] runeBossTextures;

    public Sprite[] runeTextures_Tapa;
    public Sprite[] runeTextures_Pin;
    public Sprite[] runeTextures_Power;
    public Sprite[] runeTextures_Exits;

    public Material[] gridLines;

    /////////////
    // BRIDGES //
    /////////////

    public Mesh[] bridge_BridgeMeshes;
    public Mesh[] bridge_StairsMeshes;
    public Mesh[] bridge_PlankMeshes;
    public Mesh[] bridge_BridgeMesh_Wooden;
    public Mesh[] bridge_BridgeMesh_City;

    public Material[] bridge_Materials;

    ///////////
    // SLABS //
    ///////////

    public Mesh[] slab_Meshes;
    public Mesh[] slabFlat_Meshes;
    public Mesh[] canyon_Meshes;
    public Mesh[] grate_Meshes;
    public Mesh[] floor_ObjMesh;
    public Mesh[] floor_Lines_ObjMesh;
    public Mesh[] floor_Carpet_ObjMesh;
    public Mesh[] floor_Frames;
    public Material[] slab_Materials;
    public Material[] carpet_Materials;
    public Material material_Island;
    public Material material_Island_Canyon;
    public Material material_Island_Moss;
    public Material material_Island_Onyx;
    public Material material_Island_Vines;
    public Material material_Island_Dirt;
    public Material material_Cover_Grass;
    public Material material_Cover_Grass_Light;
    public Material material_Cover_Grass_Dark;
    public Material material_Cover_Dirt;
    public Material material_Cover_Lines;
    public Material material_Cover_Grate;
    public Material material_Cover_Grate_Rotated;
}
