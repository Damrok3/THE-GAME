using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 1;
    public List<AudioClip> clips;
    private AudioSource audio;
    private Animator anim;
    private Rigidbody2D rb;
    private Stopwatch audioDelay = new Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();  
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
        audioDelay.Start();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ManagePlayerLookDirection();
        ManagePlayerMovement();
    }

    private void ManagePlayerMovement()
    {
        
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");
        if(hAxis != 0 || vAxis != 0)
        {
            if(audioDelay.ElapsedMilliseconds > 750)
            {
                audio.PlayOneShot(clips[Random.Range(0, clips.Count)]);
                audioDelay.Restart();
            }
            anim.SetBool("isWalking", true);
            Vector2 vVector = Vector2.up * vAxis;
            Vector2 hVector = Vector2.right * hAxis;
            rb.AddForce((vVector + hVector).normalized * speed, ForceMode2D.Force);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
        
    }
    private void ManagePlayerLookDirection()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, GetPlayerAngle() - 90));
    }

    private float GetPlayerAngle()
    {
        Vector2 mousePos = GetMouseWorldPosition();
        Vector2 playerPos = transform.position;
        Vector2 directionVec = mousePos - playerPos;
        float angleInRadians = Mathf.Atan2(directionVec.y, directionVec.x);
        return angleInRadians * Mathf.Rad2Deg;
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = Camera.main.nearClipPlane + 1;
        Vector2 position = Camera.main.ScreenToWorldPoint(screenPosition);
        return position;
    }
}
