using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class FinalResultados : MonoBehaviour
{
    [Header("UI Panel Final")]
    public GameObject panel;
    public TMP_Text tiempoTotalText;
    public TMP_Text scoreTotalText;
    public Button saveButton;

    private void Start()
    {
        panel.SetActive(false); // Ocultar el panel al inicio
        saveButton.onClick.AddListener(SaveResultsToJSON);
    }

    // Este método lo llamas desde el cofre (OnTriggerEnter o similar)
    public void ShowPanel()
    {
        panel.SetActive(true);

        // Obtener datos desde el GameManager
        int score = GameManager.Instance.GetScore();
        float tiempoTotal = GameManager.Instance.GlobalTime;

        // Mostrar score
        scoreTotalText.text = "Puntaje: " + score.ToString("D6");

        // Convertir tiempo en mm:ss:cs
        int minutos = Mathf.FloorToInt(tiempoTotal / 60);
        int segundos = Mathf.FloorToInt(tiempoTotal % 60);
        int centesimas = Mathf.FloorToInt((tiempoTotal - (minutos * 60 + segundos)) * 100);
        tiempoTotalText.text = string.Format("Tiempo: {0:00}:{1:00}:{2:00}", minutos, segundos, centesimas);

        // Pausar el juego
        Time.timeScale = 0f;
    }

    public void SaveResultsToJSON()
    {
        ResultData data = new ResultData();

        data.ScoreTotal = GameManager.Instance.GetScore();
        float totalTime = GameManager.Instance.GlobalTime;

        int minutos = Mathf.FloorToInt(totalTime / 60);
        int segundos = Mathf.FloorToInt(totalTime % 60);
        int centesimas = Mathf.FloorToInt((totalTime - (minutos * 60 + segundos)) * 100);
        data.TiempoTotal = string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, centesimas);

        string json = JsonUtility.ToJson(data, true);

        string folderPath = Application.streamingAssetsPath;
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "resultadosFinales.json");
        File.WriteAllText(filePath, json);

        Debug.Log("Datos guardados en: " + filePath);
    }
}

[System.Serializable]
public class ResultData
{
    public string descripcion = "Resultados de los niveles";
    public int ScoreTotal;
    public string TiempoTotal;
}