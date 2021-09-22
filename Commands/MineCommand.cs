using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineCommand : ACommand
{
    // The MineCommand operates very similarly to the Bridge and Plank commands
    // But also involves a reference to a rock piece
    
    // Rock Space
    public int[] rockData_ArrPos;
    public RiftObj riftObj;
    public RockData prv_RockData;

    public MineCommand(int[] pRockData_ArrPos, RiftObj pRiftObj, RockData pPrv_RockData)
    {
        // Rock Space
        rockData_ArrPos = pRockData_ArrPos;
        riftObj = pRiftObj;
        prv_RockData = pPrv_RockData;
    }

    public override void Undo()
    {
        // Use the rift unmine rock command
        riftObj.UnMine_RockObj(rockData_ArrPos);
    }
}
