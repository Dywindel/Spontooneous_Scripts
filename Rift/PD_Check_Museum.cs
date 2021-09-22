using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class PD_Check_Museum
{
    // Check solutions for museum icons
    public static bool PD_C_M(RuneData runeData, RiftObj riftObj)
    {
        GridElement[,,] arrayOf_GridElements = new GridElement[riftObj.gsly, riftObj.gsx, riftObj.gsy];
        // Go through each bridgeData in that rift's array
        // Create empty classes for each position
        for (int ly = 0; ly < riftObj.gsly; ly ++)
        {
            for (int i = 0; i < riftObj.gsx; i ++)
            {
                for (int j = 0; j < riftObj.gsy; j ++)
                {
                    arrayOf_GridElements[ly, i, j] = new GridElement();
                }
            }
        }
        // Create the truth table using our premade rules function
        PD_Check.Return_ArrayOf_GridElement(ref arrayOf_GridElements, riftObj, new int[3] {riftObj.gsly, riftObj.gsx, riftObj.gsy}, new int[3] {0, 0, 0});

        ////////////////
        // 0 - Last Tree
        if (runeData.museumType == MuseumType.LastTree)
        {
            // There are six trees (Six blank squares)
            // Each tree must be adjacent to no other trees
            
            // Go through each square
            // If it's empty, there should be no empty squares next to it
            bool isHeld = true;
            // There must be only six empty squares
            int emptySquareTotal = 0;
            for (int i = 0; i < riftObj.gsx; i++)
            {
                // And each column, y
                for (int j = 0; j < riftObj.gsy; j++)
                {
                    // Check if not path
                    if (!arrayOf_GridElements[runeData.pos[0], i, j].isPath)
                    {
                        emptySquareTotal += 1;

                        // Check if adjacent squares are not paths
                        // Check either side of the X direction
                        for (int ix = i - 1; ix <= i + 1; ix ++)
                        {
                            // Make sure we're inside the grid
                            if (ix >= 0 && ix < riftObj.gsx)
                            {
                                // Ignore the centre square
                                if (ix != i)
                                {
                                    // If a Path doesn't exist here, the rule is broken
                                    if (!arrayOf_GridElements[runeData.pos[0], ix, j].isPath)
                                    {
                                        isHeld = false;
                                    }
                                }
                            }
                        }

                        // Check either side of the Y direction
                        for (int jy = j - 1; jy <= j + 1; jy ++)
                        {
                            // Make sure we're inside the grid
                            if (jy >= 0 && jy < riftObj.gsy)
                            {
                                // Ignore the centre square
                                if (jy != j)
                                {
                                    // If a Path doesn't exist here, the rule is broken
                                    if (!arrayOf_GridElements[runeData.pos[0], i, jy].isPath)
                                    {
                                        isHeld = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // If there were more of less than six empty squares, this fails
            if (emptySquareTotal != 6)
            {
                isHeld = false;
            }

            return isHeld;
        }

        ////////////////
        // 1 - Hammer
        if (runeData.museumType == MuseumType.Hammers)
        {
            ///////////////////
            // Hammer - RULES
            // 1. All corners must have one adjacent neighbour
            // 2. All edges must have two adjacent neighbours
            // 3. All others must have three adjacent neighbours

            // Check if held
            bool isHeld = true;

            // Go through every square on the grid
            for (int i = 0; i < riftObj.gsx; i++)
            {
                // And each column, y
                for (int j = 0; j < riftObj.gsy; j++)
                {
                    // We only need to check paths
                    if (arrayOf_GridElements[runeData.pos[0], i, j].isPath)
                    {
                        // For corners
                        if ((i == 0 && j == 0)
                            || (i == riftObj.gsx - 1 && j == 0)
                            || (i == 0 && j == riftObj.gsy - 1)
                            || (i == riftObj.gsx - 1 && j == riftObj.gsy - 1))
                        {
                            // Check total adjacent paths
                            int totalAP = RL_F.C_AP(new int[3] {runeData.pos[0], i, j}, riftObj, arrayOf_GridElements);
                            if (totalAP != 1)
                            {
                                isHeld = false;
                            }
                        }

                        // For edges
                        else if (i == 0
                                || i == riftObj.gsx - 1
                                || j == 0
                                || j == riftObj.gsy - 1)
                        {
                            int totalAP = RL_F.C_AP(new int[3] {runeData.pos[0], i, j}, riftObj, arrayOf_GridElements);
                            if (totalAP != 2)
                            {
                                isHeld = false;
                            }
                        }

                        // For all other pieces
                        else
                        {
                            int totalAP = RL_F.C_AP(new int[3] {runeData.pos[0], i, j}, riftObj, arrayOf_GridElements);
                            Debug.Log("This centre has: " + totalAP);
                            if (totalAP != 3)
                            {
                                isHeld = false;
                            }
                        }
                    }
                }
            }

            return isHeld;
        }

        ///////////////////
        // 2 - Security Eye
        if (runeData.museumType == MuseumType.SecurityEye)
        {
            ////////////////
            // Eye - Rules
            // - All other paths must have two neighbouring paths only

            // We break if, at any point, the icon has failed
            bool isHeld = true;

            // Check all pieces
            // I'll turn this into a function later
            for (int ii = 0; ii < riftObj.gsx; ii++)
            {
                for (int jj = 0; jj < riftObj.gsy; jj++)
                {
                    int[] tempPos = new int[3] {runeData.pos[0], ii, jj};
                    
                    // We only check a position if it has a path itself
                    if (arrayOf_GridElements[tempPos[0], tempPos[1], tempPos[2]].isPath)
                    {
                        if (RL_F.C_AP(tempPos, riftObj, arrayOf_GridElements) != 2)
                        {
                            Debug.Log("Too many adjacent!");
                            isHeld = false;
                        }
                    }
                }
            }
            
            return isHeld;
        }

        return false;
    }
}
