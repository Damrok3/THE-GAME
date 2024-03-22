using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private GameObject player; 
    [SerializeField] private HeatMapGenericVisual heatMap;
    public static float enemySpeed = 40;
    private GameObject [] boundaryList;

    public static Pathfinding pathfinding;
    public static float cellSize = 5f;
    public static Vector3 playerClosestPathPosition;
    public static bool isPlayerOnThePath = true;

    private void Start()
    {
        pathfinding = new Pathfinding(160, 100, cellSize);
        boundaryList = GameObject.FindGameObjectsWithTag("boundary");
        pathfinding.InitializePathBoundaries(boundaryList);
        heatMap.SetGrid(pathfinding.GetGrid());
        pathfinding.GetGrid().TriggerGridObjectChanged();
    }
    private void Update()
    {
        HandleInput();
        CheckIfPlayerOnPath();       
    }

    private void HandleInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            pathfinding.GetGrid().GetXY(player.transform.position, out int Px, out int Py);
            List<PathNode> path = pathfinding.FindPath(Px, Py, x, y);
            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(new Vector3(path[i].x, path[i].y) * cellSize + Vector3.one * cellSize / 2,
                                    new Vector3(path[i + 1].x, path[i + 1].y) * cellSize + Vector3.one * cellSize / 2,
                                    Color.red,
                                    2f);
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 position = GetMouseWorldPosition();
            PathNode node = pathfinding.GetGrid().GetGridObject(position);
            node.isWalkable = !node.isWalkable;
            //pathfinding.GetGrid().GetXY(position, out int x, out int y);
            pathfinding.GetGrid().TriggerGridObjectChanged();
        }
    }
    private Vector3 CheckIfPlayerOnPath()
    {
        foreach (GameObject boundary in boundaryList)
        {
            if (MyFunctions.IsInsideCollider(boundary.GetComponent<BoxCollider2D>(), player.transform.position))
            {
                isPlayerOnThePath = false;
                playerClosestPathPosition = boundary.GetComponent<BoxCollider2D>().bounds.ClosestPoint(player.transform.position);
                Vector3 awayFromColliderCenter = (playerClosestPathPosition - boundary.transform.position).normalized;
                while(!pathfinding.GetGrid().GetGridObject(playerClosestPathPosition).isWalkable)
                {
                    playerClosestPathPosition += awayFromColliderCenter;
                }
                return playerClosestPathPosition;
            }
        }
        isPlayerOnThePath = true;
        return Vector3.zero;
    }
    private Vector2 GetMouseWorldPosition()
    {
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = Camera.main.nearClipPlane + 1;
        Vector2 position = Camera.main.ScreenToWorldPoint(screenPosition);
        return position;
    }


}
