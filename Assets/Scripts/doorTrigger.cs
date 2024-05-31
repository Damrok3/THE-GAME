using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class doorTrigger : MonoBehaviour
{
    public GameObject doorDialogue;
    private GameObject player;
    private bool crON = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!crON)
        {
            StartCoroutine(PlayerText(5f));
            doorDialogue.SetActive(true);
        }
    }

    IEnumerator PlayerText(float time)
    {
        switch(GameController.keysCollected)
        {
            case 0:
                doorDialogue.GetComponent<TextMeshPro>().text = "I need to find all four keys in order to escape";
                break;
            case 1:
                doorDialogue.GetComponent<TextMeshPro>().text = "Three more to go, come on";
                break;
            case 2:
                doorDialogue.GetComponent<TextMeshPro>().text = "I've got two, halfway to escaping this";
                break;
            case 3:
                doorDialogue.GetComponent<TextMeshPro>().text = "Just one more";
                break;
            case 4:
                if (!SceneManager.GetSceneByName("Menu").IsValid())
                {
                    SceneManager.LoadScene("Menu", LoadSceneMode.Single);
                }
                break;

        }
        
        crON = true;
        while (time > 0)
        {
            yield return null;
            doorDialogue.transform.position = player.transform.position + Vector3.up * 10f;
            time -= Time.deltaTime;
        }
        doorDialogue.SetActive(false);
        crON = false;
    }

}
