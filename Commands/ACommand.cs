
// This abstract base command is used as a parent reference
// For difference inherited Command types

public abstract class ACommand
{
    public ActionKit actionKit;
    public DirType moveCard;
    public DirType faceCard;
    public float[] storePos;     // Store a position

    public abstract void Undo();
}
