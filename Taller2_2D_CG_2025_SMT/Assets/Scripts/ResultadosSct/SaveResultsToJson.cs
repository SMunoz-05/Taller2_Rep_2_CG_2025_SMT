using UnityEngine;
using System.IO;

[System.Serializable]
public class ResultData
{

    public string descripcion = "Resultados de los niveles";
    public int ScoreTotal;
    public string TiempoTotal;

}

public class SaveResultsToJson : MonoBehaviour
{
    public void SaveResults(int score, float totalTime)
    {
        int minutos = Mathf.FloorToInt(totalTime / 60);
        int segundos = Mathf.FloorToInt(totalTime % 60);
        int centesimas = Mathf.FloorToInt((totalTime - (minutos * 60 + segundos)) * 100);
        string tiempo = string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, centesimas);

        ResultData data = new ResultData();
        data.ScoreTotal = score;
        data.TiempoTotal = tiempo;

        string jsonString = JsonUtility.ToJson(data, true);

        string folderPath = Application.streamingAssetsPath;

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "resultadosFinales.json");

        File.WriteAllText(filePath, jsonString);

        Debug.Log("Archivo JSON guardado correctamente en: " + filePath);
    }

    public void OnSaveButtonClick()
    {
        int score = GameManager.Instance.GetScore();
        float totalTime = GameManager.Instance.TotalTimeUsed;
        SaveResults(score, totalTime);
    }
}
