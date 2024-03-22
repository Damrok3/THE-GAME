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
        var timer = Stopwatch.StartNew();

        if (!canSeePlayer && path == null || !canSeePlayer && path.Count < 2)
        {
            path = FindPath();
        }
        timer.Stop();
        if (path != null && (Testing.pathfinding.NodeToVector3(path[1]) - transform.position).magnitude < 3.6f)
        {
            path.Remove(path[0]);
            path.Remove(path[0]);
        }

        playerDirection = (player.transform.position - transform.position).normalized;

        raycastHit = Physics2D.Raycast(transform.position + playerDirection * 3, playerDirection);

        if (raycastHit.collider.tag == "Player")
        {
            canSeePlayer = true;
            path = null;
        }
        else
        {
            canSeePlayer = false;
            direction = Vector3.zero;
        }

        if (canSeePlayer)
        {
            direction = (player.transform.position - transform.position);
        }
        else if (path != null && path.Count >= 2)
        {
            Vector3 vec3Node = Testing.pathfinding.NodeToVector3(path[1]);
            direction = (vec3Node - transform.position) + new Vector3(Testing.cellSize / 2 , Testing.cellSize / 2, 0f);
        }

        if (canSeePlayer || path != null)
        {
            rb.AddForce(direction.normalized * Testing.enemySpeed, ForceMode2D.Force);
        }

        UnityEngine.Debug.DrawLine(transform.position, transform.position + direction, Color.red);

        if (path != null)
        {
            //UnityEngine.Debug.Log("path count: " + path.Count);

            for (int i = 0; i < path.Count - 1; i++)
            {
                UnityEngine.Debug.DrawLine(new Vector3(path[i].x, path[i].y) * Testing.cellSize + Vector3.one * Testing.cellSize / 2,
                new Vector3(path[i + 1].x, path[i + 1].y) * Testing.cellSize + Vector3.one * Testing.cellSize / 2,
                Color.green);
            }
        }


        
        UnityEngine.Debug.Log(timer.Elapsed);
    }

    private List<PathNode> FindPath()
    {
        grid = Testing.pathfinding.GetGrid();
        int Px, Py;
        if(Testing.isPlayerOnThePath)
        {
            grid.GetXY(player.transform.position, out Px, out Py);
        }
        else
        {
            grid.GetXY(Testing.playerClosestPathPosition, out Px, out Py);
        }
        grid.GetXY(transform.position, out int x, out int y);
        List<PathNode> path = Testing.pathfinding.FindPath(x, y, Px, Py);
        return path;
    }
}
