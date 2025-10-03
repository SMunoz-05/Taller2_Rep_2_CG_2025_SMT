using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); 
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    
    public void RestartGame()
    {
        SceneManager.LoadScene("Level 1 Scene"); 
    }

   
    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene"); 
    }
}


