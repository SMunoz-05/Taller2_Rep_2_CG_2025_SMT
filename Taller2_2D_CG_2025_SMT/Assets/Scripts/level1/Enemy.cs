using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float speed = 2f;
    private Transform targetPoint;
    private bool isDead = false;

    public bool IsDead => isDead;

    private void Start()
    {
        targetPoint = patrolPointB;
    }

    private void Update()
    {
        if (isDead) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            targetPoint = targetPoint == patrolPointA ? patrolPointB : patrolPointA;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        Die();
    }

    public void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.transform.CompareTag("Player"))
        {
            Player player = collision.transform.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
        }
    }
}