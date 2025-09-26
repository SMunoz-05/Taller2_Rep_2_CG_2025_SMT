using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    private int score = 0;
    public TMP_Text scoreText;


    public float totalTime = 120f; 
    private float timerTime;
    private bool isRunning = false;


    private float totalTimeUsed = 0f;

    public TMP_Text timerMinutes;
    public TMP_Text timerSeconds;
    public TMP_Text timerSeconds100;

    public float TotalTimeUsed => totalTimeUsed;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {

            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ResetTimer();
        MostrarTiempo(timerTime);
        StartTimer();
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    private void Update()
    {
        if (isRunning)
        {
            timerTime -= Time.deltaTime;
            if (timerTime <= 0f)
            {
                float timeUsedThisLevel = totalTime;
                totalTimeUsed += timeUsedThisLevel;
                timerTime = 0f;
                isRunning = false;
                MostrarTiempo(timerTime);
                Debug.Log("Se terminó el tiempo en nivel: " + SceneManager.GetActiveScene().name);
            }
            else
            {
                MostrarTiempo(timerTime);
            }
        }
    }

    public void ResetTimer()
    {
        timerTime = totalTime;
        MostrarTiempo(timerTime);
        isRunning = false;
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void CargarSiguienteNivelPorNombre()
    {
        float timeUsedThisLevel = totalTime - timerTime;
        totalTimeUsed += timeUsedThisLevel;

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {

            SceneManager.LoadScene(nextIndex);
            ResetTimer();
            StartTimer();
        }
        else
        {
            Debug.Log("No hay más escenas para cargar.");
            MostrarTotalesFinales();
        }
    }

    public void AddScore(int puntos)
    {
        score += puntos;
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    public int GetScore() => score;

    public void ResetScore()
    {
        score = 0;
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    private void MostrarTiempo(float tiempo)
    {
        if (timerMinutes == null || timerSeconds == null || timerSeconds100 == null) return;

        int minutesInt = Mathf.FloorToInt(tiempo / 60);
        int secondsInt = Mathf.FloorToInt(tiempo % 60);
        int seconds100Int = Mathf.FloorToInt((tiempo - (minutesInt * 60 + secondsInt)) * 100);

        timerMinutes.text = (minutesInt < 10) ? "0" + minutesInt : minutesInt.ToString();
        timerSeconds.text = (secondsInt < 10) ? "0" + secondsInt : secondsInt.ToString();
        timerSeconds100.text = (seconds100Int < 10) ? "0" + seconds100Int : seconds100Int.ToString();
    }

    public void MostrarTotalesFinales()
    {
        int minutes = Mathf.FloorToInt(totalTimeUsed / 60);
        int seconds = Mathf.FloorToInt(totalTimeUsed % 60);
        Debug.Log("El tiempo total fue de : " + minutes + " minutos " + seconds + " segundos");
        Debug.Log("El puntaje total fue de : " + score + " puntos");
    }

    public void SetTimerUI(TMP_Text min, TMP_Text sec, TMP_Text sec100)
    {
        timerMinutes = min;
        timerSeconds = sec;
        timerSeconds100 = sec100;
        MostrarTiempo(timerTime);
    }

    public void SetScoreUI(TMP_Text scoreTextUI)
    {
        scoreText = scoreTextUI;
        scoreText.text = score.ToString();
    }
}
