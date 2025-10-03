using UnityEngine;
using System.Collections;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    public ParticleSystem hitEffect;

    [Header("Combat Settings")]
    public float health = 1f; // Cambiado a 1 para que muera de un golpe
    public int damage = 1;   // Cambiado a 1 para que haga 1 de daño por golpe
    public float timeBetweenAttacks = 2f; // Mantener para evitar insta-kill
    public float attackRange = 1.5f;
    public float sightRange = 8f;

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float walkPointRange = 5f;

    [Header("Layer Settings")]
    public LayerMask groundLayer;

    private Transform _player;
    private Vector2 _walkPoint;
    private bool _walkPointSet;
    private bool _alreadyAttacked;
    private bool _isDead;
    private bool _facingRight = true;
    private bool _isWaiting;

    private enum State { Patrolling, Chasing, Attacking, Dead }
    private State _state = State.Patrolling;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null)
            _player = playerGo.transform;

        rb.freezeRotation = true;
        rb.gravityScale = 0f;
    }

    void Update()
    {
        if (_isDead || _player == null) return;

        float dist = Vector2.Distance(transform.position, _player.position);
        
        // Update state based on distance
        if (dist <= attackRange)
            _state = State.Attacking;
        else if (dist <= sightRange)
            _state = State.Chasing;
        else
            _state = State.Patrolling;

        // Execute current state
        switch (_state)
        {
            case State.Patrolling: Patrol(); break;
            case State.Chasing: MoveTo(_player.position, chaseSpeed); break;
            case State.Attacking: AttackPlayer(); break;
        }

        UpdateAnimations();
    }

    void Patrol()
    {
        if (_isWaiting) 
        { 
            rb.linearVelocity = Vector2.zero; 
            return; 
        }

        if (!_walkPointSet) 
            SetRandomWalkPoint();

        if (_walkPointSet)
        {
            MoveTo(_walkPoint, patrolSpeed);
            float distance = Vector2.Distance(transform.position, _walkPoint);
            if (distance < 1.5f) // Mayor tolerancia
            {
                rb.linearVelocity = Vector2.zero; // Detener movimiento
                _walkPointSet = false;
                StartCoroutine(WaitPatrol());
            }
        }
    }

    void SetRandomWalkPoint()
    {
        int maxTries = 10;
        for (int i = 0; i < maxTries; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + new Vector2(
                Random.Range(-walkPointRange, walkPointRange),
                Random.Range(-walkPointRange / 2, walkPointRange / 2)
            );
            // Solo aceptar puntos que no colisionen con groundLayer
            if (!Physics2D.OverlapCircle(randomPoint, 0.3f, groundLayer))
            {
                _walkPoint = randomPoint;
                _walkPointSet = true;
                return;
            }
        }
        // Si no encuentra un punto libre, usar el último generado
        _walkPoint = (Vector2)transform.position + new Vector2(
            Random.Range(-walkPointRange, walkPointRange),
            Random.Range(-walkPointRange / 2, walkPointRange / 2)
        );
        _walkPointSet = true;
    }

    void MoveTo(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;

        // Flip sprite based on horizontal movement
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            bool shouldFaceRight = direction.x > 0;
            if (shouldFaceRight != _facingRight) 
                Flip();
        }
    }

    void AttackPlayer()
    {
        rb.linearVelocity = Vector2.zero;
        
        if (_alreadyAttacked || _player == null) return;

        // Face player
        Vector2 lookDirection = (_player.position - transform.position).normalized;
        bool shouldFaceRight = lookDirection.x > 0;
        if (shouldFaceRight != _facingRight) 
            Flip();

        _alreadyAttacked = true;
        
        if (animator != null)
            animator.SetTrigger("Attack");

        DealDamageToPlayer();
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    void DealDamageToPlayer()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distanceToPlayer <= attackRange)
        {
            Player playerScript = _player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }
        }
    }

    void ResetAttack() 
    { 
        _alreadyAttacked = false; 
    }

    void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale; 
        scale.x *= -1; 
        transform.localScale = scale;
    }

    IEnumerator WaitPatrol() 
    { 
        _isWaiting = true; 
        yield return new WaitForSeconds(Random.Range(1f, 3f)); 
        _isWaiting = false; 
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

    void Die()
    {
        if (_isDead) return;

        _isDead = true;
        _state = State.Dead;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        GameManager gameController = FindFirstObjectByType<GameManager>();
        if (gameController != null)
            gameController.OnEnemyKilled(this);

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
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

    void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    private void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Patrol range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);

        // Current walk point
        if (_walkPointSet)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_walkPoint, 0.2f);
            Gizmos.DrawLine(transform.position, _walkPoint);
        }
    }

    public bool IsDead => _isDead;
    public float GetHealthPercentage() => Mathf.Clamp01(health / 100f);
}