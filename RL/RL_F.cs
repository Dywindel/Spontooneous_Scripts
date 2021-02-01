using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Reference library of functions
public static class RL_F
{
    // Takes the vector different between two points and return relative grid position
    public static int[] Return_IntArray_Difference(Vector3 startPos, Vector3 endPos)
    {
        int[] intArray = new int[3];
        intArray[0] = (int)(startPos.y - endPos.y);     // Layer
        intArray[1] = (int)(startPos.x - endPos.x);     // X pos
        intArray[2] = (int)(startPos.z - endPos.z);     // Y pos

        return intArray;
    }

    public static Vector3 Return_Vector3_Difference(int[] intArray, Vector3 riftPos)
    {
        Vector3 endPos = new Vector3(intArray[1], intArray[0], intArray[2]) + riftPos;

        return endPos;
    }
}

