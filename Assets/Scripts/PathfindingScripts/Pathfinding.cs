using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class Pathfinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private Grid<PathNode> grid;

    //list that is open for searching
    private List<PathNode> openList;

    //list that we already searched
    private List<PathNode> closedList;

    public Pathfinding(int width, int height, float cellSize)
    {
        grid = new Grid<PathNode>(width, height, cellSize, Vector3.zero, (Grid<PathNode> global, int x, int y) => new PathNode(grid, x, y));
    }

    public Grid<PathNode> GetGrid() { return grid; }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        if (!endNode.isWalkable) { return null; }

        openList = new List<PathNode>() { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                // Reached final Node
                return CalculatePath(endNode);
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.isWalkable)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if(tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }
        // Out of nodes on the open list
        return null;
    }

    public void InitializePathBoundaries(GameObject [] boundaryArray)
    {
        foreach (GameObject boundary in boundaryArray)
        {
            float width;
            float height;

            if (boundary.GetComponent<SpriteShapeController>() != null)
            {
                width = boundary.GetComponent<Collider2D>().bounds.size.x * 2;
                height = boundary.GetComponent<Collider2D>().bounds.size.y * 2;
            }
            else
            {
                width = boundary.GetComponent<Collider2D>().bounds.size.x;
                height = boundary.GetComponent<Collider2D>().bounds.size.y;
            }

            Vector3 bottomLeftCoord = boundary.transform.position - new Vector3(width/2, height/2);
            for (int x = 0; x < width/grid.GetCellSize() + 1; x++)
            {
                for(int y = 0; y < height/grid.GetCellSize() + 1; y++)
                {
                    Vector3 gridNodePosition = bottomLeftCoord + new Vector3 (grid.GetCellSize() * x, grid.GetCellSize() * y);
                    
                    PathNode node = grid.GetGridObject(gridNodePosition);
                    gridNodePosition = GameController.pathfinding.NodeToVector3(node);
                    //node bottom left corner
                    if (MyFunctions.IsInsideCollider(boundary, gridNodePosition))
                    {
                        node.isWalkable = false;
                    }
                    //bottom right
                    else if (MyFunctions.IsInsideCollider(boundary, gridNodePosition + new Vector3(grid.GetCellSize(), 0)))
                    {
                        node.isWalkable = false;
                    }
                    //upper left
                    else if (MyFunctions.IsInsideCollider(boundary, gridNodePosition + new Vector3(0, grid.GetCellSize())))
                    {
                        node.isWalkable = false;
                    }
                    //upper right
                    else if (MyFunctions.IsInsideCollider(boundary, gridNodePosition + new Vector3(grid.GetCellSize(), grid.GetCellSize())))
                    {
                        node.isWalkable = false;
                    }
                }
            }
        }
    }

    public Vector3 getNearestWalkableNodePosition(GameObject gameObject)
    {
        float smallestNoPathZoneDist = float.MaxValue;
        Vector3 playerPos = gameObject.transform.position;
        GameObject nearestNoPathZone = null;
        GameObject[] noPathZoneList = GameObject.FindGameObjectsWithTag("nopathzone");

        foreach (GameObject noPathZone in noPathZoneList)
        {
            Collider2D c = null;
            if (noPathZone.GetComponent<BoxCollider2D>() != null)
            {
                c = noPathZone.GetComponent<BoxCollider2D>();
            }
            else
            {
                c = noPathZone.GetComponent<EdgeCollider2D>();
            }
            if ((playerPos - (Vector3)c.ClosestPoint(playerPos)).magnitude < smallestNoPathZoneDist)
            {
                smallestNoPathZoneDist = (playerPos - (Vector3)c.ClosestPoint(playerPos)).magnitude;
                nearestNoPathZone = noPathZone;
            }
        }
        if (nearestNoPathZone != null)
        {
            Collider2D c = null;
            if (nearestNoPathZone.GetComponent<BoxCollider2D>() != null)
            {
                c = nearestNoPathZone.GetComponent<BoxCollider2D>();
            }
            else
            {
                c = nearestNoPathZone.GetComponent<EdgeCollider2D>();
            }

            Vector3 pointOnZoneSurface = c.ClosestPoint(playerPos);
            pointOnZoneSurface = new Vector3(pointOnZoneSurface.x, pointOnZoneSurface.y, 0);
            Vector3 awayFromColliderCenter;
            // if outside of collider, get the nearest point on the surface
            if (pointOnZoneSurface != playerPos)
            {
                awayFromColliderCenter = (playerPos - pointOnZoneSurface).normalized;
            }
            // if inside calculate the position of the closest edge and use that to set the point on surface
            else
            {
                //pain and suffering

                Vector3 zonePos = nearestNoPathZone.transform.position;
                float zoneSizeY = nearestNoPathZone.transform.localScale.y;
                float zoneSizeX = nearestNoPathZone.transform.localScale.x;

                Vector3 playerFromOtherEdge =  playerPos - (zonePos - nearestNoPathZone.transform.up * zoneSizeY / 2);  
                Vector3 playerFromOtherEdge2 =  playerPos - (zonePos + nearestNoPathZone.transform.up * zoneSizeY / 2);  
                Vector3 playerFromOtherEdge3 =  playerPos - (zonePos - nearestNoPathZone.transform.right * zoneSizeX / 2);  
                Vector3 playerFromOtherEdge4 =  playerPos - (zonePos + nearestNoPathZone.transform.right * zoneSizeX / 2);  
                Vector3 normal = nearestNoPathZone.transform.up;
                Vector3 normal2 = nearestNoPathZone.transform.right;
                Vector3 playerVectorProject = Vector3.Project(playerFromOtherEdge, normal);  
                Vector3 playerVectorProject2 = Vector3.Project(playerFromOtherEdge2, normal);  
                Vector3 playerVectorProject3 = Vector3.Project(playerFromOtherEdge3, normal2);  
                Vector3 playerVectorProject4 = Vector3.Project(playerFromOtherEdge4, normal2);  
                
                float dist = zoneSizeY - Vector3.Magnitude(playerVectorProject);
                float dist2 = zoneSizeY - Vector3.Magnitude(playerVectorProject2);
                float dist3 = zoneSizeX - Vector3.Magnitude(playerVectorProject3);
                float dist4 = zoneSizeX - Vector3.Magnitude(playerVectorProject4);

                List<Vector3> edges = new List<Vector3>()
                {
                     new Vector3(playerPos.x, playerPos.y) +  nearestNoPathZone.transform.up * dist,
                     new Vector3(playerPos.x, playerPos.y) - nearestNoPathZone.transform.up * dist2,
                     new Vector3(playerPos.x, playerPos.y) + nearestNoPathZone.transform.right * dist3,
                     new Vector3(playerPos.x, playerPos.y) - nearestNoPathZone.transform.right * dist4
                };
                


                float surfaceDist = float.MaxValue;
                foreach (Vector3 edge in edges)
                {
                    
                    if ((playerPos - edge).magnitude < surfaceDist)
                    {
                        pointOnZoneSurface = edge;
                        surfaceDist = (playerPos - edge).magnitude;
                    }
                }

                awayFromColliderCenter = (pointOnZoneSurface - playerPos).normalized;
            }
            Debug.DrawLine(playerPos, pointOnZoneSurface, Color.blue);
            while (!GetGrid().GetGridObject(playerPos).isWalkable)
            {
                playerPos += awayFromColliderCenter;
            }
            return playerPos;
        }
        else
        {
            return Vector3.zero;
        }

    }

    public Vector3 NodeToVector3(PathNode node)
    {
        float gridCellSize = grid.GetCellSize();
        return new Vector3(node.x * gridCellSize, node.y * gridCellSize);
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        if (currentNode.x - 1 >= 0)
        {
            // Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            // Left Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            // Left Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        if (currentNode.x + 1 < grid.GetWidth())
        {
            // Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            // Right Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            // Right Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));
        }
        // Down
        if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        // Up
        if(currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1 ));

        return neighbourList;
    }

    private PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode> ();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        if (b != null)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }
        else
        {
            return 0;
        }
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
