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
        keysCollected = 0;
    }
    private void Update()
    {
        HandleInput();
        UpdateKeyCount();
    }

    private void UpdateKeyCount()
    {
        keyScore.text = keysCollected.ToString() + "/4";
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

    }
    public static bool CheckIfObjectOnPath(GameObject obj)
    {
        return (pathfinding.GetGrid().GetGridObject(obj.transform.position).isWalkable != false);   
    }


}
