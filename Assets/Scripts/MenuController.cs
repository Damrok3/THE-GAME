using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void ChangeScene()
    {
        if (SceneManager.GetSceneByName("MenuInGame").IsValid())
        {
            SceneManager.UnloadSceneAsync("MenuInGame");
            Time.timeScale = 1f;
            AudioSource[] audios = FindObjectsOfType<AudioSource>();
            foreach (AudioSource audio in audios)
            {

                audio.UnPause();
            }
        }
        else
        {
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
    }


    public void ExitGame()
    {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else

            Application.Quit();
        #endif

    }
}
