using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Diagnostics;
using System.Data;
using Unity.VisualScripting;

public class EnemyController : MonoBehaviour
{

    private GameObject player;
    private Rigidbody2D rb;
    private Grid<PathNode> grid;
    private List<PathNode> path;
    private RaycastHit2D raycastHit;
    private Vector3 playerDirection;
    private Vector3 direction;
    private bool canSeePlayer = false;
    private Stopwatch pathCooldown = new Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("player");
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

        if (shouldFindPath())
        {
            path = FindPath();
        }
        DebugPath();
        CheckIfCanSeePlayer();
        MoveTowardsPlayer();
        ClearTraversedPath();

    }

    private bool shouldFindPath()
    {
        if (path == null && !canSeePlayer) return true;
        else if (!canSeePlayer && path.Count < 2) return true;
        else return false;
    }

    private void CheckIfCanSeePlayer()
    {
        playerDirection = (player.transform.position - transform.position).normalized;
        raycastHit = Physics2D.Raycast(transform.position, playerDirection);

        if (raycastHit.collider.tag == "Player")
        {
            canSeePlayer = true;
            path = null;
        }
        else
        {
            canSeePlayer = false;
        }
    }
    private void MoveTowardsPlayer()
    {
        if (canSeePlayer)
        {
            direction = (player.transform.position - transform.position);
        }
        else if (path != null && path.Count >= 2)
        {
            Vector3 vec3Node = Testing.pathfinding.NodeToVector3(path[1]);
            direction = (vec3Node - transform.position) + new Vector3(Testing.cellSize / 2, Testing.cellSize / 2, 0f);
        }
        else
        {
            direction = Vector3.zero;
        }

        UnityEngine.Debug.DrawLine(transform.position, transform.position + direction, Color.red);

        if (direction != Vector3.zero)
        {
            rb.AddForce(direction.normalized * Testing.enemySpeed, ForceMode2D.Force);
        }
    }
    private void DebugPath()
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                UnityEngine.Debug.DrawLine(new Vector3(path[i].x, path[i].y) * Testing.cellSize + Vector3.one * Testing.cellSize / 2,
                new Vector3(path[i + 1].x, path[i + 1].y) * Testing.cellSize + Vector3.one * Testing.cellSize / 2,
                Color.green);
            }
        }
    }
    private void ClearTraversedPath()
    {
        if(path != null && path.Count < 2)
        {
            path = null;
        }
        if (path != null && (Testing.pathfinding.NodeToVector3(path[1]) - transform.position).magnitude < 3.6f)
        {
            path.Remove(path[0]);
            path.Remove(path[0]);
        }
    }
    private List<PathNode> FindPath()
    {
        grid = Testing.pathfinding.GetGrid();
        int Px, Py;
        int x, y;
        if (Testing.isPlayerOnThePath)
        {
            grid.GetXY(player.transform.position, out Px, out Py);
        }
        else
        {
            grid.GetXY(Testing.playerClosestPathNodePosition, out Px, out Py);
        }

        if (!grid.GetGridObject(transform.position).isWalkable)
        {
            Vector3 nearestWalkableNodePos = Testing.pathfinding.getNearestWalkableNodePosition(gameObject);
            grid.GetXY(nearestWalkableNodePos, out x, out y);
        }
        else
        {
            grid.GetXY(transform.position, out x, out y);
        }
        List<PathNode> path = Testing.pathfinding.FindPath(x, y, Px, Py);
        return path;
    }
}
