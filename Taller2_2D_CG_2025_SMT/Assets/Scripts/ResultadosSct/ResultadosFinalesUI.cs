using UnityEngine;
using TMPro;

public class ResultadosFinalesUI : MonoBehaviour
{
    public TMP_Text tiempoTotalText;
    public TMP_Text scoreTotalText;

    void Start()
    {

        int score = GameManager.Instance.GetScore();
        scoreTotalText.text = score.ToString("D6");   


        float tiempoTotal = GameManager.Instance.TotalTimeUsed;
        int minutos = Mathf.FloorToInt(tiempoTotal / 60);
        int segundos = Mathf.FloorToInt(tiempoTotal % 60);
        int centesimas = Mathf.FloorToInt((tiempoTotal - (minutos * 60 + segundos)) * 100);

        tiempoTotalText.text = string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, centesimas);
    }
}
