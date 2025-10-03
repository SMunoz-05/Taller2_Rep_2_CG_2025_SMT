using UnityEngine;

public class LavaBubble : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float growSpeed = 2f;
    public float maxScale = 1.2f;
    public ParticleSystem explosion;

    [Header("Game Over Settings")]
    public GameOverManager gameOverManager; // referencia al script que maneja el panel

    private bool exploded = false;


    void Update()
    {
        if (!exploded)
        {
            // Hacer crecer la burbuja
            transform.localScale += Vector3.one * growSpeed * Time.deltaTime;

            // Si alcanza el tamaño máximo -> explota sola
            if (transform.localScale.x >= maxScale)
            {
                Explode();
            }
        }
    }


    void Explode()
    {
        exploded = true;
        // Instanciar partículas
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }


    // Detectar colisión con el jugador
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // asegúrate de que tu Player tenga el tag "Player"
        {
            if (gameOverManager != null)
            {
                gameOverManager.ShowGameOver();
            }
            Explode(); // la burbuja también explota al tocar al jugador
        }
    }
}
