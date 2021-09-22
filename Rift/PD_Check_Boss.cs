using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Solution checks for boss type icons

public class PD_Check_Boss : MonoBehaviour
{
    public static bool PD_C_B(RuneData runeData, RiftObj riftObj)
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
        // 0 - L Piece
        if (runeData.bossType == BossType.LPiece)
        {
            // Detects a single L-Piece on the grid.
            // The piece can fit horizontally, vertically or diagonally, as long as it's arms are the same length

            bool isHeld_HorVer = true;
            bool isHeld_Diag = true;

            // Get length of rows and cols
            // They all have to be 0, 1 or two instances of the same length
            // They all have to have single paths

            // HOR/VER
            # region HOR/Ver

            // Check number of paths for each row and column and return path lengths
            List<int>[] rowPaths = new List<int>[riftObj.gsy];
            List<int>[] colPaths = new List<int>[riftObj.gsx];

            for (int i = 0; i < riftObj.gsx; i++)
            {
                // Initiate the lists
                colPaths[i] = new List<int>();
                int totalLength = 0;

                for (int j = 0; j < riftObj.gsy; j++)
                {
                    if (arrayOf_GridElements[runeData.pos[0], i, j].isPath)
                    {
                        // Add to total
                        totalLength += 1;
                    }
                    else
                    {
                        if (totalLength > 0)
                        {
                            // Mark the path
                            colPaths[i].Add(totalLength);
                            // Reset total
                            totalLength = 0;
                        }
                    }
                }

                // Finally, just in case we reach the end without an additional path
                if (totalLength > 0)
                {
                    // Mark the path
                    colPaths[i].Add(totalLength);
                    // Reset total
                    totalLength = 0;
                }
            }

            for (int j = 0; j < riftObj.gsy; j++)
            {
                // Initiate the lists
                rowPaths[j] = new List<int>();
                int totalLength = 0;

                for (int i = 0; i < riftObj.gsx; i++)
                {
                    if (arrayOf_GridElements[runeData.pos[0], i, j].isPath)
                    {
                        // Add to total
                        totalLength += 1;
                    }
                    else
                    {
                        if (totalLength > 0)
                        {
                            // Mark the path
                            rowPaths[j].Add(totalLength);
                            // Reset total
                            totalLength = 0;
                        }
                    }
                }

                // Finally, just in case we reach the end without an additional path
                if (totalLength > 0)
                {
                    // Mark the path
                    rowPaths[j].Add(totalLength);
                    // Reset total
                    totalLength = 0;
                }
            }

            // Check all paths are single length paths
            for (int a = 0; a < colPaths.Length; a++)
            {
                if (colPaths[a].Count > 1)
                {
                    isHeld_HorVer = false;
                }
            }

            for (int a = 0; a < rowPaths.Length; a++)
            {
                if (rowPaths[a].Count > 1)
                {
                    isHeld_HorVer = false;
                }
            }

            // Check all paths are either 0, 1 or a special number that is reflected in the other list
            int armLength_Hor = 0;
            for (int a = 0; a < rowPaths.Length; a++)
            {
                if (rowPaths[a].Count > 0 && rowPaths[a].First() != 1 && rowPaths[a].First() != 0)
                {
                    // Store this value once
                    if (armLength_Hor == 0)
                    {
                        armLength_Hor = rowPaths[a].First();
                    }
                    else
                    {
                        isHeld_HorVer = false;
                    }
                }
            }

            // Check other direction
            int armLength_Ver = 0;
            for (int a = 0; a < colPaths.Length; a++)
            {
                if (colPaths[a].Count > 0 && colPaths[a].First() != 1 && colPaths[a].First() != 0)
                {
                    // Store this value once
                    if (armLength_Ver == 0)
                    {
                        armLength_Ver = colPaths[a].First();
                    }
                    else
                    {
                        isHeld_HorVer = false;
                    }
                }
            }

            // Check the lengths match
            if (armLength_Hor != armLength_Ver)
            {
                isHeld_HorVer = false;
            }

            // check the length is above 1
            if (armLength_Hor <= 1)
            {
                isHeld_HorVer = false;
            }

            // Check that the longest paths are next to a path of zero length
            for (int j = 0; j < rowPaths.Length; j++)
            {
                if (rowPaths[j].Count > 0 && rowPaths[j].First() == armLength_Hor)
                {
                    // Check above and below

                    if ((j > 0 && rowPaths[j - 1].Count > 0) &&
                        (j + 1 < riftObj.gsy && rowPaths[j + 1].Count > 0)  )
                    {
                        isHeld_HorVer = false;
                    }
                }
            }

            for (int i = 0; i < colPaths.Length; i++)
            {
                if (colPaths[i].Count > 0 && colPaths[i].First() == armLength_Ver)
                {
                    // Check above and below

                    if ((i > 0 && colPaths[i - 1].Count > 0) &&
                        (i + 1 < riftObj.gsx && colPaths[i + 1].Count > 0)  )
                    {
                        isHeld_HorVer = false;
                    }
                }
            }

            # endregion

            // DIAGONAL

            # region Diagonal

            // First, grab the extreme points on the edge of the shape
            // Top, right, bottom, left
            int[] extremePoints = new int[4] {-2, -2, -2, -2};
            int doubleEdge = -1;    // This will be useful later
            // Use the row and column length collections that were made earlier
            // Check from top to bottom and bottom to top for 1 path of length 1
            // Left to Right
            for (int a = 0; a < colPaths.Length; a++)
            {
                if (colPaths[a].Count > 0)
                {
                    // If this has 1 path of length 1
                    if (colPaths[a].Count == 1 && colPaths[a].First() == 1)
                    {
                        // Store this value
                        extremePoints[3] = a;
                        break;
                    }
                    // If this has two paths
                    else if (colPaths[a].Count == 2)
                    {
                        extremePoints[3] = -1;
                        doubleEdge = a;
                        break;
                    }
                    // In any other case
                    else
                    {
                        // Extreme point stays as -2
                        // We fail here
                        isHeld_Diag = false;
                        break;
                    }
                }
            }
            // Right to Left
            for (int a = colPaths.Length - 1; a >= 0; a--)
            {
                if (colPaths[a].Count > 0)
                {
                    if (colPaths[a].Count == 1 && colPaths[a].First() == 1)
                    {
                        // Store this value
                        extremePoints[1] = a;
                        break;
                    }
                    // If this has two paths
                    else if (colPaths[a].Count == 2)
                    {
                        extremePoints[1] = -1;
                        doubleEdge = a;
                        break;
                    }
                    // In any other case
                    else
                    {
                        // We fail here
                        isHeld_Diag = false;
                        break;
                    }
                }
            }
            // Bottom to Top
            for (int a = 0; a < rowPaths.Length; a++)
            {
                if (rowPaths[a].Count > 0)
                {
                    if (rowPaths[a].Count == 1 && rowPaths[a].First() == 1)
                    {
                        // Store this value
                        extremePoints[2] = a;
                        break;
                    }
                    // If this has two paths
                    else if (rowPaths[a].Count == 2)
                    {
                        extremePoints[2] = -1;
                        doubleEdge = a;
                        break;
                    }
                    // In any other case
                    else
                    {
                        // We fail here
                        isHeld_Diag = false;
                        break;
                    }
                }
            }
            // Top to Bottom
            for (int a = rowPaths.Length - 1; a >= 0; a--)
            {
                if (rowPaths[a].Count > 0)
                {
                    if (rowPaths[a].Count == 1 && rowPaths[a].First() == 1)
                    {
                        // Store this value
                        extremePoints[0] = a;
                        break;
                    }
                    // If this has two paths
                    else if (rowPaths[a].Count == 2)
                    {
                        extremePoints[0] = -1;
                        doubleEdge = a;
                        break;
                    }
                    // In any other case
                    else
                    {
                        // We fail here
                        isHeld_Diag = false;
                        break;
                    }
                }
            }

            // Check there is only one -1 value in this list
            // And also check that no value is -2
            int failCheck = 0;
            for (int a = 0; a < extremePoints.Length; a++)
            {
                if (extremePoints[a] == -1)
                {
                    failCheck += 1;
                }
                else if (extremePoints[a] == -2)
                {
                    isHeld_Diag = false;
                }
            }
            if (failCheck != 1)
            {
                isHeld_Diag = false;
            }

            // Now we check these extreme point are positioned correctly
            // We want to ignore other cases
            if (isHeld_Diag)
            {
                // Find the index of the -1 value
                int pointIndex = -1;
                for (int a = 0; a < extremePoints.Length; a++)
                {
                    if (extremePoints[a] == -1)
                    {
                        pointIndex = a;
                        // Change the extremePoint[a] value to its actual position, to make things flow easier later on
                        extremePoints[a] = doubleEdge;
                    }
                }

                // Then, we use this info to check the diagonals
                // We check the diagonals going away from the -1 end on either side of the -1 piece
                // I'm just going to write this algorithm out 4 times for now and figure the rest out later
                int tempIndex = (pointIndex + 1) % 4;
                int tempIndexPrv = (pointIndex + 3) % 4;
                int tempIndexCorner = (pointIndex + 2) % 4;

                // Get all the positions of the major components. This will be useful later
                int[] pos_North = new int[2];
                int[] pos_East = new int[2];
                int[] pos_South = new int[2];
                int[] pos_West = new int[2];

                // North & South Position
                // Go along with extreme North and South row and find the first path
                for (int i = 0; i < riftObj.gsx; i++)
                {
                    if (arrayOf_GridElements[runeData.pos[0], i, extremePoints[0]].isPath)
                    {
                        // Store these coordinates
                        pos_North = new int[2] {i, extremePoints[0]};
                        Debug.Log("North Position: " + i + " - " + extremePoints[0]);
                    }

                    if (arrayOf_GridElements[runeData.pos[0], i, extremePoints[2]].isPath)
                    {
                        // Store these coordinates
                        pos_South = new int[2] {i, extremePoints[2]};
                        Debug.Log("South Position: " + i + " - " + extremePoints[2]);
                    }
                }

                // East & West Position
                // Go along with extreme East and West row and find the first path
                for (int j = 0; j < riftObj.gsy; j++)
                {
                    if (arrayOf_GridElements[runeData.pos[0], extremePoints[1], j].isPath)
                    {
                        // Store these coordinates
                        pos_East = new int[2] {extremePoints[1], j};
                        Debug.Log("East Position: " + extremePoints[1] + " - " + j);
                    }

                    if (arrayOf_GridElements[runeData.pos[0], extremePoints[3], j].isPath)
                    {
                        // Store these coordinates
                        pos_West = new int[2] {extremePoints[3], j};
                        Debug.Log("West Position: " + extremePoints[3] + " - " + j);
                    }
                }

                // North
                # region North
                if (tempIndex == 0)
                {
                    // Check that the path from North to East is diagonal and also that it has a path at every point
                    int diffLong = Mathf.Abs(pos_North[0] - pos_East[0]);
                    if (diffLong != Mathf.Abs(pos_North[1] - pos_East[1]))
                    {
                        isHeld_Diag = false;
                    }

                    // Check the length of the short distance
                    int diffShort = pos_North[0] - pos_West[0];

                    // Check the North to East diagonals
                    // Do this next bit the number of times equal to the diffShort
                    for (int a = 0; a <= diffShort; a++)
                    {
                        // Take a diagonal path from North to East and check there are paths here
                        // Go through each square between these two points in a diagonal line and check if they're paths
                        // Also check the squares directly below them (If we're not on the last run of this)
                        for (int b = 0; b <= diffLong; b++)
                        {
                            // Start at North
                            if (!arrayOf_GridElements[runeData.pos[0], pos_North[0] - a + b, pos_North[1] - a - b].isPath)
                            {
                                // If not a path, this diag rule has failed
                                isHeld_Diag = false;
                            }

                            // If we're not on the last b piece and we're not on the last a piece
                            if (b != diffLong && a != diffShort)
                            {
                                // We can check for a path beneath us (South)
                                if (!arrayOf_GridElements[runeData.pos[0], pos_North[0] - a + b, pos_North[1] - a - b - 1].isPath)
                                {
                                    isHeld_Diag = false;
                                }
                            }
                        }
                    }

                    // Check the South to east diagonals
                    for (int a = 0; a <= diffShort; a++)
                    {
                        // Take a diagonal path from South to East and check there are paths here
                        // Go through each square between these two points in a diagonal line and check if they're paths
                        // Also check the squares directly above them (If we're not on the last run of this)
                        for (int b = 0; b <= diffLong; b++)
                        {
                            // Start at South
                            if (!arrayOf_GridElements[runeData.pos[0], pos_South[0] - a + b, pos_South[1] + a + b].isPath)
                            {
                                // If not a path, this diag rule has failed
                                isHeld_Diag = false;
                            }

                            // If we're not on the last b piece and we're not on the last a piece
                            if (b != diffLong && a != diffShort)
                            {
                                // We can check for a path beneath us (South)
                                if (!arrayOf_GridElements[runeData.pos[0], pos_South[0] - a + b, pos_South[1] + a + b + 1].isPath)
                                {
                                    isHeld_Diag = false;
                                }
                            }
                        }
                    }
                }

                # endregion
            
                // East
                # region East
                if (tempIndex == 1)
                {
                    // Check that the path from North to East is diagonal and also that it has a path at every point
                    int diffLong = Mathf.Abs(pos_East[0] - pos_South[0]);
                    if (diffLong != Mathf.Abs(pos_East[1] - pos_South[1]))
                    {
                        isHeld_Diag = false;
                    }

                    // Check the length of the short distance
                    int diffShort = pos_East[1] - pos_North[1];

                    // Check the East to South diagonals
                    // Do this next bit the number of times equal to the diffShort
                    for (int a = 0; a <= diffShort; a++)
                    {
                        // Take a diagonal path from North to East and check there are paths here
                        // Go through each square between these two points in a diagonal line and check if they're paths
                        // Also check the squares directly below them (If we're not on the last run of this)
                        for (int b = 0; b <= diffLong; b++)
                        {
                            // Start at North
                            if (!arrayOf_GridElements[runeData.pos[0], pos_East[0] - a - b, pos_East[1] + a - b].isPath)
                            {
                                // If not a path, this diag rule has failed
                                isHeld_Diag = false;
                            }

                            // If we're not on the last b piece and we're not on the last a piece
                            if (b != diffLong && a != diffShort)
                            {
                                // We can check for a path to the west of us
                                if (!arrayOf_GridElements[runeData.pos[0], pos_East[0] - a - b - 1, pos_East[1] + a - b].isPath)
                                {
                                    isHeld_Diag = false;
                                }
                            }
                        }
                    }

                    // Check the West to South diagonals
                    for (int a = 0; a <= diffShort; a++)
                    {
                        // Take a diagonal path from South to East and check there are paths here
                        // Go through each square between these two points in a diagonal line and check if they're paths
                        // Also check the squares directly above them (If we're not on the last run of this)
                        for (int b = 0; b <= diffLong; b++)
                        {
                            // Start at South
                            if (!arrayOf_GridElements[runeData.pos[0], pos_West[0] + a + b, pos_West[1] + a - b].isPath)
                            {
                                // If not a path, this diag rule has failed
                                isHeld_Diag = false;
                            }

                            // If we're not on the last b piece and we're not on the last a piece
                            if (b != diffLong && a != diffShort)
                            {
                                // We can check for a path East
                                if (!arrayOf_GridElements[runeData.pos[0], pos_West[0] + a + b + 1, pos_West[1] + a - b].isPath)
                                {
                                    isHeld_Diag = false;
                                }
                            }
                        }
                    }
                }

                # endregion

                // South
                # region South
                if (tempIndex == 2)
                {
                    // Check that the path from North to East is diagonal and also that it has a path at every point
                    int diffLong = Mathf.Abs(pos_South[0] - pos_West[0]);
                    if (diffLong != Mathf.Abs(pos_South[1] - pos_West[1]))
                    {
                        isHeld_Diag = false;
                    }

                    // Check the length of the short distance
                    int diffShort = pos_South[0] - pos_East[0];

                    // Check the South to West diagonals
                    // Do this next bit the number of times equal to the diffShort
                    for (int a = 0; a <= diffShort; a++)
                    {
                        // Take a diagonal path from North to East and check there are paths here
                        // Go through each square between these two points in a diagonal line and check if they're paths
                        // Also check the squares directly below them (If we're not on the last run of this)
                        for (int b = 0; b <= diffLong; b++)
                        {
                            // Start at North
                            if (!arrayOf_GridElements[runeData.pos[0], pos_South[0] + a - b, pos_South[1] + a + b].isPath)
                            {
                                // If not a path, this diag rule has failed
                                isHeld_Diag = false;
                            }

                            // If we're not on the last b piece and we're not on the last a piece
                            if (b != diffLong && a != diffShort)
                            {
                                // We can check for a path North
                                if (!arrayOf_GridElements[runeData.pos[0], pos_South[0] + a - b, pos_South[1] + a + b + 1].isPath)
                                {
                                    isHeld_Diag = false;
                                }
                            }
                        }
                    }

                    // Check the North to West diagonals
                    for (int a = 0; a <= diffShort; a++)
                    {
                        // Take a diagonal path from South to East and check there are paths here
                        // Go through each square between these two points in a diagonal line and check if they're paths
                        // Also check the squares directly above them (If we're not on the last run of this)
                        for (int b = 0; b <= diffLong; b++)
                        {
                            // Start at South
                            if (!arrayOf_GridElements[runeData.pos[0], pos_North[0] + a - b, pos_North[1] - a - b].isPath)
                            {
                                // If not a path, this diag rule has failed
                                isHeld_Diag = false;
                            }

                            // If we're not on the last b piece and we're not on the last a piece
                            if (b != diffLong && a != diffShort)
                            {
                                // We can check for a path South
                                if (!arrayOf_GridElements[runeData.pos[0], pos_North[0] + a - b, pos_North[1] - a - b - 1].isPath)
                                {
                                    isHeld_Diag = false;
                                }
                            }
                        }
                    }
                }

                # endregion

                // West
                # region West
                if (tempIndex == 3)
                {
                    // Check that the path from North to East is diagonal and also that it has a path at every point
                    int diffLong = Mathf.Abs(pos_West[0] - pos_North[0]);
                    if (diffLong != Mathf.Abs(pos_West[1] - pos_North[1]))
                    {
                        isHeld_Diag = false;
                    }

                    // Check the length of the short distance
                    int diffShort = pos_West[1] - pos_South[1];

                    // Check the West to North diagonals
                    // Do this next bit the number of times equal to the diffShort
                    for (int a = 0; a <= diffShort; a++)
                    {
                        // Take a diagonal path from North to East and check there are paths here
                        // Go through each square between these two points in a diagonal line and check if they're paths
                        // Also check the squares directly below them (If we're not on the last run of this)
                        for (int b = 0; b <= diffLong; b++)
                        {
                            // Start at North
                            if (!arrayOf_GridElements[runeData.pos[0], pos_West[0] + a + b, pos_West[1] - a + b].isPath)
                            {
                                // If not a path, this diag rule has failed
                                isHeld_Diag = false;
                            }

                            // If we're not on the last b piece and we're not on the last a piece
                            if (b != diffLong && a != diffShort)
                            {
                                // We can check for a path East
                                if (!arrayOf_GridElements[runeData.pos[0], pos_West[0] + a + b + 1, pos_West[1] - a + b].isPath)
                                {
                                    isHeld_Diag = false;
                                }
                            }
                        }
                    }

                    // Check the East to North diagonals
                    for (int a = 0; a <= diffShort; a++)
                    {
                        // Take a diagonal path from South to East and check there are paths here
                        // Go through each square between these two points in a diagonal line and check if they're paths
                        // Also check the squares directly above them (If we're not on the last run of this)
                        for (int b = 0; b <= diffLong; b++)
                        {
                            // Start at South
                            if (!arrayOf_GridElements[runeData.pos[0], pos_East[0] - a - b, pos_East[1] - a + b].isPath)
                            {
                                // If not a path, this diag rule has failed
                                isHeld_Diag = false;
                            }

                            // If we're not on the last b piece and we're not on the last a piece
                            if (b != diffLong && a != diffShort)
                            {
                                // We can check for a path West
                                if (!arrayOf_GridElements[runeData.pos[0], pos_East[0] - a - b - 1, pos_East[1] - a + b].isPath)
                                {
                                    isHeld_Diag = false;
                                }
                            }
                        }
                    }
                }

                # endregion
            }

            # endregion
            
            // FINAL

            // Only one of these booleans has to fail
            if (!isHeld_HorVer && !isHeld_Diag)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    
        return false;
    }
}
