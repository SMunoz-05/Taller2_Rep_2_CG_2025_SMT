using UnityEngine;
using System.Collections;

public class GroundEnemy : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    public ParticleSystem hitEffect;

    [Header("Combat Settings")]
    public float health = 1f; // Cambiado a 1 para que mueran de un golpe
    public int damage = 1;   // Cambiado a 1 para que hagan 1 de daño por golpe
    public float timeBetweenAttacks = 2f;
    public float attackRange = 10f; // Aumentado temporalmente para depuración

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float patrolWaitTime = 2f;
    public float arrivalDistance = 0.5f;

    // Private variables
    private Transform _player;
    private bool _alreadyAttacked;
    private bool _isDead;
    private bool _facingRight = true;
    private bool _movingToPointB = true;
    private bool _isWaiting;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null) {
            _player = playerGo.transform;
            Debug.Log("[Enemy] Jugador encontrado en Awake: " + _player.name);
        } else {
            Debug.LogWarning("[Enemy] No se encontró ningún GameObject con tag 'Player' en Awake");
        }

        rb.freezeRotation = true;
        rb.gravityScale = 1f;
    }

    private void Update()
    {
        Debug.Log("[Enemy] Update ejecutándose en: " + gameObject.name);
        if (_isDead) return;

        CheckForPlayerAttack();
        Patrol();
        UpdateAnimations();
    }

    private void CheckForPlayerAttack()
    {
        if (_player == null || _alreadyAttacked) {
            Debug.Log("[Enemy] _player es null o ya atacó");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        Debug.Log($"[Enemy] Distancia calculada en CheckForPlayerAttack: {distanceToPlayer}, attackRange: {attackRange}");
        if (distanceToPlayer <= attackRange)
        {
            Debug.Log("[Enemy] Jugador dentro de rango, llamando AttackPlayer");
            AttackPlayer();
        }
    }

    private void Patrol()
    {
        if (_isWaiting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (patrolPointA == null || patrolPointB == null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Vector2 targetPoint = _movingToPointB ? patrolPointB.position : patrolPointA.position;
        Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;

        // Move towards target
        rb.linearVelocity = new Vector2(direction.x * patrolSpeed, rb.linearVelocity.y);

        // Flip sprite based on movement direction
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            bool shouldFaceRight = direction.x > 0;
            if (shouldFaceRight != _facingRight)
                Flip();
        }

        // Check if reached target point
        float distanceToTarget = Vector2.Distance(transform.position, targetPoint);
        if (distanceToTarget <= arrivalDistance)
        {
            StartCoroutine(WaitAndSwitchDirection());
        }
    }

    private IEnumerator WaitAndSwitchDirection()
    {
        if (_isWaiting) yield break; // Prevent multiple coroutines

        _isWaiting = true;
        yield return new WaitForSeconds(patrolWaitTime);
        _movingToPointB = !_movingToPointB;
        _isWaiting = false;
    }

    private void AttackPlayer()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Face player
        Vector2 lookDir = (_player.position - transform.position).normalized;
        bool shouldFaceRight = lookDir.x > 0;
        if (shouldFaceRight != _facingRight)
            Flip();

        _alreadyAttacked = true;

        // Play attack animation
        if (animator != null)
            animator.SetTrigger("Attack");

        DealDamageToPlayer();
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void DealDamageToPlayer()
    {
        if (_player == null) {
            Debug.LogWarning("[Enemy] _player es null");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        Debug.Log($"[Enemy] Distancia al jugador: {distanceToPlayer}, attackRange: {attackRange}");
        if (distanceToPlayer <= attackRange)
        {
            Player playerScript = _player.GetComponent<Player>();
            if (playerScript == null)
                playerScript = _player.GetComponentInChildren<Player>();
            if (playerScript != null)
            {
                Debug.Log("[Enemy] Se encontró el script Player. Aplicando daño.");
                playerScript.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("[Enemy] No se encontró el script Player en el objeto con tag 'Player'.");
            }
        }
        else
        {
            Debug.Log("[Enemy] Jugador fuera de rango de ataque.");
        }
    }

    private void ResetAttack()
    {
        _alreadyAttacked = false;
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TakeDamage(float damageAmount)
    {
        if (_isDead) return;

        health -= damageAmount;

        if (hitEffect != null)
            hitEffect.Play();

        if (animator != null)
            animator.SetTrigger("Hit");

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        if (_isDead) return;

        _isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Notify game manager
        GameManager gameController = FindFirstObjectByType<GameManager>();
        if (gameController != null)
            gameController.OnEnemyKilled(this);

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (animator != null)
            animator.SetTrigger("Die");

        yield return new WaitForSeconds(1.5f);

        if (hitEffect != null)
        {
            ParticleSystem deathEffect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(deathEffect.gameObject, 2f);
        }

        Destroy(gameObject);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", speed);
    }

    private void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Patrol points
        if (patrolPointA != null && patrolPointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(patrolPointA.position, 0.3f);
            Gizmos.DrawSphere(patrolPointB.position, 0.3f);
            Gizmos.DrawLine(patrolPointA.position, patrolPointB.position);

            // Show current target
            Vector2 targetPoint = _movingToPointB ? patrolPointB.position : patrolPointA.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPoint);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isDead) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            Player playerScript = collision.gameObject.GetComponent<Player>();
            if (playerScript == null)
                playerScript = collision.gameObject.GetComponentInChildren<Player>();
            if (playerScript != null)
            {
                Debug.Log("[Enemy] Daño por contacto. Aplicando daño al jugador.");
                playerScript.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("[Enemy] No se encontró el script Player en el objeto colisionado.");
            }
        }
    }

    // Public properties
    public bool IsDead => _isDead;
    public float GetHealthPercentage() => Mathf.Clamp01(health / 100f);
}
