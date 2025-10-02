using UnityEngine;
using UnityEngine.SceneManagement;

public class Cofre : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Verifica si fue el jugador
        {
            SceneManager.LoadScene(sceneToLoad);


        }
    }
}
