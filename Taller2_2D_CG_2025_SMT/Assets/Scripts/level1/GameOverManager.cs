using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject gameOverPanel; // arrastra el Panel GameOver aquí en el inspector

    // Mostrar Game Over
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Pausar el juego
        }
    }

    // Reiniciar juego (ejemplo de botón)
    public void RestartGame(string sceneName)
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    // Salir del juego (ejemplo de botón)
    public void QuitGame()
    {
        Application.Quit();
    }
}

