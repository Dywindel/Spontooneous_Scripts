using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This script stores the checking function to see if a puzzle has been solved

public static class PD_Check
{
    // The rift we're checking
    static RiftObj riftObj;
    // The Truth table for checking floors
    static GridElement[,,] arrayOf_GridElements;

    // This is the main function for checking if a puzzle is solved
    // Returns a list of RuneObjs that failed to have their rules implemented
    // Returns an empty list if all rules were followed
    public static void PD_C(RiftObj pRiftObj)
    {
        /////////////////
        // PREPERATION //
        /////////////////

        // If we're using the halved symbol
        bool has_RuneHalved = false;

        // Prepare any data for solving
        riftObj = pRiftObj;
        List<RuneData> listOf_FaltyRunes = new List<RuneData>();

        ///////////////////////
        // SETUP TRUTH TABLE //
        ///////////////////////

        arrayOf_GridElements = new GridElement[riftObj.gsly, riftObj.gsx, riftObj.gsy];
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
        Return_ArrayOf_GridElement(ref arrayOf_GridElements, riftObj, new int[3] {riftObj.gsly, riftObj.gsx, riftObj.gsy}, new int[3] {0, 0, 0});

        // Go through each Rune
        //foreach (RuneData runeData in riftObj.listOf_RuneData)
        for (int r = 0; r < riftObj.listOf_RuneData.Count; r++)
        {
            // Temp runeData for reference (This is a refence by value, not to the object itself)
            RuneData runeData = riftObj.listOf_RuneData[r];

            /////////////
            // A - Museum
            if (runeData.runeType == RuneType.Museum)
            {
                bool museumBool = PD_Check_Museum.PD_C_M(riftObj.listOf_RuneData[r], riftObj);
                FaultyRune(r, museumBool);
                continue;
            }

            ///////////
            // B - Boss
            if (runeData.runeType == RuneType.Boss)
            {
                bool bossBool = PD_Check_Boss.PD_C_B(riftObj.listOf_RuneData[r], riftObj);
                FaultyRune(r, bossBool);
                continue;
            }

            /////////////
            // 0 - Island
            if (runeData.runeType == RuneType.Island)
            {
                /////////////////
                // Island - Rules
                // - There must be a path, walkable, from one side of the level to the other
                // - The start and end point of the level must be part of one island
                // - This includes planks

                // Simply take the start and end positions
                // Check their segment values
                // Check the segements values are part of the same island
                int[] startPos_RS = RL_F.V3DFV3_IA(riftObj.startPoint.position, riftObj.transform.position);
                int[] endPos_RS = RL_F.V3DFV3_IA(riftObj.endPoint.position, riftObj.transform.position);

                int[] startPos_TS = new int[3] {startPos_RS[0] + riftObj.trellisOffset[0], startPos_RS[1] + riftObj.trellisOffset[1], startPos_RS[2] + riftObj.trellisOffset[2]};
                int[] endPos_TS = new int[3] {endPos_RS[0] + riftObj.trellisOffset[0], endPos_RS[1] + riftObj.trellisOffset[1], endPos_RS[2] + riftObj.trellisOffset[2]};

                // I only need to grab one segment as a sample to check
                // Make sure the start and end position exist as a path
                if (riftObj.pos_To_Trellis_Plank.ContainsKey(startPos_TS) && riftObj.pos_To_Trellis_Plank.ContainsKey(endPos_TS))
                {
                    int startSeg = riftObj.pos_To_Trellis_Plank[startPos_TS].listOf_Segments[0];
                    int endSeg = riftObj.pos_To_Trellis_Plank[endPos_TS].listOf_Segments[0];

                    if (riftObj.listOf_Segments_Plank[startSeg].island != riftObj.listOf_Segments_Plank[endSeg].island)
                    {
                        FaultyRune(r, false);
                        continue;
                    }
                }
                else
                {
                    FaultyRune(r, false);
                    continue;
                }                
            }

            ///////////
            // 1 - Must
            if (runeData.runeType == RuneType.Must)
            {
                ///////////////
                // Must - Rules
                // - A path must go here

                // The rules are stored in the function for setting up the grid array
                if (arrayOf_GridElements[runeData.pos[0], runeData.pos[1], runeData.pos[2]].isPath)
                {
                    // The rule is held
                }
                else
                {
                    // The rule has not been held, update it's data
                    FaultyRune(r, false);
                    continue;
                }
            }
        
            /////////////
            // 2 - Forbid
            else if (runeData.runeType == RuneType.Forbid)
            {
                ///////////////
                // Must - Rules
                // - A path can't go here

                if (!arrayOf_GridElements[runeData.pos[0], runeData.pos[1], runeData.pos[2]].isPath)
                {
                    // The rule is held
                }
                else
                {
                    // The rule has not been held, store the incorrect rune
                    FaultyRune(r, false);
                    continue;
                }
            }

            ////////////
            // 3 - Arrow
            else if (runeData.runeType == RuneType.Arrow)
            {
                ////////////////
                // Arrow - Rules
                // - At least one path in this direction (Exclusive)
                
                // False until proven true
                bool tempBool_Arrow = false;

                // Ignore this grid position
                // Follow the arrow
                // If there is a path, that counts as a win and you can stop checking
                if (A_ST(runeData.pos, runeData.dirType) < A_GS(runeData.pos, runeData.dirType))
                {
                    for (int i = A_ST(runeData.pos, runeData.dirType); i < A_GS(runeData.pos, runeData.dirType); i++)
                    {
                        int[] tPos = C_PS(runeData.pos, i, runeData.dirType);
                        if (arrayOf_GridElements[tPos[0], tPos[1], tPos[2]].isPath)
                        {
                            tempBool_Arrow = true;
                        }
                    }
                }

                // If at the end of this check, the arrow is true, we are ok
                // If not, we store the incorrect rune
                if (!tempBool_Arrow)
                {
                    FaultyRune(r, false);
                    continue;
                }
            }

            //////////////
            // 4 - Chevron
            else if (runeData.runeType == RuneType.Chevron)
            {
                //////////////////
                // Chevron - Rules
                // - Add up the number of paths on each side
                // - The shape of the chevron inequality must hold.

                // Add up all the Bridges/Stairs in this direction
                // Bridges above this Rune are position
                // Bridges below this Rune are negative
                // The final value must be above zero if pointing North or negative if pointing South
                // Ignore the Chevron's actual position

                int totalBridges = 0;
                for (int i = 0; i < C_GS(runeData.dirType); i++)
                {
                    // Ignore the Chevron's position
                    if (i != C_PS_1(runeData.pos, runeData.dirType))
                    {
                        // Consider paths
                        int[] tPos = C_PS(runeData.pos, i, runeData.dirType);
                        if (arrayOf_GridElements[tPos[0], tPos[1], tPos[2]].isPath)
                        {
                            // Negative if above
                            if (i > C_PS_1(runeData.pos, runeData.dirType))
                            {
                                totalBridges -= 1;
                            }
                            // Positive if below
                            else
                            {
                                totalBridges += 1;
                            }
                        }
                    }
                }

                // If the total paths are positive
                // We cound that as being more paths N, E and U from the Rune
                // The Chevron points the opposite direction to what we want, so
                // When pointing in these directions, we want a negative total
                // Which we confirm by negating a positive total
                if (runeData.dirType == DirType.N || 
                    runeData.dirType == DirType.E ||
                    runeData.dirType == DirType.U)
                {
                    if (totalBridges >= 0)
                    {
                        FaultyRune(r, false);
                        continue;
                    }
                }
                else if (runeData.dirType == DirType.S || 
                    runeData.dirType == DirType.W ||
                    runeData.dirType == DirType.D)
                {
                    // And vice versa
                    if (totalBridges <= 0)
                    {
                        FaultyRune(r, false);
                        continue;
                    }
                }

            }

            ///////////
            // 5 - Tapa
            else if (runeData.runeType == RuneType.Tapa)
            {
                ///////////////
                // Tapa - Rules
                // - The tapa markings indicate the number and value of grounds of paths around it
                // - But not necessarily the order of each block or where they're located

                // Grab a list of Paths around the RuneData as a binary series
                // Where '1' represents a valid placement
                int[] tapa_Binary = new int[8];

                // Go through each row, x
                for (int i = runeData.pos[1] - 1; i <= runeData.pos[1] + 1; i ++)
                {
                    // And each column, y
                    for (int j = runeData.pos[2] - 1; j <= runeData.pos[2] + 1; j++)
                    {
                        // Make sure we're inside the grid
                        if (i >= 0 && i < riftObj.gsx && j >= 0 && j < riftObj.gsy)
                        {
                            // Ignore the centre square
                            if (!(i == runeData.pos[1] && j == runeData.pos[2]))
                            {
                                // If a Path exists here, we add it to the binary
                                if (arrayOf_GridElements[runeData.pos[0], i, j].isPath)
                                {
                                    // Add to the binary array
                                    tapa_Binary[RL_F.T_PS(new int[3] 
                                        {runeData.pos[0], i - runeData.pos[1] + 1, j - runeData.pos[2] + 1})] = 1;
                                }
                            }
                        }
                    }
                }

                // Using the binary list, we turn it into a list of number blocks
                List<int> tapaBlocks = new List<int>();
                for (int j = 0; j < 8; j++)
                {
                    // If the first icon is 1, create a new block
                    if (j == 0 && tapa_Binary[0] == 1)
                    {
                        tapaBlocks.Add(1);
                    }

                    // If the current icon is 1 and the previous value was 1, increment the current block value
                    else if (j > 0 && tapa_Binary[j] == 1 && tapa_Binary[j - 1] == 1)
                    {
                        tapaBlocks[tapaBlocks.Count - 1] += 1;
                    }

                    // If the current icon is 0 and the previous value was 1, create a new block
                    else if (j > 0 && tapa_Binary[j] == 1 && tapa_Binary[j - 1] == 0)
                    {
                        tapaBlocks.Add(1);
                    }

                    // If we're on the last case. If this and the first icon are both 1, join them together
                    if (j == 7 && tapa_Binary[7] == 1 && tapa_Binary[0] == 1)
                    {
                        // As long as this isn't part of one single chain
                        if (tapaBlocks.Count > 1)
                        {
                            // Add the last block to the first
                            tapaBlocks[0] += tapaBlocks[tapaBlocks.Count - 1];
                            // Remove the last item from the list
                            tapaBlocks.RemoveAt(tapaBlocks.Count - 1);
                        }
                    }
                }

                // Rearange the tapa blocks in order
                tapaBlocks.Sort();

                bool tempBool_Tapa = true;

                if (tapaBlocks.Count == RL.dict_TapaIndex[(int)runeData.tapaType].Count)
                {
                    // Check this list matches the value set by the icon. If they don't match, the answer is wrong
                    for (int k = 0; k < tapaBlocks.Count; k++)
                    {
                        if (tapaBlocks[k] != RL.dict_TapaIndex[(int)runeData.tapaType][k])
                        {
                            tempBool_Tapa = false;
                        }
                    }
                }
                else
                {
                    tempBool_Tapa = false;
                }

                // If the tapa bool gets set to false, this rune has failed
                if (!tempBool_Tapa)
                {
                    FaultyRune(r, false);
                    continue;
                }
            }
        
            //////////
            // 6 - Pin
            else if (runeData.runeType == RuneType.Pin)
            {
                ///////////////
                // Pin - Rules
                // - This new setup uses information created in the riftObj script, the Trellis funciton
                // - In the Trellis function a map of bridge networks are created and stored to be used for checking here
                // - Every Pin must connect to at least one other pin
                // - The path can only be made what is considered a path
                // - The path doesn't have to be part of one puzzle grid, if other grids exist nearby

                // Start at the pinType.Start
                if (runeData.pinType == PinType.Start)
                {
                    // Check bool. We're looking for confirmation
                    bool checkPin = false;

                    // If the trellisSpace hasn't been set inside the riftObj yet, we can argue that is autofails
                    if (riftObj.trellisOffset.Length != 0)
                    {
                        // Check this bridge part is part of a segment
                        // By checking if its pos is inside the key list
                        int[] bridgePos = new int[3] {runeData.pos[0] + riftObj.trellisOffset[0], runeData.pos[1] + riftObj.trellisOffset[1], runeData.pos[2] + riftObj.trellisOffset[2]};
                        BridgeData startBridgeData = riftObj.arrayOf_BridgeData[runeData.pos[0], runeData.pos[1], runeData.pos[2]];

                        if (riftObj.pos_To_Trellis.ContainsKey(bridgePos))
                        {
                            // We grab a list of the segments
                            List<int> listOf_Segments = riftObj.pos_To_Trellis[bridgePos].listOf_Segments;

                            // Go through each segment that is attached to this pin
                            foreach (int seg in listOf_Segments)
                            {
                                // Go through each pos that is attached to each segment
                                for (int p = 0; p < riftObj.listOf_Segments[seg].listOf_Pos.Count; p++)
                                {
                                    int[] tPos = riftObj.listOf_Segments[seg].listOf_Pos[p];

                                    // I have to figure out which offset is the correct offset to use
                                    // Run through each adjoined rift
                                    foreach (RiftObj tRiftObj in riftObj.listOf_AdjoiningRifts)
                                    {
                                        // Check if the offset position inside their riftspace exists within their grid
                                        int[] tPos_Offset = new int[3] {tPos[0] - tRiftObj.trellisOffset[0], tPos[1] - tRiftObj.trellisOffset[1], tPos[2] - tRiftObj.trellisOffset[2]};

                                        if (RL_F.C_IG(new int[3]{tRiftObj.gsly, tRiftObj.gsx, tRiftObj.gsy}, tPos_Offset))
                                        {
                                            // Then, we can grab the rune item
                                            RuneData refRuneData = tRiftObj.arrayOf_RuneData[tPos_Offset[0], tPos_Offset[1], tPos_Offset[2]];

                                            // Ignore ourselves
                                            if (refRuneData != runeData)
                                            {

                                                // Is this bridgeData a PinType and is it a start version
                                                // In this method, we want to connect pins to each other. There is only one symbol now
                                                if (refRuneData.runeType == RuneType.Pin && refRuneData.pinType == PinType.Start)
                                                {
                                                    // Success!
                                                    checkPin = true;
                                                    // If we succeed, we don't care about anything else and can break
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }

                    if (!checkPin)
                    {
                        // Add this rune to the incorrect data list
                        FaultyRune(r, false);
                        continue;
                    }
                }
            }
        
            ////////////
            // 7 - Power
            else if (runeData.runeType == RuneType.Power && runeData.powerType == PowerType.Sink)
            {
                /////////////////
                // Power - Rules
                // - A power sink must be on the same island as at least one other power source
                // - This doesn't include planks?

                // We check if at any point we can confirm a connection
                bool checkPair = false;

                // Store this position in TS
                int[] runePos_TS = new int[3] {runeData.pos[0] + riftObj.trellisOffset[0], runeData.pos[1] + riftObj.trellisOffset[1], runeData.pos[2] + riftObj.trellisOffset[2]};

                // Check if it exists as a path in TS
                if (riftObj.pos_To_Trellis.ContainsKey(runePos_TS))
                {
                    // Grab any segment
                    int t_Seg = riftObj.pos_To_Trellis[runePos_TS].listOf_Segments[0];
                    // Grab the island it belongs to. This is the island our rune is on
                    int t_Island = riftObj.listOf_Segments[t_Seg].island;

                    // Go through every rune in this puzzle
                    foreach (RuneData t_RuneData in riftObj.listOf_RuneData)
                    {
                        // Ignore ourselves
                        if (t_RuneData != runeData)
                        {
                            // We're only looking for power sources
                            if (t_RuneData.runeType == RuneType.Power && t_RuneData.powerType == PowerType.Source)
                            {
                                // Check that the power source is in its ON state
                                // This occurs when the powerMode is True
                                if (t_RuneData.powerMode)
                                {
                                    // Do the same thing
                                    // Store this position in TS
                                    int[] t_RunePos_TS = new int[3] {t_RuneData.pos[0] + t_RuneData.parent.trellisOffset[0], t_RuneData.pos[1] + t_RuneData.parent.trellisOffset[1], t_RuneData.pos[2] + t_RuneData.parent.trellisOffset[2]};

                                    // Check if it exists as a path in TS
                                    if (riftObj.pos_To_Trellis.ContainsKey(t_RunePos_TS))
                                    {
                                        // Grab any segment
                                        int t_t_Seg = riftObj.pos_To_Trellis[t_RunePos_TS].listOf_Segments[0];
                                        // Grab the island it belongs to.
                                        int t_t_Island = riftObj.listOf_Segments[t_t_Seg].island;

                                        // Check and see if both islands match
                                        if (t_Island == t_t_Island)
                                        {
                                            checkPair = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Finally, if a pairing was made, we are successful
                if (!checkPair)
                {
                    FaultyRune(r, false);
                    continue;
                }
            }
        
            ///////////
            // 8 - Gate
            else if (runeData.runeType == RuneType.Gate)
            {
                /////////////////
                // Gate - Rules
                // - This rune requires a bridge piece if the number of neighbours it has are even
                // - It cannot have a bridge piece if the number of neighbours it has are odd

                // Grab a list of Paths around the RuneData as a binary series
                // Where '1' represents a valid placement
                int[] tapa_Binary = new int[8];

                // Go through each row, x
                for (int i = runeData.pos[1] - 1; i <= runeData.pos[1] + 1; i ++)
                {
                    // And each column, y
                    for (int j = runeData.pos[2] - 1; j <= runeData.pos[2] + 1; j++)
                    {
                        // Make sure we're inside the grid
                        if (i >= 0 && i < riftObj.gsx && j >= 0 && j < riftObj.gsy)
                        {
                            // Ignore the centre square
                            if (!(i == runeData.pos[1] && j == runeData.pos[2]))
                            {
                                // If a Path exists here, we add it to the binary
                                if (arrayOf_GridElements[runeData.pos[0], i, j].isPath)
                                {
                                    // Add to the binary array
                                    tapa_Binary[RL_F.T_PS(new int[3] 
                                        {runeData.pos[0], i - runeData.pos[1] + 1, j - runeData.pos[2] + 1})] = 1;
                                }
                            }
                        }
                    }
                }

                // If the total number of paths around this rune are even, check to see if no bridge exists here
                if (tapa_Binary.Sum() % 2 == 0)
                {
                    // If a path does exist on this rune, this icon is wrong
                    if (arrayOf_GridElements[runeData.pos[0], runeData.pos[1], runeData.pos[2]].isPath)
                    {
                        FaultyRune(r, false);
                        continue;
                    }
                }
                // If the total number of paths around this rune are odd
                else
                {
                    // If a path doesn't exist on this rune, this icon is wrong
                    if (!arrayOf_GridElements[runeData.pos[0], runeData.pos[1], runeData.pos[2]].isPath)
                    {
                        FaultyRune(r, false);
                        continue;
                    }
                }
            }
        
            //////////
            // 9 - Box
            else if (runeData.runeType == RuneType.Box)
            {
                /////////////////
                // Box - Rules
                // - This rune can't have a path on it
                // - This rune requires a box to be place around it.
                // - The box can be rectangular or square, but it must have only four sides

                // First, check this rune contains no path
                if (arrayOf_GridElements[runeData.pos[0], runeData.pos[1], runeData.pos[2]].isPath)
                {
                    FaultyRune(r, false);
                    continue;
                }
                else
                {
                    
                    // Could extend in four directions to find distance of first path - This dictates the box size
                    // Then check that each path in the perimeter is present
                    // Then check that each path inside the perimeter is empty

                    // N, E, S, W box edges, if they are present
                    int[] boxVectors = new int[4] {-1, -1, -1, -1};
                    // Check to see if there are box edges so we can stop quickly if there aren't
                    bool hasBoxEdges = true;

                    // Starting at the Rune, check north, ignoring the rune itself
                    for (int j = runeData.pos[2]; j < riftObj.gsy; j++)
                    {
                        // We ignore the runepos itself
                        if (j != runeData.pos[2])
                        {
                            // The first time we find a path, break and mark it's position
                            if (arrayOf_GridElements[runeData.pos[0], runeData.pos[1], j].isPath)
                            {
                                boxVectors[0] = j;
                                break;
                            }

                            // If we're on the last item at the boxVectors hasn't been set, we mark this rune as failed
                            if (j + 1 == riftObj.gsy && boxVectors[0] == -1)
                            {
                                hasBoxEdges = false;
                            }
                        }
                    }

                    // Next, check East
                    for (int i = runeData.pos[1]; i < riftObj.gsx; i++)
                    {
                        // We ignore the runepos itself
                        if (i != runeData.pos[1])
                        {
                            // The first time we find a path, break and mark it's position
                            if (arrayOf_GridElements[runeData.pos[0], i, runeData.pos[2]].isPath)
                            {
                                boxVectors[1] = i;
                                break;
                            }

                            // If we're on the last item at the boxVectors hasn't been set, we mark this rune as failed
                            if (i + 1 == riftObj.gsx && boxVectors[1] == -1)
                            {
                                hasBoxEdges = false;
                            }
                        }
                    }

                    // Then, check South
                    for (int j = runeData.pos[2]; j >= 0; j--)
                    {
                        // We ignore the runepos itself
                        if (j != runeData.pos[2])
                        {
                            // The first time we find a path, break and mark it's position
                            if (arrayOf_GridElements[runeData.pos[0], runeData.pos[1], j].isPath)
                            {
                                boxVectors[2] = j;
                                break;
                            }

                            // If we're on the last item at the boxVectors hasn't been set, we mark this rune as failed
                            if (j == 0 && boxVectors[2] == -1)
                            {
                                hasBoxEdges = false;
                            }
                        }
                    }

                    // Finally, check West
                    for (int i = runeData.pos[1]; i >= 0; i--)
                    {
                        // We ignore the runepos itself
                        if (i != runeData.pos[1])
                        {
                            // The first time we find a path, break and mark it's position
                            if (arrayOf_GridElements[runeData.pos[0], i, runeData.pos[2]].isPath)
                            {
                                boxVectors[3] = i;
                                break;
                            }

                            // If we're on the last item at the boxVectors hasn't been set, we mark this rune as failed
                            if (i == 0 && boxVectors[3] == -1)
                            {
                                hasBoxEdges = false;
                            }
                        }
                    }
                
                    // If one of the box rune edges wasn't there, this rune has failed
                    if (!hasBoxEdges)
                    {
                        FaultyRune(r, false);
                        continue;
                    }
                    else
                    {
                        // Now we check the box space
                        bool hasBoxSpace = true;
                        for (int i = boxVectors[3]; i <= boxVectors[1]; i++)
                        {
                            for (int j = boxVectors[2]; j <= boxVectors[0]; j++)
                            {
                                // If i or j are equal to either of their component boxVector values, they must be paths
                                if (i == boxVectors[3] || i == boxVectors[1] || j == boxVectors[2] || j == boxVectors[0])
                                {
                                    if (!arrayOf_GridElements[runeData.pos[0], i, j].isPath)
                                    {
                                        hasBoxSpace = false;
                                    }
                                }
                                // For all other cases, the grid must be empty
                                else
                                {
                                    if (arrayOf_GridElements[runeData.pos[0], i, j].isPath)
                                    {
                                        hasBoxSpace = false;
                                    }
                                }
                            }
                        }

                        // Final check, if there is a box around this rune
                        if (!hasBoxSpace)
                        {
                            FaultyRune(r, false);
                            continue;
                        }
                    }
                }

            }
        
            /////////////
            // 10 - Exits
            else if (runeData.runeType == RuneType.Exits)
            {
                /////////////////
                // Exits - Rules
                // - This rune checks the number of cardinal paths connected to this piece
                // - The number of paths must match the connected value
                // - This rune must have a path

                // First, check this rune contains a path
                if (!arrayOf_GridElements[runeData.pos[0], runeData.pos[1], runeData.pos[2]].isPath)
                {
                    FaultyRune(r, false);
                    continue;
                }
                else
                {
                    // Total exits
                    int totalExits = 0;

                    // Check either side of the X direction
                    for (int i = runeData.pos[1] - 1; i <= runeData.pos[1] + 1; i ++)
                    {
                        // Make sure we're inside the grid
                        if (i >= 0 && i < riftObj.gsx)
                        {
                            // Ignore the centre square
                            if (i != runeData.pos[1])
                            {
                                // If a Path exists here, we add it to the total
                                if (arrayOf_GridElements[runeData.pos[0], i, runeData.pos[2]].isPath)
                                {
                                    totalExits += 1;
                                }
                            }
                        }
                    }

                    // Check either side of the Y direction
                    for (int j = runeData.pos[2] - 1; j <= runeData.pos[2] + 1; j ++)
                    {
                        // Make sure we're inside the grid
                        if (j >= 0 && j < riftObj.gsy)
                        {
                            // Ignore the centre square
                            if (j != runeData.pos[2])
                            {
                                // If a Path exists here, we add it to the total
                                if (arrayOf_GridElements[runeData.pos[0], runeData.pos[1], j].isPath)
                                {
                                    totalExits += 1;
                                }
                            }
                        }
                    }

                    // Finally, check the number of exists matches the numberType as set by the user
                    if (totalExits != runeData.numberType)
                    {
                        FaultyRune(r, false);
                        continue;
                    }
                }
            }
        
            //////////////
            // 11 - Halved
            else if (runeData.runeType == RuneType.Halved)
            {
                /////////////////
                // Halved - Rules
                // - Half the number of runes in this puzzle must be lit
                // - The other half must be empty

                // Just create a check here to confirm there are 'Halved' icons in this region
                has_RuneHalved = true;
            }
        
            //////////////
            // 12 - Mirror
            else if (runeData.runeType == RuneType.Mirror)
            {
                /////////////////
                // Mirror - Rules
                // - Checks either side of this icon (Depending on direction)
                // - The lit bridges on either side must match
                // - This rune also works when its placed at the edges of a board
                // - It does not matter if this rune is lit or not

                // We're going to keep track of the total on each side
                int total = 0;

                // Going from one side to the other (Left to right for now)
                for (int i = 0; i < C_GS(runeData.dirType); i ++)
                {
                    // Calcalate the distance from the mirror and keep it's positive or negative status
                    int distance = i - C_PS_1(runeData.pos, runeData.dirType);

                    if (distance < 0)
                    {
                        if (runeData.dirType == DirType.S || runeData.dirType == DirType.W || runeData.dirType == DirType.D)
                        {
                            // We need to reduce the distance by 1
                            distance += 1;
                        }
                    }
                    else if (distance > 0)
                    {
                        if (runeData.dirType == DirType.N || runeData.dirType == DirType.E || runeData.dirType == DirType.U)
                        {
                            // We need to reduce the distance by 1
                            distance -= 1;
                        }
                    }

                    // If there is a path here
                    int[] tempPos = C_PS(runeData.pos, i, runeData.dirType);
                    if (arrayOf_GridElements[tempPos[0], tempPos[1], tempPos[2]].isPath)
                    {
                        // For the case when the distance is zero, we need to chech the sign of this value
                        if (distance == 0)
                        {
                            if (i - C_PS_1(runeData.pos, runeData.dirType) == 0)
                            {
                                if (runeData.dirType == DirType.N || runeData.dirType == DirType.E || runeData.dirType == DirType.U)
                                {
                                    total += -1;
                                }
                                else if (runeData.dirType == DirType.S || runeData.dirType == DirType.W || runeData.dirType == DirType.D)
                                {
                                    total += 1;
                                }
                            }
                            else if (i - C_PS_1(runeData.pos, runeData.dirType) < 0)
                            {
                                if (runeData.dirType == DirType.S || runeData.dirType == DirType.W || runeData.dirType == DirType.D)
                                {
                                    total += -1;
                                }
                            }
                            else if (i - C_PS_1(runeData.pos, runeData.dirType) > 0)
                            {
                                if (runeData.dirType == DirType.N || runeData.dirType == DirType.E || runeData.dirType == DirType.U)
                                {
                                    total += 1;
                                }
                            }
                        }
                        else
                        {
                            total += (int)( Mathf.Sign(distance) * Mathf.Pow(2, Mathf.Abs(distance)) );
                        }
                    }
                }

                // If the total is not zero, the mirror has not held
                if (total != 0)
                {
                    FaultyRune(r, false);
                    continue;
                }
            }
        
            //////////////
            // 13 - Knight
            else if (runeData.runeType == RuneType.Knight)
            {
                /////////////////
                // Knight - Rules
                // - Only one path must exist a knights move from this square

                // Store the relative movements to check as a list of vector 3's
                // Go through and check those distances to see if they have paths

                int checkKnightsMove = 0;

                List<Vector3> listOfKnightsMoves = new List<Vector3>{   new Vector3(1f, 0f, 2f), new Vector3(-1f, 0f, 2f), new Vector3(-1f, 0f, -2f), new Vector3(1f, 0f, -2f),
                                                                        new Vector3(2f, 0f, 1f), new Vector3(-2f, 0f, 1f), new Vector3(-2f, 0f, -1f), new Vector3(2f, 0f, -1f)  };

                // Go through the list
                foreach (Vector3 temp_Vec3 in listOfKnightsMoves)
                {
                    // Create a new position to check
                    Vector3 checkPos = RL_F.IADFV3_V3(runeData.pos, temp_Vec3);
                    int[] CheckPosArr = new int[3] {(int)checkPos.y, (int)checkPos.x, (int)checkPos.z};

                    // Check its inside the grid
                    // Make sure we're inside the grid
                    // Remember, my grid positions are different from the Unity grid positions
                    if (CheckPosArr[1] >= 0 && CheckPosArr[1] < riftObj.gsx && CheckPosArr[2] >= 0 && CheckPosArr[2] < riftObj.gsy)
                    {
                        // Check for path
                        if (arrayOf_GridElements[CheckPosArr[0], CheckPosArr[1], CheckPosArr[2]].isPath)
                        {
                            checkKnightsMove += 1;
                        }
                    }
                }

                // If knights move check is anything but 1, this fails
                if (checkKnightsMove != 1)
                {
                    FaultyRune(r, false);
                    continue;
                }
            }
        
            ////////////////////
            // 14 - Arrow Single
            else if (runeData.runeType == RuneType.ArrowSingle)
            {
                ////////////////
                // Arrow - Rules
                // - Only one path in this direction (Exclusive)

                // False until proven true
                int int_ArrowSingle = 0;

                // Ignore this grid position
                // Follow the arrow
                // If there is a path, add to the int value
                if (A_ST(runeData.pos, runeData.dirType) < A_GS(runeData.pos, runeData.dirType))
                {
                    for (int i = A_ST(runeData.pos, runeData.dirType); i < A_GS(runeData.pos, runeData.dirType); i++)
                    {
                        int[] tPos = C_PS(runeData.pos, i, runeData.dirType);
                        if (arrayOf_GridElements[tPos[0], tPos[1], tPos[2]].isPath)
                        {
                            int_ArrowSingle += 1;
                        }
                    }
                }

                // If at the end of this check, the arrow is true, we are ok
                // If not, we store the incorrect rune
                if (int_ArrowSingle != 1)
                {
                    FaultyRune(r, false);
                    continue;
                }
            }
        
            // If any rune gets to here
            FaultyRune(r, true);
            continue;
        }

        // This I'll need to edit later, it's a bit of a dramatic change
        if (has_RuneHalved)
        {
            List<RuneData> listOf_FaultyRunes_Additional = Additional_RuneCheck(has_RuneHalved);

            foreach (RuneData tempRuneData in listOf_FaultyRunes_Additional)
            {
                FaultyRune(0, false);
                continue;
            }
        }
    }

    // Additional runeChecks that don't happen in the main rune loop can happen here
    static List<RuneData> Additional_RuneCheck(bool has_RuneHalved)
    {
        List<RuneData> listOf_FaultyRuneData_Additional = new List<RuneData>();
        
        //////////////
        // 11 - Halved
        // - Half the number of runes in this puzzle must be lit
        // - The other half must be empty

        if (has_RuneHalved)
        {
            // Save an integer value of Halved Runes as a total
            // Save an integer value of bridges that sit in those runes
            int total_HalvedRunes = 0;
            int total_HalvedRunes_IsPath = 0;

            foreach (RuneData runeData in riftObj.listOf_RuneData)
            {
                if (runeData.runeType == RuneType.Halved)
                {
                    total_HalvedRunes += 1;
                    if (arrayOf_GridElements[runeData.pos[0], runeData.pos[1], runeData.pos[2]].isPath)
                    {
                        total_HalvedRunes_IsPath += 1;
                    }
                }
            }

            // Check that half the number of Runes are paths
            if (2*total_HalvedRunes_IsPath != total_HalvedRunes)
            {
                // If not, we need to add every single halved rune to the list (I guess?)
                // Or maybe just the first one?
                foreach (RuneData runeData in riftObj.listOf_RuneData)
                {
                    if (runeData.runeType == RuneType.Halved)
                    {
                        listOf_FaultyRuneData_Additional.Add(runeData);
                    }
                }
            }
        }

        return listOf_FaultyRuneData_Additional;
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    // Pass the rune and whether or not is succeeded
    // Here, we change the state of the runeData to represent it's state and or change
    static void FaultyRune(int rIndex, bool isSucceed)
    {
        if (!isSucceed)
        {
            if (riftObj.listOf_RuneData[rIndex].runeState == RuneState.Fail || riftObj.listOf_RuneData[rIndex].runeState == RuneState.BecomeFail)
            {
                riftObj.listOf_RuneData[rIndex].runeState = RuneState.Fail;
            }
            else if (riftObj.listOf_RuneData[rIndex].runeState == RuneState.Succeed || riftObj.listOf_RuneData[rIndex].runeState == RuneState.BecomeSucceed)
            {
                riftObj.listOf_RuneData[rIndex].runeState = RuneState.BecomeFail;
            }
        }
        else
        {
            if (riftObj.listOf_RuneData[rIndex].runeState == RuneState.Fail || riftObj.listOf_RuneData[rIndex].runeState == RuneState.BecomeFail)
            {
                riftObj.listOf_RuneData[rIndex].runeState = RuneState.BecomeSucceed;
                Debug.Log("Become succeed");
            }
            else if (riftObj.listOf_RuneData[rIndex].runeState == RuneState.Succeed || riftObj.listOf_RuneData[rIndex].runeState == RuneState.BecomeSucceed)
            {
                riftObj.listOf_RuneData[rIndex].runeState = RuneState.Succeed;
                Debug.Log("Succeed");
            }
        }
    }

    //////////////////////
    // RULES FOR FLOORS //
    //////////////////////

    // Can't manipulate arrays as easily as I'd like in C#
    // Instead, pass already created array of gridElements
    // And just change individual elements
    public static void Return_ArrayOf_GridElement(ref GridElement[,,] arrayOf_GridElements, RiftObj pRiftObj, int[] newSpace_GS, int[] newSpace_Offset)
    {
        // Go through each bridgeData in that rift's array
        for (int ly = 0; ly < pRiftObj.gsly; ly ++)
        {
            for (int i = 0; i < pRiftObj.gsx; i ++)
            {
                for (int j = 0; j < pRiftObj.gsy; j ++)
                {
                    // Store our position in rift space
                    int[] curPos_RS = new int[3] {ly, i, j};
                    // And in user defined space (If this is null, this will be stored in rift space)
                    int[] curPos_OS = new int[3] {ly + newSpace_Offset[0], i + newSpace_Offset[1], j + newSpace_Offset[2]};

                    // For bridgeType
                    if (pRiftObj.arrayOf_BridgeData[ly, i, j].bridgeType != BridgeType.None)
                    {
                        // Store the bridge type
                        BridgeType bridgeType = pRiftObj.arrayOf_BridgeData[ly, i, j].bridgeType;

                        // For Static
                        if (bridgeType == BridgeType.Static)
                        {
                            // We store a floor in this position in Trellis Space, remember to add the offset
                            arrayOf_GridElements[curPos_OS[0], curPos_OS[1], curPos_OS[2]] = new GridElement(FloorType.Floor);
                        }
                        // For bridges
                        if (bridgeType == BridgeType.Bridge)
                        {
                            // We store a floor in this position in Trellis Space, remember to add the offset
                            arrayOf_GridElements[curPos_OS[0], curPos_OS[1], curPos_OS[2]] = new GridElement(FloorType.Floor);
                        }
                        // For Stairs
                        if (bridgeType == BridgeType.Stairs)
                        {
                            // We store a stairs in this position in Trellis Space, remember to add the offset
                            arrayOf_GridElements[curPos_OS[0], curPos_OS[1], curPos_OS[2]] = new GridElement(FloorType.Stairs);
                        }
                        // For Planks
                        if (bridgeType == BridgeType.Plank)
                        {
                            // We store a plank in this position in trellis space
                            // We also store the plank type for reference later
                            // I have a constructor that does most of this
                            arrayOf_GridElements[curPos_OS[0], curPos_OS[1], curPos_OS[2]] = new GridElement(pRiftObj.arrayOf_BridgeData[ly, i, j].plankDir);
                        }
                    }

                    // For Rocktype
                    // Check if there is a rock floor
                    // We check the same layer as the RockData array is shift
                    if (pRiftObj.arrayOf_RockData[ly, i, j].rockType == RockType.Solid)
                    {
                        // This acts as a floor
                        arrayOf_GridElements[curPos_OS[0], curPos_OS[1], curPos_OS[2]] = new GridElement(FloorType.Floor);
                    }

                    // Do this last
                    // Check if there is a rock in the way
                    // We check one layer up as the RockData array is shifted
                    if (pRiftObj.arrayOf_RockData[ly + 1, i, j].rockType == RockType.Solid)
                    {
                        // If there is a rock blocking our path, we cancel out any paths that might already be there
                        arrayOf_GridElements[curPos_OS[0], curPos_OS[1], curPos_OS[2]] = new GridElement();
                    }
                }
            }
        }
    }

    // Check the bridge and stairs componenets, but only using their component type
    static bool C_BS_T(BridgeType bridgeType)
    {
        if (bridgeType == BridgeType.Bridge || bridgeType == BridgeType.Stairs)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Given a direction and a start position
    // Loop through each square
    static int[] C_PS(int[] intArr, int j, DirType dirType)
    {
        if ((dirType == DirType.N) || (dirType == DirType.S))
        {
            return new int[3] {intArr[0], intArr[1], j};
        }
        else if ((dirType == DirType.E) || (dirType == DirType.W))
        {
            return new int[3] {intArr[0], j, intArr[2]};
        }
        else if ((dirType == DirType.U) || (dirType == DirType.D))
        {
            return new int[3] {j, intArr[1], intArr[2]};
        }
        else
        {
            return new int[3] {0, 0, 0};
        }
    }

    // Returns the array value based on which dirType is present
    static int C_PS_1(int[] intArr, DirType dirType)
    {
        if (dirType == DirType.N || dirType == DirType.S)
        {
            return intArr[2];
        }
        else if (dirType == DirType.E || dirType == DirType.W)
        {
            return intArr[1];
        }
        else if (dirType == DirType.U || dirType == DirType.D)
        {
            return intArr[0];
        }
        else
        {
            return 0;
        }
    }

    static int C_GS(DirType dirType)
    {
        if (dirType == DirType.N || dirType == DirType.S)
        {
            return riftObj.gsy;
        }
        else if (dirType == DirType.E || dirType == DirType.W)
        {
            return riftObj.gsx;
        }
        else if (dirType == DirType.U || dirType == DirType.D)
        {
            return riftObj.gsly;
        }
        else
        {
            return 0;
        }
    }
    
    // Used during a forloop for the arrow rule
    // Starts the for loop at our exclusive position direction
    // Or at 0
    static int A_ST(int[] intArr, DirType dirType)
    {
        if (dirType == DirType.N)
        {
            return (intArr[2] + 1);
        }
        else if (dirType == DirType.E)
        {
            return (intArr[1] + 1);
        }
        else if (dirType == DirType.U)
        {
            return (intArr[0] + 1);
        }
        else if ((dirType == DirType.W) || (dirType == DirType.S) || (dirType == DirType.D))
        {
            return 0;
        }
        else
        {
            return 0;
        }
    }

    // For arrow rule.
    // Find the bounds based on arrow direction
    // Either the grid size or the position we started checking
    static int A_GS(int[] inArr, DirType dirType)
    {
        if (dirType == DirType.N)
        {
            return riftObj.gsy;
        }
        else if (dirType == DirType.E)
        {
            return riftObj.gsx;
        }
        else if (dirType == DirType.S)
        {
            return inArr[2];
        }
        else if (dirType == DirType.W)
        {
            return inArr[1];
        }
        else if (dirType == DirType.U)
        {
            return riftObj.gsly;
        }
        else if (dirType == DirType.D)
        {
            return inArr[0];
        }
        else
        {
            return 0;
        }
    }

    // Return the opposite direction
    static DirType O_DT(DirType dirType)
    {
        if (dirType == DirType.N)
        {
            return DirType.S;
        }
        else if (dirType == DirType.S)
        {
            return DirType.N;
        }
        else if (dirType == DirType.E)
        {
            return DirType.W;
        }
        else if (dirType == DirType.W)
        {
            return DirType.E;
        }
        else if (dirType == DirType.U)
        {
            return DirType.D;
        }
        else if (dirType == DirType.D)
        {
            return DirType.U;
        }
        else
        {
            return DirType.None;
        }
    }
}
