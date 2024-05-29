using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float playerStamina;
    public int playerHealth = 2;
    private float finalSpeed;
    public List<AudioClip> clips;
    private AudioSource audioSrc;
    private Animator anim;
    private Rigidbody2D rb;
    private bool footStepPlaying = false;
    private Stopwatch sprintCooldown = new Stopwatch();
    private bool Iframes = false;
    public Slider staminaBar;
    public List<GameObject> healthbar;
    public Volume globalVolume;
    private bool delay = false;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        GameController.current.GameEvent += TakePLayerHealthPoint;
    }
    void LateUpdate()
    {
        ManagePlayerLookDirection();
    }
    void Update()
    {
        ManagePlayerMovement();
        MangagePlayerHealth();
    }

    private void MangagePlayerHealth()
    {
        switch (playerHealth)
        {
            case 2:
                break;
            case 1:
                healthbar[2].SetActive(false); 
                break;
            case 0:
                healthbar[1].SetActive(false); 
                break;
            default:
                healthbar[0].SetActive(false);
                if (!SceneManager.GetSceneByName("GameOver").IsValid())
                {
                    Time.timeScale = 0f;
                    if(!delay)
                    {
                        StartCoroutine(Delay());
                    }
                    SceneManager.LoadScene("GameOver", LoadSceneMode.Additive);
                }
                break;

        }
    }

    IEnumerator Delay()
    {
        delay = true;
        // WaitForSecondsRealtime instead of just waitForSeconds because previously using Time.timescale messes with waitforseconds
        yield return new WaitForSecondsRealtime(4f);
        AudioSource[] audios = FindObjectsOfType<AudioSource>();

        foreach (AudioSource audio in audios)
        {
            audio.Pause();
        }
        delay = false;
    }

    private void TakePLayerHealthPoint(object sender, GameController.EventArgs e)
    {
        if(e.eventName == "playerHurt")
        {
            playerHealth --;
            Mathf.Clamp(playerHealth, -1, 3);
        }
    }
    private void ManagePlayerMovement()
    {
        ManagePLayerSprint();

        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");
        if(hAxis != 0 || vAxis != 0)
        {
            if (!footStepPlaying)
            {
                StartCoroutine(PlayFootStep());
            }
            anim.SetBool("isWalking", true);
            Vector2 vVector = Vector2.up * vAxis;
            Vector2 hVector = Vector2.right * hAxis;
            
            rb.AddForce((vVector + hVector).normalized * finalSpeed * Time.deltaTime, ForceMode2D.Force);
        }
        else
        {
            anim.SetBool("isWalking", false);
            audioSrc.Stop();
        }
    }
    private void ManagePLayerSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !sprintCooldown.IsRunning)
        {
            finalSpeed = speed * 2;
            audioSrc.pitch = 1.3f;
            playerStamina -= Time.deltaTime * 15f;
        }
        else
        {
            playerStamina += Time.deltaTime * 5f;
            if (sprintCooldown.IsRunning)
            {
                finalSpeed = speed / 2;
                audioSrc.pitch = 1;
            }
            else
            {
                finalSpeed = speed;
                audioSrc.pitch = 1;
            }
        }

        playerStamina = Mathf.Clamp(playerStamina, 0, 100);
        staminaBar.value = playerStamina;

        if (playerStamina <= 0)
        {
            sprintCooldown.Start();
        }
        if (sprintCooldown.ElapsedMilliseconds >= 4000)
        {
            sprintCooldown.Stop();
            sprintCooldown.Reset();
        }
    }
    private void ManagePlayerLookDirection()
    {
        rb.angularVelocity = 0f;
        Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, GetPlayerAngle() - 90)); ; 
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 uangle = Mathf.Round(Input.GetAxis("Horizontal")) * -Vector3.right + Mathf.Round(Input.GetAxis("Vertical")) * Vector3.up;
            float signedangle = Vector3.SignedAngle(uangle, Vector3.up, Vector3.forward);
            rotation = Quaternion.Euler(0f,0f,signedangle);
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }
    private float GetPlayerAngle()
    {
        Vector2 mousePos = MyFunctions.GetMouseWorldPosition();
        Vector2 playerPos = transform.position;
        Vector2 directionVec = mousePos - playerPos;
        float angleInRadians = Mathf.Atan2(directionVec.y, directionVec.x);
        return angleInRadians * Mathf.Rad2Deg;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        
        if(collision.gameObject.CompareTag("enemy") && !Iframes && !enemy.isSeenByPlayer)
        {
            StartCoroutine(ScreenHurtEffect());
            StartCoroutine(PlayerIFrames());
            PushPlayerAway(collision.transform.position, 200f);
            GameController.current.FireEvent("playerHurt", enemy.id);
        }
    }
    private void PushPlayerAway(Vector3 pushDir, float force)
    {
        Vector3 dir = (transform.position - pushDir).normalized;
        rb.AddForce(dir * force, ForceMode2D.Impulse);
    }
    IEnumerator PlayFootStep()
    {
        footStepPlaying = true;
        yield return new WaitForSeconds(0.75f);
        audioSrc.clip = clips[Random.Range(0, clips.Count)];
        audioSrc.Play();
        footStepPlaying = false;
    }
    IEnumerator PlayerIFrames()
    {
        int blinks = 40;
        Iframes = true;
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        while (blinks >= 0)
        {
            yield return new WaitForSeconds(0.05f);
            renderer.enabled = !renderer.enabled;
            blinks--;
        }
        renderer.enabled = true;
        Iframes = false;
    }
    IEnumerator ScreenHurtEffect()
    {
        globalVolume.enabled = true;
        yield return new WaitForSeconds(0.1f);
        globalVolume.enabled = false;
    }

}
