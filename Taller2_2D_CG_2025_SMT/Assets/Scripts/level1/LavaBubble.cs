using UnityEngine;

public class LavaBubble : MonoBehaviour
{
    public float growSpeed = 2f;
    public float maxScale = 1.2f;
    public ParticleSystem explosion;

    private bool exploded = false;

    void Update()
    {
        if (!exploded)
        {
            // Hacer crecer la burbuja
            transform.localScale += Vector3.one * growSpeed * Time.deltaTime;

            // Si alcanza el tamaño máximo -> explota
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
}