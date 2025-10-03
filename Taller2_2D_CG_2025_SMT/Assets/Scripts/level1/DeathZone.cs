using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            GameOverManager gameOver = FindObjectOfType<GameOverManager>();
            if (gameOver != null)
            {
                gameOver.ShowGameOver();
            }
        }
    }
}

