using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpCommand : ACommand
{
    public Vector3 prv_PlayerPos;

    public WarpCommand(Vector3 p_Prv_PlayerPos)
    {
        prv_PlayerPos = p_Prv_PlayerPos;
    }

    // Update is called once per frame
    public override void Undo()
    {
        // Update player position
        Sc_Player.Instance.Update_PlayerPos_Instant(prv_PlayerPos, DirType.N);
    }
}
