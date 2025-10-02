using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5;
    public float jumpForce = 4;
    private Rigidbody2D rb2D;
    private float move;

    [Header("Ground Check")]
    private bool isGrounded;
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Health System")]
    public int maxHealth = 3;
    private int currentHealth;
    public TMP_Text TextHealth; // Añade este text en el HUD

    [Header("Components")]
    private Animator animator;

    [Header("Collectibles")]
    public int coins;
    public int esmeralda;
    public int rubi;
    public TMP_Text TextCoins;
    public TMP_Text TextEsmeraldas;
    public TMP_Text TextRubis;
    public TMP_Text TextScore;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip coinClip;
    public AudioClip esmeraldaClip;
    public AudioClip rubiClip;
    public AudioClip damageClip; // Añade un clip de daño

    [Header("Damage Settings")]
    public float invincibilityTime = 1.5f;
    private bool isInvincible = false;
    
    [Header("Combat")]
    public float attackRange = 1.5f;
    public int attackDamage = 25;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer = 1 << 6;
    private bool canAttack = true;

    // Public properties
    public int Coins => coins;
    public int Esmeraldas => esmeralda;
    public int Rubis => rubi;
    public int CurrentHealth => currentHealth;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Initialize health
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        move = Input.GetAxisRaw("Horizontal");
        rb2D.linearVelocity = new Vector2(move * speed, rb2D.linearVelocity.y);

        if (move != 0)
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
        }

        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("IsGrounded", isGrounded);
        
        if (Input.GetKeyDown(KeyCode.Z) && canAttack)
        {
            Attack();
        }
    }

    private void Attack()
    {
        canAttack = false;
    
        // Animación de ataque
        animator.SetTrigger("Attack");
    
        // Detectar enemigos en rango
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
    
        foreach (Collider2D enemy in enemiesInRange)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null && !enemyScript.IsDead)
            {
                enemyScript.TakeDamage(attackDamage);
            }
        }
    
        StartCoroutine(AttackCooldownCoroutine());
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    // MÉTODO PARA QUE EL ENEMIGO LLAME
    public void TakeDamage(int damage)
    {
        // No recibir daño si está en invencibilidad
        if (isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // No bajar de 0

        // Play damage sound
        if (audioSource != null && damageClip != null)
            audioSource.PlayOneShot(damageClip);

        // Update UI
        UpdateHealthUI();

        // Visual feedback
        animator.SetTrigger("Hit");

        // Check if dead
        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
        else
        {
            // Start invincibility frames
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        
        // Visual feedback - parpadeo
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            float elapsed = 0f;
            while (elapsed < invincibilityTime)
            {
                sprite.enabled = !sprite.enabled;
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            sprite.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityTime);
        }
        
        isInvincible = false;
    }

    private void UpdateHealthUI()
    {
        if (TextHealth != null)
        {
            TextHealth.text = $"{currentHealth}/{maxHealth}";
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // No exceder el máximo
        UpdateHealthUI();
    }

    private void PlayerDeath()
    {
        // Disable player controls
        enabled = false;
        
        StartCoroutine(WaitAndRespawn());
    }

    public void PlayerDamaged()
    {
        // Este método lo mantienes para compatibilidad con Spikes
        TakeDamage(1);
    }

    private IEnumerator WaitAndRespawn()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Hit"));
        float hitAnimDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(hitAnimDuration);
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
        if (collision.transform.CompareTag("Rubi"))
        {
            audioSource.PlayOneShot(rubiClip);
            Destroy(collision.gameObject);
            rubi++;
            TextRubis.text = rubi.ToString();
            GameManager.Instance.AddScore(500);
            TextScore.text = GameManager.Instance.GetScore().ToString();
        }

        if (collision.transform.CompareTag("Damage"))
        {
            PlayerDamaged();
        }


    }

    // Para debugging
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}