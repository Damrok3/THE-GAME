using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    public List<AudioClip> audio;
    private AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GameController.current.GameEvent += audioStart;
    }

    private void audioStart(object sender, GameController.EventArgs e)
    {
        switch(e.eventName)
        {
            case "keyPicked":
                audioSource.PlayOneShot(audio[0]);
                break;
            default:
                break;
        }
    }
}
