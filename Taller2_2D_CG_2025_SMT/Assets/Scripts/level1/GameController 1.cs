using TMPro;
using UnityEngine;

public class GameController1 : MonoBehaviour
{
    public TMP_Text minutos;
    public TMP_Text segundos;
    public TMP_Text milisegundos;
    public TMP_Text scoreText;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            // Una sola vez por escena, actualiza referencia del GameManager a UI local
            GameManager.Instance.SetTimerUI(minutos, segundos, milisegundos);
            GameManager.Instance.SetScoreUI(scoreText);
        }
    }
}

