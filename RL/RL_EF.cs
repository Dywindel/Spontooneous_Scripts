using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Editor based functions that are referenced by multiple behaviours

public static class RL_EF
{
    public static void Setup_RiftData()
    {
        // Grab the PD
        PD pD = GameObject.FindObjectOfType<PD>();
        // Grab all the riftObjs
        RiftObj[] riftObjs = GameObject.FindObjectsOfType<RiftObj>();
        // Grav all the ZoneMarkers
        ZM[] zMs = GameObject.FindObjectsOfType<ZM>();

        // Set the list size for the PD
        pD.arrayOf_ZM = new ZM[Enum.GetValues( typeof( Biome ) ).Length];

        // Pass all the rifts to the PD
        pD.listOf_RiftObjs = riftObjs.ToList();

        // Go through each ZM
        foreach (ZM zM in zMs)
        {
            // This zone will add itself to the appropriate index inside the PD array
            pD.arrayOf_ZM[(int)zM.biome] = zM;
            // Reset the list of ZM
            zM.listOf_Rifts = new List<RiftObj>();
        }

        // Go through each rift
        foreach (RiftObj riftObj in riftObjs)
        {
            // Setup rift
            riftObj.Setup_RiftData_Editor();

            // Pass rift to correct ZM
            foreach (ZM zM in zMs)
            {
                if (zM.biome == riftObj.biome)
                {
                    zM.listOf_Rifts.Add(riftObj);
                }
            }
        }
    }

    // This function receives a list of neighbours as a binary and returns
    // A correct rotation and value and correct index value for loading the right mesh to display
    public static int[] Calculate_MeshIndex(List<int> bin_AsList, int pass_bin)
    {
        // This is value we plan to return
        int[] returnInt = new int[2] {0, 0};

        // First, let's invert all the numbers!
        List<int> bin_AsList_Inverted = MathList(bin_AsList, 1);

        // Let's do this by the size of each list
        // Size 0 v/
        if (bin_AsList.Count == 0)
        {
            // Update the mesh to our new shape
            returnInt = new int[2] {0, 0};
            return returnInt;
        }
        
        // Size 1 v/
        else if (bin_AsList.Count == 1)
        {
            // If even
            if (bin_AsList[0] % 2 == 0)
            {
                // It's a dead end
                returnInt = new int[2] {1, bin_AsList[0]};
                return returnInt;
            }
            // If odd, it's an island
            else
            {
                returnInt = new int[2] {0, 0};
                return returnInt;
            }
        }

        // Size 2 v/
        else if (bin_AsList.Count == 2)
        {
            // If both numbers are odd, It's an island
            if ((bin_AsList[0]*bin_AsList[1]) % 2 != 0)
            {
                returnInt = new int[2] {0, 0};
                return returnInt;
            }
            // If one number is even, it's a dead end
            else if ((bin_AsList[0] + bin_AsList[1]) % 2 != 0)
            {
                returnInt = new int[2] {1, MathDictionary(bin_AsList, 0)};
                return returnInt;
            }
            // If both numbers are even, it's a closed corner or a bridge
            else
            {
                // If the difference between both numbers is 4, it's a bridge
                if (Mathf.Abs(bin_AsList[0] - bin_AsList[1]) == 4)
                {
                    returnInt = new int[2] {3, bin_AsList[0]};
                    return returnInt;
                }
                // Otherwise, it's a closed corner
                else
                {
                    // Special case if there's a six involved
                    if (bin_AsList[1] == 6)
                    {
                        if (bin_AsList[0] == 0)
                        {
                            returnInt = new int[2] {2, bin_AsList[1]};
                            return returnInt;
                        }
                        else
                        {
                            returnInt = new int[2] {2, bin_AsList[0]};
                            return returnInt;
                        }
                    }
                    else
                    {
                        returnInt = new int[2] {2, bin_AsList[0]};
                        return returnInt;
                    }
                }
            }
        }

        // Size 3 v/
        else if (bin_AsList.Count == 3)
        {
            // If all numbers are even, it's a T-Piece
            if (MathCheck(bin_AsList, 0))
            {
                // To find the angle, we add all the numbers togeter, subtract 6 and invert them
                returnInt = new int[2] {5, ((12 - bin_AsList.AsQueryable().Sum()) + 2) % 8};
                return returnInt;
            }

            // If all three numbers are odd, it's a solitary island
            else if (MathCheck(bin_AsList, 1))
            {
                returnInt = new int[2] {0, 0};
                return returnInt;
            }
            // If one number is odd
            else if ((bin_AsList.AsQueryable().Sum() % 2) != 0)
            {
                // If these numbers are consequtive
                if (MathCheck(bin_AsList, 2))
                {
                    // Special case for 7
                    if (bin_AsList[2] == 7)
                    {
                        returnInt = new int[2] {4, bin_AsList[1]};
                        return returnInt;
                    }
                    else
                    {
                        returnInt = new int[2] {4, bin_AsList[0]};
                        return returnInt;
                    }
                }
                // Otherwise
                else
                {
                    // Grab the two even numbers and put the whole thing back into the second stage above
                    returnInt = Calculate_MeshIndex(MathList(bin_AsList, 0), BinaryReConversion(bin_AsList));
                    return returnInt;
                }
            }
            // If two numbers are odd
            else
            {
                // They're all deadends. Grab the one even number and apply the dead end to it
                returnInt = Calculate_MeshIndex(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
                return returnInt;
            }
        }

        // Size 4
        else if (bin_AsList.Count == 4)
        {
            // If all the numbers are even, its a crossroads
            if (MathCheck(bin_AsList, 0))
            {
                returnInt = new int[2] {8, 0};
                return returnInt;
            }

            // If they're all odd, it's solitary
            else if (MathCheck(bin_AsList, 1))
            {
                returnInt = new int[2] {0, 0};
                return returnInt;
            }

            // This list if specific to an edge and corner piece
            else if (new List<int>{23, 92, 113, 197}.Contains(pass_bin))
            {
                // invert, grab the even number, use that for rotation
                // + 2 % 8
                returnInt = new int[2] {6, ((MathList(bin_AsList_Inverted, 0)[0]) + 2) % 8};
                return returnInt;
            }

            // Same as before, but inverted
            else if (new List<int>{29, 116, 209, 71}.Contains(pass_bin))
            {
                // invert, grab the even number, use that for rotation
                // + 2 % 8
                returnInt = new int[2] {7, ((MathList(bin_AsList_Inverted, 0)[0]) + 2) % 8};
                return returnInt;
            }

            // If one of them is odd, I can pass back the three remaining even numbers
            else if (MathDictionary(bin_AsList, 1) == 3)
            {
                returnInt = Calculate_MeshIndex(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
                return returnInt;
            }

            // If two of them are odd
            // There could be a consequtive closed corner, or an open corner, or a bridge
            else if (MathDictionary(bin_AsList, 1) == 2)
            {
                // If consequtive numbers, it's a closed corner
                if (MathCheck(bin_AsList, 2))
                {
                    // For consequtive that starts with an odd numbers
                    if (new List<int>{10, 18}.Contains(bin_AsList.AsQueryable().Sum()))
                    {
                        // Special case
                        if (bin_AsList[0] == 0)
                        {
                            if (bin_AsList[1] == 1)
                            {
                                // Here we want the first three numbers
                                returnInt = Calculate_MeshIndex(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                                return returnInt;
                            }
                            else
                            {
                                // Engineer a new list
                                List<int> p_List = new List<int>{bin_AsList[0], bin_AsList[2], bin_AsList[3]};
                                returnInt = Calculate_MeshIndex(p_List, BinaryReConversion(p_List));
                                return returnInt;
                            }
                        }
                        else
                            // Here we want the last three numbers
                            returnInt = Calculate_MeshIndex(bin_AsList.Skip(1).Take(3).ToList(), BinaryReConversion(bin_AsList.Skip(1).Take(3).ToList()));
                            return returnInt;
                    }
                    // For consequtive that starts with even numbers
                    else
                    {
                        // Special case
                        if (bin_AsList[1] == 1)
                        {
                            if (bin_AsList[2] == 2)
                            {
                                // Here we want the first three numbers
                                returnInt = Calculate_MeshIndex(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                                return returnInt;
                            }
                            else
                            {
                                // We need to engineer a new list
                                List<int> p_List = new List<int>{bin_AsList[0], bin_AsList[2], bin_AsList[3]};
                                returnInt = Calculate_MeshIndex(p_List, BinaryReConversion(p_List));
                                return returnInt;
                            }
                        }
                        else
                        {
                            // Grab the first three numbers
                            returnInt = Calculate_MeshIndex(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                            return returnInt;
                        }
                    }
                }
                // If special case, it's also an open corner
                else if (new List<int>{39, 114, 156, 201}.Contains(pass_bin))
                {
                    // Special case
                    if (bin_AsList[0] == 0)
                    {
                        if (bin_AsList[1] == 1)
                        {
                            // Here we want the first three numbers
                            returnInt = Calculate_MeshIndex(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                            return returnInt;
                        }
                        else
                        {
                            // Engineer a new list
                            List<int> p_List = new List<int>{bin_AsList[0], bin_AsList[2], bin_AsList[3]};
                            returnInt = Calculate_MeshIndex(p_List, BinaryReConversion(p_List));
                            return returnInt;
                        }
                    }
                    else
                    {
                        if (bin_AsList[0] == 1)
                        {
                            // In this case, we want the last three numbers
                            returnInt = Calculate_MeshIndex(bin_AsList.Skip(1).Take(3).ToList(), BinaryReConversion(bin_AsList.Skip(1).Take(3).ToList()));
                            return returnInt;
                        }
                        else
                        {
                            // Here we want the first three numbers
                            returnInt = Calculate_MeshIndex(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                            return returnInt;
                        }
                    }
                }
                else
                {
                    // For most cases, I can just pass back the two even numbers
                    returnInt = Calculate_MeshIndex(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
                    return returnInt;
                }
            }
            // If three of them are odd, it's just a dead end
            else if (MathDictionary(bin_AsList, 1) == 1)
            {
                returnInt = Calculate_MeshIndex(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
                return returnInt;
            }
        }

        // Size 5
        else if (bin_AsList.Count == 5)
        {
            // If all the inverted numbers are even, it's a deadend
            if (MathCheck(bin_AsList_Inverted, 0))
            {
                // Method taken from size = 3 above
                returnInt = new int[2] {1, 12 - bin_AsList_Inverted.AsQueryable().Sum()};
                return returnInt;
            }
            // If all the inverted numbers are odd
            else if (MathCheck(bin_AsList_Inverted, 1))
            {
                // It's that weird shape, where we use the missing odd number as rotation value
                returnInt = new int[2] {10, (MathList(bin_AsList, 2)[0] + 7) % 8};
                return returnInt;
            }
            // If the list of numbers is consequtive
            else if (MathCheck(bin_AsList, 2))
            {
                // If there are more even numbers, it's the open edge shape
                if (MathDictionary(bin_AsList, 1) == 3)
                {
                    // To find the angle, I have to invert the numbers and find the single even value
                    // + 2 % 8
                    returnInt = new int[2] {9, ((MathList(bin_AsList_Inverted, 0)[0]) + 2) % 8};
                    return returnInt;
                }
                // If there are more odd numbers, we can send back the middle three numbers
                else
                {
                    // Send back the odd number, plus 4 and the integers either side
                    int passN = (MathList(bin_AsList_Inverted, 2)[0] + 4);
                    List<int> spliceList = new List<int>{(passN + 1) % 8, passN % 8, (passN - 1) % 8};
                    spliceList.Sort();
                    returnInt = Calculate_MeshIndex(spliceList, BinaryReConversion(spliceList));
                    return returnInt;
                }
            }

            // If no inverted number is consequtive
            else if (MathCheck(bin_AsList_Inverted, 3))
            {
                // We just pass back the even values
                returnInt = Calculate_MeshIndex(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
                return returnInt;
            }

            // If one of the inverted numbers is odd, the shape is either an open corner or a bridge
            else if ((bin_AsList_Inverted.AsQueryable().Sum() % 2) != 0)
            {
                // if the two even numbers are on opposite sides, send them back, it's a bridge
                List<int> splice_Even = MathList(bin_AsList, 0);
                if (splice_Even[1] - splice_Even[0] % 8 == 4)
                {
                    returnInt = Calculate_MeshIndex(splice_Even, BinaryReConversion(splice_Even));
                    return returnInt;
                }

                // Else, send back the two even numbers, plus the odd number between them
                else
                {
                    // special case
                    if (splice_Even[0] == 0 && splice_Even[1] == 6)
                    {
                        returnInt = Calculate_MeshIndex(new List<int>{0, 6, 7}, BinaryReConversion(new List<int>{0, 6, 7}));
                        return returnInt;
                    }
                    else
                    {
                        returnInt = Calculate_MeshIndex(new List<int>{splice_Even[0], splice_Even[0] + 1, splice_Even[0] + 2}, BinaryReConversion(new List<int>{splice_Even[0], splice_Even[0] + 1, splice_Even[0] + 2}));
                        return returnInt;
                    }
                }
            }

            // For the remaining cases
            else
            {
                // Grab the inverted odd numbers
                List<int> spliceList = MathList(bin_AsList_Inverted, 2);
                // The even number in the inverted slots are next to the odd number we want
                spliceList = MathList(bin_AsList_Inverted, 0);
                // Remove the odd number either side from the main list
                if (bin_AsList.Contains((spliceList[0] + 1) % 8))
                    bin_AsList.Remove((spliceList[0] + 1) % 8);
                else
                    bin_AsList.Remove((spliceList[0] + 7) % 8);
                // Then send back the new list
                returnInt = Calculate_MeshIndex(bin_AsList, BinaryReConversion(bin_AsList));
                return returnInt;
            }
        }

        // Size 6
        else if (bin_AsList.Count == 6)
        {
            // When the inverted numbers are both even
            if (MathCheck(bin_AsList_Inverted, 0))
            {
                // If they're opposite each other
                List<int> spliceList = MathList(bin_AsList_Inverted, 0);
                if ((spliceList[1] - spliceList[0]) == 4)
                {
                    //send back the two other even numbers
                    returnInt = Calculate_MeshIndex(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
                    return returnInt;
                }
                else
                {
                    // Special case
                    if ((spliceList[1] == 4) && (spliceList[0] == 2))
                    {
                        spliceList = new List<int>{0, 6, 7};
                        returnInt = Calculate_MeshIndex(spliceList, BinaryReConversion(spliceList));
                        return returnInt;
                    }
                    else
                    {
                        // Send back the two even numbers and the odd number between them
                        spliceList = MathList(bin_AsList, 0);
                        List<int> spliceList_2 = new List<int>{spliceList[0], spliceList[0] + 1, spliceList[1]};
                        returnInt = Calculate_MeshIndex(spliceList_2, BinaryReConversion(spliceList_2));
                        return returnInt;
                    }
                }
            }

            // If both inverted numbers are odd
            else if (MathCheck(bin_AsList_Inverted, 1))
            {
                // If the odd numbers are opposite each other
                List<int> spliceList = MathList(bin_AsList_Inverted, 2);
                if ((spliceList[1] - spliceList[0]) == 4)
                {
                    // This mesh is two opposing corners. We can grab the angle by using the lowest odd number
                    // And adding 1
                    returnInt = new int[2] {12, spliceList[0] + 1};
                    return returnInt;
                }
                else
                {
                    // Special Case
                    if ((spliceList[1] == 7) && (spliceList[0] == 1))
                    {
                        returnInt = new int[2] {11, 2};
                        return returnInt;
                    }
                    else
                    {
                        // This mesh is also a new shape that I can't describe
                        // Grab the angle by finding the midpoint between the two odd numbers
                        // + 2 % 8
                        returnInt = new int[2] {11, (((spliceList.AsQueryable().Sum())/2) + 2) % 8};
                        return returnInt;
                    }
                }
            }

            // Final Cases actually all overlap
            else
            {
                // Grab the inverted even number and remove the odd number next to it
                List<int> spliceList = MathList(bin_AsList_Inverted, 0);
                if (bin_AsList.Contains((spliceList[0] + 1) % 8))
                    bin_AsList.Remove((spliceList[0] + 1) % 8);
                if (bin_AsList.Contains((spliceList[0] + 7) % 8))
                    bin_AsList.Remove((spliceList[0] + 7) % 8);
                // Then send back those remaining numbers
                returnInt = Calculate_MeshIndex(bin_AsList, BinaryReConversion(bin_AsList));
                return returnInt;
            }
        }

        // Size 7
        else if (bin_AsList.Count == 7)
        {
            // If the single number in the inverted list is even
            if (bin_AsList_Inverted[0] % 2 == 0)
            {
                // Remove the two odd numbers beside it
                bin_AsList.Remove((bin_AsList_Inverted[0] + 1) % 8);
                bin_AsList.Remove((bin_AsList_Inverted[0] + 7) % 8);
                // Then send back those remaining numbers
                returnInt = Calculate_MeshIndex(bin_AsList, BinaryReConversion(bin_AsList));
                return returnInt;
            }
            // If the single number in the inverted list is odd, you can use the odd number - 3 (Which is + 9 % 8)
            // To get the orientation
            else
            {
                returnInt = new int[2] {13, (bin_AsList_Inverted[0] + 9) % 8};
                return returnInt;
            }
        }

        // Size 8
        else if (bin_AsList.Count == 8)
        {
            // There's only one shape possible
            returnInt = new int[2] {14, 0};
            return returnInt;
        }

        // In case of any mistakes
        returnInt = new int[2] {0, 0};
        return returnInt;
    }

    static int MathDictionary(List<int> listN, int caseN)
    {
        switch (caseN)
        {
            // Return the even number of two numbers
            case 0:
                if (listN[0] % 2 == 0)
                    return listN[0];
                else
                    return listN[1];
            // How many even numbers?
            case 1:
                int totalEven = 0;
                foreach(int j in listN)
                {  
                    if ((j % 2) == 0)
                        totalEven += 1;
                }
                return totalEven;
        }

        return -1;
    }

    static bool MathCheck(List<int> listN, int caseN)
    {
        switch (caseN)
        {
            // Check if all numbers are even
            case 0:
                foreach(int j in listN)
                {
                    if ((j % 2) != 0)
                        return false;
                }
                return true;
            // Check if all numbers are odd
            case 1:
                foreach(int j in listN)
                {
                    if ((j % 2) == 0)
                        return false;
                }
                return true;
            // If a list of numbers is consequtive (Including looping)
            case 2:
                for (int j = 0; j < 8; j++)
                {
                    // Create a list of consequtive numbers
                    List<int> spliceList = Enumerable.Range(j, listN.Count).ToList();
                    // Make sure % 8
                    List<int> spliceList_2 = spliceList.Select(i => (i % 8)).ToList();
                    // Sort them
                    spliceList_2.Sort();
                    // Check each number matches
                    int totalCheck = 0;
                    for (int k = 0; k < listN.Count; k++)
                    {
                        if (spliceList_2[k] == listN[k])
                            totalCheck += 1;
                    }
                    if (totalCheck == listN.Count)
                        return true;
                }
                // If none of these lists match, the numbers aren't consequtive
                return false;
            
            // If a list of nunmbers are not next to each other
            case 3:
                int checkN = 0;
                for (int j = 0; j < listN.Count(); j++)
                {
                    checkN = Mathf.Abs(listN[j] - listN[(j + 1) % listN.Count]);
                    if ((checkN == 1) || (checkN == 7))
                        return false;
                }
                return true;
        }

        return false;
    }

    // Splice a list from another list, based on parameters
    static List<int> MathList(List<int> listN, int caseN)
    {
        switch (caseN)
        {
            // Return a list of even numbers only
            case 0:
                List<int> pass_ListN = new List<int>();
                foreach (int j in listN)
                {
                    // If even, add it to list
                    if ((j % 2) == 0)
                        pass_ListN.Add(j);
                }
                // Then return the list
                return pass_ListN;
            // Invert list
            case 1:
                List<int> pass_ListN_2 = new List<int>{0, 1, 2, 3, 4, 5, 6, 7};
                foreach (int j in listN)
                    pass_ListN_2.Remove(j);
                return pass_ListN_2;
            // Return a list of odd numbers only
            case 2:
                List<int> pass_ListN_3 = new List<int>();
                foreach (int j in listN)
                {
                    // If odd, add it to list
                    if ((j % 2) != 0)
                        pass_ListN_3.Add(j);
                }
                // Then return the list
                return pass_ListN_3;
        }

        return new List<int>{0};
    }

    // Convert a number into a list of powers of 2
    public static List<int> BinaryConversion(int pass_bin)
    {
        List<int> bin_AsList = new List<int>();

        for (int i = 8 - 1; i >= 0; i--)
        {
            if (pass_bin >= Mathf.Pow(2, i))
            {
                bin_AsList.Add(i);
                pass_bin -= (int)Mathf.Pow(2, i);
            }
        }

        bin_AsList.Reverse();

        return bin_AsList;
    }

    // Convert a list in a binary number
    static int BinaryReConversion(List<int> pass_Bin_AsList)
    {
        int pass_bin = 0;

        foreach (int j in pass_Bin_AsList)
        {
            pass_bin += (int)Mathf.Pow(2, j);
        }

        return pass_bin;
    }
}
