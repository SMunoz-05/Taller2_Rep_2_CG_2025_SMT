using UnityEngine;
using TMPro;

public class GameUISetup : MonoBehaviour
{
    public TMP_Text minutos;
    public TMP_Text segundos;
    public TMP_Text milisegundos;
    public TMP_Text scoreText;

    void Start()
    {
        // Una sola vez por escena, actualiza referencia del GameManager a UI local
        GameManager.Instance.SetTimerUI(minutos, segundos, milisegundos);
        GameManager.Instance.SetScoreUI(scoreText);
    }
}
