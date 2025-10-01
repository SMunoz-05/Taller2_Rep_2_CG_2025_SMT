using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.Flying;
    
    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    public ParticleSystem hitEffect;

    [Header("Combat Settings")]
    public float health = 100f;
    public int damage = 10;
    public float timeBetweenAttacks = 2f;
    public float attackRange = 1.5f;
    public float sightRange = 8f;

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float walkPointRange = 5f;
    public float checkInterval = 0.5f; // Aumentar a 0.5 segundos
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;
    public float minWalkPointDistance = 2f; // Nueva variable: distancia mínima entre puntos

    
    
    [Header("Layer Settings")]
    public LayerMask groundLayer;
    public LayerMask playerLayer = 1 << 7;

    [Header("Ground Patrol Settings")]
    public bool useFixedPatrol = true; // Para terrestres
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float patrolWaitTime = 2f;
    
    // Variables privadas
    private Vector2 currentPatrolTarget;
    private bool movingToPointB = true;
    private bool isPatrolWaiting = false;
    // Enemy types
    public enum EnemyType
    {
        Flying,      // Abeja - persigue al jugador, movimiento libre
        Ground,      // Enemigo terrestre - camina en el suelo
        Stationary   // Enemigo estático - no se mueve, solo ataca
    }

    // Private variables
    private Transform player;
    private Vector2 walkPoint;
    private bool walkPointSet;
    private bool alreadyAttacked;
    private bool takeDamage;
    private bool isDead;
    private float lastCheckTime;
    private bool facingRight = true;
    private bool isWaiting = false;
    private float lastStateChangeTime;
    private float stateChangeCooldown = 1f; // Cooldown entre cambios de estado

    // States
    private enum EnemyState { Patrolling, Chasing, Attacking, Dead }
    private EnemyState currentState = EnemyState.Patrolling;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure Player has 'Player' tag.");
        }

        SetupEnemyByType();
    }

    private void SetupEnemyByType()
    {
        if (rb == null) return;

        rb.freezeRotation = true;

        switch (enemyType)
        {
            case EnemyType.Flying:
                rb.gravityScale = 0f;
                break;
            case EnemyType.Ground:
                rb.gravityScale = 1f;
                break;
            case EnemyType.Stationary:
                rb.gravityScale = 1f;
                rb.freezeRotation = true;
                break;
        }
    }

    private void Update()
    {
        if (isDead || player == null || rb == null)
            return;

        if (Time.time - lastCheckTime > checkInterval)
        {
            UpdateEnemyState();
            lastCheckTime = Time.time;
        }

        ExecuteCurrentState();
    }

   private void UpdateEnemyState()
   {
       if (player == null) return;
   
       float distanceToPlayer = Vector2.Distance(transform.position, player.position);
       EnemyState newState = currentState;
   
       if (distanceToPlayer <= attackRange)
       {
           newState = EnemyState.Attacking;
       }
       else if (ShouldChasePlayer(distanceToPlayer))
       {
           newState = EnemyState.Chasing;
       }
       else
       {
           newState = EnemyState.Patrolling;
       }
   
       // Solo cambiar estado si ha pasado suficiente tiempo
       if (newState != currentState && Time.time - lastStateChangeTime > stateChangeCooldown)
       {
           currentState = newState;
           lastStateChangeTime = Time.time;
       }
   } 

    private bool ShouldChasePlayer(float distanceToPlayer)
    {
        switch (enemyType)
        {
            case EnemyType.Flying:
                return distanceToPlayer <= sightRange || takeDamage;
            case EnemyType.Ground:
                return false; // Enemigos terrestres no persiguen
            case EnemyType.Stationary:
                return false; // Enemigos estáticos no se mueven
            default:
                return false;
        }
    }

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patroling();
                break;
            case EnemyState.Chasing:
                ChasePlayer();
                break;
            case EnemyState.Attacking:
                AttackPlayer();
                break;
        }
    }

private void Patroling()
{
    if (enemyType == EnemyType.Stationary)
    {
        rb.linearVelocity = Vector2.zero;
        return;
    }

    if (isWaiting || isPatrolWaiting) return;

    if (!walkPointSet)
    {
        SearchWalkPoint();
    }

    if (walkPointSet)
    {
        float distanceToWalkPoint = Vector2.Distance(transform.position, walkPoint);

        if (distanceToWalkPoint < 1f)
        {
            walkPointSet = false;
            rb.linearVelocity = Vector2.zero;

            // Si es patrullaje fijo, cambiar de punto objetivo
            if (enemyType == EnemyType.Ground && useFixedPatrol)
            {
                StartCoroutine(PatrolWaitAndSwitch());
            }
            else
            {
                StartCoroutine(IdleBeforeNextPatrol());
            }
        }
        else
        {
            MoveTo(walkPoint, patrolSpeed);
        }
    }
    else
    {
        rb.linearVelocity = Vector2.zero;
    }
}
    private void SearchWalkPoint()
    {
        switch (enemyType)
        {
            case EnemyType.Flying:
                SearchFlyingWalkPoint();
                break;
            case EnemyType.Ground:
                SearchGroundWalkPoint();
                break;
            case EnemyType.Stationary:
                // No se mueve
                break;
        }
    }

    private void SearchFlyingWalkPoint()
    {
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        float randomY = Random.Range(-walkPointRange/2, walkPointRange/2);
        walkPoint = new Vector2(transform.position.x + randomX, transform.position.y + randomY);
        walkPointSet = true;
    }

private void SearchGroundWalkPoint()
{
    if (useFixedPatrol && patrolPointA != null && patrolPointB != null)
    {
        // Sistema de patrullaje entre dos puntos
        Vector2 targetPoint = movingToPointB ? patrolPointB.position : patrolPointA.position;
        
        // Verificar que el punto objetivo tenga suelo
        RaycastHit2D groundCheck = Physics2D.Raycast(targetPoint, Vector2.down, 2f, groundLayer);
        if (groundCheck.collider != null)
        {
            walkPoint = new Vector2(targetPoint.x, groundCheck.point.y + 0.5f);
            walkPointSet = true;
        }
        else
        {
            // Si no hay suelo, usar la posición del transform directamente
            walkPoint = targetPoint;
            walkPointSet = true;
        }
        return;
    }

    // Sistema original de puntos aleatorios (como respaldo)
    for (int i = 0; i < 20; i++)
    {
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        Vector2 candidate = new Vector2(transform.position.x + randomX, transform.position.y);

        float distanceFromCurrent = Vector2.Distance(transform.position, candidate);
        if (distanceFromCurrent < minWalkPointDistance)
            continue;

        RaycastHit2D hit = Physics2D.Raycast(candidate, Vector2.down, 2f, groundLayer);
        if (hit.collider != null)
        {
            walkPoint = new Vector2(candidate.x, hit.point.y + 0.5f);
            walkPointSet = true;
            return;
        }
    }

    // Fallback
    walkPointSet = false;
}
private void MoveTo(Vector2 target, float speed)
{
    Vector2 direction = (target - (Vector2)transform.position).normalized;

    switch (enemyType)
    {
        case EnemyType.Flying:
            rb.linearVelocity = direction * speed;
            break;
        case EnemyType.Ground:
            // Verificar obstáculos para terrestres
            if (CanMoveInDirection(direction.x))
            {
                rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
            }
            else
            {
                // Si hay obstáculo, intentar saltar (opcional)
                if (useFixedPatrol)
                {
                    AttemptJump();
                }
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            break;
        case EnemyType.Stationary:
            return;
    }

    // Flip sprite
    if (Mathf.Abs(direction.x) > 0.1f)
    {
        if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && facingRight)
        {
            Flip();
        }
    }
}

private bool CanMoveInDirection(float directionX)
{
    Vector2 rayOrigin = transform.position;
    Vector2 rayDirection = directionX > 0 ? Vector2.right : Vector2.left;
    float rayDistance = 0.8f;

    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, groundLayer);
    return hit.collider == null;
}

private void AttemptJump()
{
    // Salto simple para superar obstáculos pequeños
    if (Mathf.Abs(rb.linearVelocity.y) < 0.1f) // Solo saltar si está en el suelo
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 5f);
    }
}
    private void ChasePlayer()
    {
        // Solo enemigos voladores persiguen
        if (enemyType != EnemyType.Flying || player == null)
            return;

        MoveTo(player.position, chaseSpeed);

       
    }

    private IEnumerator IdleBeforeNextPatrol()
    {
        isWaiting = true;
        float waitTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void AttackPlayer()
    {
        rb.linearVelocity = Vector2.zero;

        if (!alreadyAttacked && player != null)
        {
            Vector2 lookDir = (player.position - transform.position).normalized;
            if ((lookDir.x > 0 && !facingRight) || (lookDir.x < 0 && facingRight))
            {
                Flip();
            }

            alreadyAttacked = true;

            if (animator != null)
                animator.SetBool("Attack", true);

            DealDamageToPlayer();

            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void DealDamageToPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
       // if (animator != null)
            //animator.SetBool("Attack", false);
    }

    public void TakeDamageFromProjectile(float damageAmount)
    {
        if (isDead) return;
        TakeDamage(damageAmount);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;

        if (hitEffect != null)
            hitEffect.Play();

        if (animator != null)
            animator.SetTrigger("Hit");

        StartCoroutine(TakeDamageCoroutine());

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator TakeDamageCoroutine()
    {
        takeDamage = true;
        yield return new WaitForSeconds(2f);
        takeDamage = false;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = EnemyState.Dead;

        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        GameManager gameController = FindObjectOfType<GameManager>();
        if (gameController != null)
        {
            gameController.OnEnemyKilled(this);
        }

        StartCoroutine(DeathSequence());
    }
    
    private IEnumerator PatrolWaitAndSwitch()
    {
        isPatrolWaiting = true;
        
        // Esperar en el punto
        yield return new WaitForSeconds(patrolWaitTime);
        
        // Cambiar objetivo
        movingToPointB = !movingToPointB;
        
        isPatrolWaiting = false;
    }

    private IEnumerator DeathSequence()
    {
        if (animator != null)
            animator.SetBool("Dead", true);

        yield return new WaitForSeconds(1.8f);

        if (hitEffect != null)
        {
            ParticleSystem deathEffect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(deathEffect.gameObject, 2f);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);

        if (walkPointSet)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(walkPoint, 0.3f);
            Gizmos.DrawLine(transform.position, walkPoint);
        }
    }

    public bool IsDead => isDead;
    public float GetHealthPercentage() => Mathf.Clamp01(health / 100f);
}