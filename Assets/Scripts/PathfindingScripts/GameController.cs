using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject player; 
    [SerializeField] private HeatMapGenericVisual heatMap;
    private GameObject [] noPathZoneList;

    // reference to the GameController class letting me to access the events outside of this script
    public static GameController current;
    public static Pathfinding pathfinding;
    public static float cellSize = 5f;
    public static Vector3 playerClosestPathNodePosition;
    public static int keysCollected = 0;

    public float timeScale = 1f;

    public TextMeshProUGUI keyScore;

    //.net standard for declaring an event handler that can take class object and store its data as well as trigger certain things
    public event EventHandler<EventArgs> GameEvent;

    //declaration of a class that objects of can be passed inside of the event
    public class EventArgs : System.EventArgs
    {
        public int x;
        public int y;
        public string eventName;
        public int id;
    }

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        //300 269
        pathfinding = new Pathfinding(300, 269, cellSize);
        noPathZoneList = GameObject.FindGameObjectsWithTag("nopathzone");
        pathfinding.InitializePathBoundaries(noPathZoneList);
        //TestPathfinding(pathfinding.GetGrid());
        heatMap.SetGrid(pathfinding.GetGrid());
        GameEvent?.Invoke(this, new EventArgs { });
    }
    private void Update()
    {
        Time.timeScale = timeScale;
        HandleInput();
        UpdateKeyCount();
    }

    private void UpdateKeyCount()
    {
        keyScore.text = keysCollected.ToString();
    }

    public void FireEvent(string name)
    {
        GameEvent?.Invoke(this, new EventArgs { eventName = name});
    }

    public void FireEvent(string name, int ID)
    {
        GameEvent?.Invoke(this, new EventArgs { eventName = name, id = ID });
    }

    private void TestPathfinding(Grid<PathNode> grid)
    {
        GameObject testObj = GameObject.Find("testobj");
        for(int i = 0; i < grid.GetWidth(); i++)
        {
            for(int j = 0; j < grid.GetHeight(); j++)
            {
                if(!grid.GetGridObject(i, j).isWalkable)
                {
                    Instantiate(testObj, grid.GetWorldPosition(i, j) + new Vector3(grid.GetCellSize() / 2, grid.GetCellSize() / 2), testObj.transform.rotation);
                }
            }
        }
    }

    private void HandleInput()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!SceneManager.GetSceneByName("MenuInGame").IsValid())
            {
                AudioSource[] audios = FindObjectsOfType<AudioSource>();
                foreach(AudioSource audio in audios)
                {
                    Debug.Log(audio.ToString());
                    audio.Pause();
                }
                Time.timeScale = 0f;
                SceneManager.LoadScene("MenuInGame", LoadSceneMode.Additive);
            }
            else
            {
                AudioSource[] audios = FindObjectsOfType<AudioSource>();
                foreach (AudioSource audio in audios)
                {
                   
                    audio.UnPause();
                }
                Time.timeScale = 1f;
                SceneManager.UnloadSceneAsync("MenuInGame");
            }
        }
        //if (Input.GetMouseButton(0))
        //{
        //    Vector3 mouseWorldPosition = GetMouseWorldPosition();
        //    pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
        //    pathfinding.GetGrid().GetXY(player.transform.position, out int Px, out int Py);
        //    List<PathNode> path = pathfinding.FindPath(Px, Py, x, y);
        //    if (path != null)
        //    {
        //        for (int i = 0; i < path.Count - 1; i++)
        //        {
        //            Debug.DrawLine(new Vector3(path[i].x, path[i].y) * cellSize + Vector3.one * cellSize / 2,
        //                            new Vector3(path[i + 1].x, path[i + 1].y) * cellSize + Vector3.one * cellSize / 2,
        //                            Color.red,
        //                            2f);
        //        }
        //    }
        //}

        //if (Input.GetMouseButtonDown(1))
        //{
        //    Vector3 position = GetMouseWorldPosition();
        //    PathNode node = pathfinding.GetGrid().GetGridObject(position);
        //    node.isWalkable = !node.isWalkable;
        //    //pathfinding.GetGrid().GetXY(position, out int x, out int y);
        //    GameEvent?.Invoke(this, new EventArgs { });
        //}

    }
    public static bool CheckIfObjectOnPath(GameObject obj)
    {
        return (pathfinding.GetGrid().GetGridObject(obj.transform.position).isWalkable != false);   
    }
    private Vector2 GetMouseWorldPosition()
    {
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = Camera.main.nearClipPlane + 1;
        Vector2 position = Camera.main.ScreenToWorldPoint(screenPosition);
        return position;
    }


}
