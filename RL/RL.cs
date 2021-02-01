using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// These Reference Library values are used throughout
// The entire game project

// This is a generic library

// Collision Layers
public enum CollLayers {Rock = 10, Bridge = 15};

// Cardinal directions for NESW and Up and Down.
public enum MoveCard {None, N, E, S, W, U, D};
// After checking movement, return what type of movement the player is allowed to perform
public enum MoveType {None, Step, Bridge, Dolley, StairsUp, StairsDown, Mine};

// Rift types
public enum RiftType {Isles, Jungle, Cave, Water, Climb};

// Bridge type
public enum BridgeType {None, Bridge, Dolley, Stairs};
public enum DolleyType {A, B}
public enum StairType {Lower, Upper, Protected}

// Icon types
public enum IconType {None, Ignore, Arrow, Tapa, Chevron, Pin, Must};
public enum DirectionType {N, E, S, W};
public enum TapaType {  T1, T2, T3, T4, T5, T6, T7, T8, T11, T12, T13, T14, T15,
                        T22, T23, T24, T33, T111, T112, T113, T122, T1111}
public enum PinType {Start, End}

// Different animation modes for player movement
public enum AnimationMode {Idle, Walking, Running, Building, Blocked};

public static class RL
{
    public static Dictionary <MoveCard, Vector3> card_To_Vect;
    public static Dictionary <Vector3, MoveCard> vect_To_Card;

    public static void DeclareFunction()
    {
        // Create a dictionary for transferring movement cardninals into vector directions
        card_To_Vect = new Dictionary <MoveCard, Vector3>();
        card_To_Vect.Add(MoveCard.None, Vector3.zero);
        card_To_Vect.Add(MoveCard.N, Vector3.forward);
        card_To_Vect.Add(MoveCard.E, Vector3.right);
        card_To_Vect.Add(MoveCard.S, Vector3.back);
        card_To_Vect.Add(MoveCard.W, Vector3.left);
        card_To_Vect.Add(MoveCard.U, Vector3.up);
        card_To_Vect.Add(MoveCard.D, Vector3.down);

        // Create a dictionary for transferring vector3 directions into movement cardinals
        vect_To_Card = new Dictionary<Vector3, MoveCard>();
        vect_To_Card.Add(Vector3.zero, MoveCard.None);
        vect_To_Card.Add(Vector3.forward, MoveCard.N);
        vect_To_Card.Add(Vector3.right, MoveCard.E);
        vect_To_Card.Add(Vector3.back, MoveCard.S);
        vect_To_Card.Add(Vector3.left, MoveCard.W);
        vect_To_Card.Add(Vector3.up, MoveCard.U);
        vect_To_Card.Add(Vector3.down, MoveCard.D);
    }
}

////////////////////
// ACTION CLASSES //
////////////////////

// Action class
public class PlayerAction
{
    public Vector3 playerPos;
    public bool isBridge = false;

    public PlayerAction(Vector3 pPlayerPos, bool pIsBridge = false)
    {
        playerPos = pPlayerPos;
        isBridge = pIsBridge;
    }
}

////////////////////
// OBJECT CLASSES //
////////////////////

// Used by a grid slot to record what type of bridge is here
public class BridgeData
{
    public int ly;
    public int x;
    public int y;
    public RiftObj parent;
    public BridgeObj bridgeObj;
    public BridgeType bridgeType;
    public DolleyType dolleyType;
    public StairType stairType;
    
    // Empty bridge type
    public BridgeData(int pLy, int pX, int pY)
    {
        ly = pLy;
        x = pX;
        y = pY;
        bridgeType = BridgeType.None;
    }

    // User define bridge
    public BridgeData(int pLy, int pX, int pY, RiftObj pParent, BridgeObj pBridgeObj)
    {
        ly = pLy;
        x = pX;
        y = pY;
        parent = pParent;
        bridgeObj = pBridgeObj;

        // Icon types
        bridgeType = pBridgeObj.bridgeType;
        dolleyType = pBridgeObj.dolleyType;
        stairType = pBridgeObj.stairType;
    }
}

// Used by a grid slow to record what type of icon is here
public class IconData
{
    public int ly;
    public int x;
    public int y;
    public RiftObj parent;
    public IconObj iconObj;
    public IconType iconType;
    public DirectionType directionType;
    public TapaType tapaType;
    public PinType pinType;

    // Each grid position will store one bridge and one icon.
    // If there are no icons in this grid position, we mark is as 'Empty'
    public IconData(int pLy, int pX, int pY)
    {
        ly = pLy;
        x = pX;
        y = pY;
        iconType = IconType.None;
    }

    // User defined lot
    public IconData(int pLy, int pX, int pY, RiftObj pParent, IconObj pIconObj)
    {
        ly = pLy;
        x = pX;
        y = pY;
        parent = pParent;
        iconObj = pIconObj;

        // Icon types
        iconType = iconObj.iconType;
        tapaType = iconObj.tapaType;
        pinType = iconObj.pinType;
    }
}
