using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

    public float speed = 5;
    private Rigidbody2D rb2D;

    private float move;

    public float jumpForce = 4;
    private bool isGrounded;
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    private Animator animator;


    public int coins;
    public int esmeralda;
    public TMP_Text TextCoins;
    public TMP_Text TextEsmeraldas;

    public AudioSource audioSource; 
    public AudioClip coinClip;
    public AudioClip barrelClip;
    public AudioClip esmeraldaClip;

    public int Coins => coins;
    public int Esmeraldas => esmeralda;

    public TMP_Text TextScore;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        move = Input.GetAxisRaw("Horizontal");
        rb2D.linearVelocity = new Vector2(move * speed, rb2D.linearVelocity.y);

        if(move != 0 )
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);
        //Si el movimiento es 0 se cambia la escala, la cual transforma la orientacion que mira el muñeco asignado. 

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
        }

        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("IsGrounded", isGrounded);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer); 
        // Lanza uan esfera que comprueba que esta colisionando con el floor de nuestra ui
    }
    private IEnumerator WaitAndRespawn()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Hit"));
        float hitAnimDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(hitAnimDuration);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void PlayerDamaged()
    {
        animator.SetTrigger("Hit");
        StartCoroutine(WaitAndRespawn());
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Coin"))
        {
            audioSource.PlayOneShot(coinClip);
            Destroy(collision.gameObject);
            coins++;
            TextCoins.text = coins.ToString();
            GameManager.Instance.AddScore(100);
            TextScore.text = GameManager.Instance.GetScore().ToString();
        }
        if (collision.transform.CompareTag("Esmeralda"))
        {
            audioSource.PlayOneShot(esmeraldaClip);
            Destroy(collision.gameObject);
            esmeralda++;
            TextEsmeraldas.text = esmeralda.ToString();
            GameManager.Instance.AddScore(200);
            TextScore.text = GameManager.Instance.GetScore().ToString();
        }

        if (collision.transform.CompareTag("Spikes"))
        {
            PlayerDamaged();
        }

        if (collision.transform.CompareTag("Barrel"))

        {
            audioSource.PlayOneShot(barrelClip);
            Vector2 knockbackDir = (rb2D.position - (Vector2)collision.transform.position).normalized;
            rb2D.linearVelocity = Vector2.zero;
            rb2D.AddForce(knockbackDir * 3, ForceMode2D.Impulse);

            BoxCollider2D[] collaiders = collision.gameObject.GetComponents<BoxCollider2D>();

            foreach (BoxCollider2D col in collaiders)
            {
                col.enabled = false;
            }

            collision.GetComponent<Animator>().enabled = true;
            Destroy(collision.gameObject, 0.5f);
        }

    }
}
