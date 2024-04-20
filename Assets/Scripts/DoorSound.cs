using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class DoorSound : MonoBehaviour
{
    private AudioSource audioComponent;
    private Rigidbody2D rb;
    private Stopwatch cooldown = new Stopwatch();
    public List<AudioClip> clips;
    void Start()
    {
        audioComponent = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
 
        if(collision.gameObject.CompareTag("Player"))
        {
            if(!audioComponent.isPlaying && Mathf.Abs(rb.angularVelocity) > 20f)
            {
                if(!cooldown.IsRunning || cooldown.ElapsedMilliseconds > 5000)
                {
                    audioComponent.PlayOneShot(clips[Random.Range(0, clips.Count)]);
                    cooldown.Restart();
                }     
            }
        }
    }
}
