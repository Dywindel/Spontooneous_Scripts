using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Reference library of functions
public static class RL_F
{


    // UNIT CONVERSIONS

    // Turns a Vector3 in Unity space into an intArr in game space
    public static int[] V3_IA(Vector3 pVec3)
    {
        int[] intArr = new int[3];
        intArr[0] = (int)pVec3.y;     // Layer
        intArr[1] = (int)pVec3.x;     // X pos
        intArr[2] = (int)pVec3.z;     // Y pos

        return intArr;
    }

    // Turns an intArr in game space into a Vector3 in Unity space
    public static Vector3 IA_V3(int[] pIntArr)
    {
        Vector3 vec3 = new Vector3(pIntArr[1], pIntArr[0], pIntArr[2]);

        return vec3;
    }

    // Turns a V3 in game space into a F3 in game space
    public static float[] V3_F3(Vector3 pVec3)
    {
        float[] floatArr = new float[3];
        floatArr[0] = pVec3[0];
        floatArr[1] = pVec3[1];
        floatArr[2] = pVec3[2];

        return floatArr;
    }

    // Turn a F3 in game space into a V3 in game space
    public static Vector3 F3_V3(float[] pF3)
    {
        Vector3 vec3 = new Vector3(pF3[0], pF3[1], pF3[2]);
        return vec3;
    }

    public static int[] V3DFV3_IA(Vector3 startVec, Vector3 endVector)
    {
        Vector3 diffVec = startVec - endVector;

        int[] intArr = V3_IA(diffVec);

        return intArr;
    }

    // Returns a vector of a Vector position in Unity space plus an int array in game space
    public static Vector3 IADFV3_V3(int[] intArr, Vector3 pVec3)
    {
        Vector3 vec3 = IA_V3(intArr) + pVec3;

        return vec3;
    }



    // PATH CHECKING
    
    // We'll be checking a lot for bridge and stair components, but no planks. This functions is useful
    public static bool C_BS(RiftObj riftObj, int[] pos)
    {
        if (riftObj.arrayOf_BridgeData[pos[0], pos[1], pos[2]].bridgeType == BridgeType.Bridge)
        {
            return true;
        }
        else if (riftObj.arrayOf_BridgeData[pos[0], pos[1], pos[2]].bridgeType == BridgeType.Stairs)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // C_BS, but for only sending in the array
    public static bool C_BS_A(BridgeData[,,] arrayOf_BridgeData, int[] pos)
    {
        if (arrayOf_BridgeData[pos[0], pos[1], pos[2]].bridgeType == BridgeType.Bridge)
        {
            return true;
        }
        else if (arrayOf_BridgeData[pos[0], pos[1], pos[2]].bridgeType == BridgeType.Stairs)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // When checking up and down movement, we also need to consider stairs
    public static bool C_BS_UD(BridgeData[,,] arrayOf_BridgeData, int[] pos, DirType dirType)
    {
        if (dirType == DirType.N || dirType == DirType.E || dirType == DirType.S || dirType == DirType.W)
        {
            return C_BS_A(arrayOf_BridgeData, pos);
        }
        else if (dirType == DirType.U)
        {
            // Check this bridge item is a stair
            if (arrayOf_BridgeData[pos[0], pos[1], pos[2]].bridgeType == BridgeType.Stairs)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (dirType == DirType.D)
        {
            // Check the bridge item above this spot is a stair
            if (arrayOf_BridgeData[pos[0] + 1, pos[1], pos[2]].bridgeType == BridgeType.Stairs)
            {
                return true;
            }
            else
            {
                return false;
            }
        }  
        else
        {
            return false;
        }
    }

    // Take the passed position and add a unit direction to it as dictated by the passed direction
    public static int[] U_PS_DT(int[] pPos, DirType dirType)
    {
        // Remember, the Vector3 coordinate system is different from my system!
        // Unity goes X, Y, Z
        // I go Ly, X, Y, because I do ok.
        Vector3 dir_To_Vect = RL.dir_To_Vect[dirType];
        int[] rIntArr = new int[3] {pPos[0] + (int)dir_To_Vect[1], pPos[1] + (int)dir_To_Vect[0], pPos[2] + (int)dir_To_Vect[2]};

        return rIntArr;
    }

    // Check position value is inside the rift grid
    // Return false if not
    public static bool C_IG(int[] gs, int[] pos)
    {
        if (pos[0] < 0 || pos[1] < 0 || pos[2] < 0)
        {
            return false;
        }
        else if (pos[0] >= gs[0] || pos[1] >= gs[1] || pos[2] >= gs[2])
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // Check the total adjacent paths
    public static int C_AP(int[] tempPos, RiftObj riftObj, GridElement[,,] arrayOf_GridElements)
    {
        int totalAdjacent = 0;

        for (int i = tempPos[1] - 1; i <= tempPos[1] + 1; i++)
        {
            for (int j = tempPos[2] - 1; j <= tempPos[2] + 1; j++)
            {
                // We want to check all the spaces where only ONE i or j value is equal to it's corresponding runeData value
                if (!(i == tempPos[1] && j == tempPos[2])
                    && (i == tempPos[1] || j == tempPos[2]))
                {
                    // Make sure we're inside the grid
                    if (RL_F.C_IG(new int[3] {riftObj.gsly, riftObj.gsy, riftObj.gsy}, new int[3] {tempPos[0], i, j}))
                    {
                        // If there's a path here, increment
                        if (arrayOf_GridElements[tempPos[0], i, j].isPath)
                        {
                            totalAdjacent += 1;
                        }
                    }
                }
            }
        }

        return totalAdjacent;
    }

    // Reverse grab array position of item in array
    public static int[] R_AL_BD(BridgeData[,,] array, BridgeData bridgeData)
    {
        for (int ly = 0; ly < array.GetLength(0); ly ++)
        {
            for (int i = 0; i < array.GetLength(1); i ++)
            {
                for (int j = 0; j < array.GetLength(2); j ++)
                {
                    // If this bridgeData items match, return their position
                    if (array[ly, i, j] == bridgeData)
                    {
                        return new int[3] {ly, i, j};
                    }
                }
            }
        }

        // In case of failure, return null coordinates
        return new int[3] {0, 0, 0};
    }

    // Turns a relative pos value into an indexed list going clockwise, where north = 0
    // Takes all three terms, but technically only uses [1] X and [2] Y
    public static int T_PS(int[] intArr)
    {
        if (intArr[1] == 0 && intArr[2] == 0)
        {
            return 5;
        }
        else if (intArr[1] == 1 && intArr[2] == 0)
        {
            return 4;
        }
        else if (intArr[1] == 2 && intArr[2] == 0)
        {
            return 3;
        }
        else if (intArr[1] == 0 && intArr[2] == 1)
        {
            return 6;
        }
        else if (intArr[1] == 2 && intArr[2] == 1)
        {
            return 2;
        }
        else if (intArr[1] == 0 && intArr[2] == 2)
        {
            return 7;
        }
        else if (intArr[1] == 1 && intArr[2] == 2)
        {
            return 0;
        }
        else if (intArr[1] == 2 && intArr[2] == 2)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    // Compare moveCard to Plank direction
    // Returns true if movement is allowed
    public static bool C_MC_PD(DirType moveCard, PlankDir plankDir)
    {
        // If the plank is horizontal, this is considered to be E <-> W facing 
        // We allow E and W moveCards
        if (plankDir == PlankDir.Hor)
        {
            if (moveCard == DirType.E || moveCard == DirType.W)
            {
                return true;
            }
        }
        // If the plank is vertical, this is considered to be N <-> S facing 
        // We allow N and S moveCards
        else if (plankDir == PlankDir.Ver)
        {
            if (moveCard == DirType.N || moveCard == DirType.S)
            {
                return true;
            }
        }
        else if (plankDir == PlankDir.Crs)
        {
            return true;
        }

        return false;
    }

    // This bridge types are allowable for the player to walk across
    public static bool C_AB(BridgeType bridgeType)
    {
        if (bridgeType == BridgeType.Static ||
            bridgeType == BridgeType.Bridge ||
            bridgeType == BridgeType.Stairs)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Rotate clockwise the passed dirType
    public static DirType R_DT(DirType dirType)
    {
        if (dirType == DirType.None || dirType == DirType.U || dirType == DirType.D)
        {
            return dirType;
        }
        else if (dirType == DirType.N)
        {
            return DirType.E;
        }
        else if (dirType == DirType.E)
        {
            return DirType.S;
        }
        else if (dirType == DirType.S)
        {
            return DirType.W;
        }
        else if (dirType == DirType.W)
        {
            return DirType.N;
        }

        return DirType.None;

    }


    ////////////////////
    // SORT GRID INFO //
    ////////////////////
    
    

    //////////////////
    // CAMERA ANGLE //
    //////////////////

    public static Vector3[] Select_CameraAngle(CameraAngle_Distance tCameraDistance, CameraAngle_Side tCameraSide, CameraAngle_Pitch tCameraPitch)
    {
        Vector3[] rCamera = new Vector3[2];
        rCamera[0] = Vector3.zero;
        rCamera[1] = Vector3.zero;

        if (tCameraDistance == CameraAngle_Distance.VeryClose)
        {
            rCamera[0].z = 2f;
            rCamera[1].x = 0.5f;
        }
        else if (tCameraDistance == CameraAngle_Distance.Close)
        {
            rCamera[0].z = 4f;
            rCamera[1].x = 1f;
        }
        else if (tCameraDistance == CameraAngle_Distance.Med)
        {
            rCamera[0].z = 7f;
            rCamera[1].x = 2f;
        }
        else if (tCameraDistance == CameraAngle_Distance.Far)
        {
            rCamera[0].z = 10f;
            rCamera[1].x = 3f;
        }
        else if (tCameraDistance == CameraAngle_Distance.VeryFar)
        {
            rCamera[0].z = 15f;
            rCamera[1].x = 5f;
        }
        else if (tCameraDistance == CameraAngle_Distance.Comet)
        {
            rCamera[0].z = 25f;
            rCamera[1].x = 5f;
        }

        if (tCameraSide == CameraAngle_Side.SideLeft)
        {
            rCamera[0].y = 90f;
        }
        else if (tCameraSide == CameraAngle_Side.Left)
        {
            rCamera[0].y = 15f;
        }
        else if (tCameraSide == CameraAngle_Side.Centre)
        {
            rCamera[0].y = 0f;
            rCamera[1].x = 0f;
        }
        else if (tCameraSide == CameraAngle_Side.Right)
        {
            rCamera[0].y = -15f;
        }
        else if (tCameraSide == CameraAngle_Side.SideRight)
        {
            rCamera[0].y = 90f;
        }
        else if (tCameraSide == CameraAngle_Side.LeftOblique)
        {
            rCamera[0].y = 45f;
        }
        else if (tCameraSide == CameraAngle_Side.RightOblique)
        {
            rCamera[0].y = -45f;
        }


        if (tCameraPitch == CameraAngle_Pitch.TopDown)
            rCamera[0].x = 90f;
        else if (tCameraPitch == CameraAngle_Pitch.Rift)
            rCamera[0].x = 80f;
        else if (tCameraPitch == CameraAngle_Pitch.Steep)
            rCamera[0].x = 60f;
        else if (tCameraPitch == CameraAngle_Pitch.Shallow)
            rCamera[0].x = 35f;
        else if (tCameraPitch == CameraAngle_Pitch.Horizontal)
            rCamera[0].x = 0f;


        return rCamera;
    }


    /////////////////////
    // MATHS FUNCTIONS //
    /////////////////////

    // Rounds up a vector3 to it's nearest int values
    public static Vector3 Round_V3(Vector3 p_V3)
    {
        Vector3 r_V3 = new Vector3 ((int)Mathf.Round(p_V3.x),
                                    (int)Mathf.Round(p_V3.y),
                                    (int)Mathf.Round(p_V3.z));

        return r_V3;
    }


    //////////////////
    // C# FUNCTIONS //
    //////////////////
}

