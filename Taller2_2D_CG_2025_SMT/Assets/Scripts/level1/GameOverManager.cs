using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // Panel oculto al inicio
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // 🔹 Reiniciar = cargar la primera escena (ej: nivel 1)
    public void RestartGame()
    {
        SceneManager.LoadScene("Level 1 Scene"); // Cambia "Scene1" por el nombre exacto de tu primera escena
    }

    // 🔹 Ir al menú principal
    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene"); // Cambia "Menu" por el nombre de tu escena del menú
    }
}


