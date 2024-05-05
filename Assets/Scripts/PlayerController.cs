using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 1;
    public float rotationSpeed;
    public List<AudioClip> clips;
    private AudioSource audioSrc;
    private Animator anim;
    private Rigidbody2D rb;
    private Stopwatch audioDelay = new Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();  
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        audioDelay.Start();
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ManagePlayerLookDirection();
        
        
    }

    void Update()
    {
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
                audioSrc.PlayOneShot(clips[Random.Range(0, clips.Count)]);
                audioDelay.Restart();
            }
            anim.SetBool("isWalking", true);
            Vector2 vVector = Vector2.up * vAxis;
            Vector2 hVector = Vector2.right * hAxis;
            rb.AddForce((vVector + hVector).normalized * speed * Time.deltaTime, ForceMode2D.Force);
            //  optional movement model
            //Vector3 vVector = Vector2.up * vAxis;
            //Vector3 hVector = Vector2.right * hAxis;
            //rb.AddForce((gameObject.transform.up * vAxis + hVector).normalized * speed, ForceMode2D.Force);

        }
        else
        {
            anim.SetBool("isWalking", false);
        }
    }

    private void ManagePlayerLookDirection()
    {
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, GetPlayerAngle() - 90)), Time.deltaTime * 5f);
    }


    private float GetPlayerAngle()
    {
        Vector2 mousePos = MyFunctions.GetMouseWorldPosition();
        Vector2 playerPos = transform.position;
        Vector2 directionVec = mousePos - playerPos;
        float angleInRadians = Mathf.Atan2(directionVec.y, directionVec.x);
        return angleInRadians * Mathf.Rad2Deg;
    }

 
}
