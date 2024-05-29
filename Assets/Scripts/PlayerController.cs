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
    public int playerHealth = 3;
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
            case 3:
                break;
            case 2:
                healthbar[2].SetActive(false); 
                break;
            case 1:
                healthbar[1].SetActive(false); 
                break;
            case 0:
                healthbar[0].SetActive(false);
                break;
            default:
                if (!SceneManager.GetSceneByName("GameOver").IsValid())
                {
                    Time.timeScale = 0f;
                    AudioSource[] audios = FindObjectsOfType<AudioSource>();
                    foreach (AudioSource audio in audios)
                    {
                        audio.Pause();
                    }
                    SceneManager.LoadScene("GameOver", LoadSceneMode.Additive);
                }
                break;

        }
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
        // sprawdza czy jest wciœniêty klawisz shift. Jeœli tak podwója szybkoœæ gracza i dekrementuje zmienn¹ playerStamina
        // w przypadku gdy playerStamina jest <= 0, spowalnia gracza na pewien czas
        ManagePLayerSprint();

        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        // jeœli jest wciœniêty przycisk poruszania siê, odtwórza dŸwiêk kroku, uruchamia animacjê gracza i dodaje do
        // obiektu gracza wektor si³y
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
        // jeœli nie jest wciœniêty przycisk poruszania siê, zatrzymuje animacjê oraz odtwarzanie dŸwiêku kroków
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
        
        // jeœli obiekt wchodz¹cy w kolizjê z graczem ma tag "enemy", gracz nie jest nietykalny
        // i przeciwnik powoduj¹cy kolizje nie znajduje siê w polu widzenia gracza
        if(collision.gameObject.CompareTag("enemy") && !Iframes && !enemy.isSeenByPlayer)
        {
            // uruchom efekt wizualny otrzymania obra¿eñ
            StartCoroutine(ScreenHurtEffect());

            // nadaj graczowi nietykalnoœæ na pewien czas
            StartCoroutine(PlayerIFrames());

            // odepchnij gracza od przeciwnika
            PushPlayerAway(collision.transform.position, 200f);

            // uruchom Event przekazuj¹c do niego tag dla zdarzenia i id przeciwnika odpowiadaj¹cy za uruchomienie funkcji
            // odtwarzaj¹cej audio w skrypcie przeciwnika
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
        int blinks = 20;
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
