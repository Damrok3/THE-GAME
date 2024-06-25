
public class PathNode
{
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable;

    public PathNode cameFromNode;

    public PathNode(int x, int y)
    {
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

}
