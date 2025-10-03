using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score System")]
    private int score = 0;
    public TMP_Text scoreText;
    public int scorePerEnemy = 50;

    [Header("Enemy Tracking")]
    private int enemiesKilled = 0;

    [Header("Global Timer")]
    private float globalTimer = 0f;  //Timer global que no se reinicia
    private bool isRunning = false;

    [Header("Collectibles")]
    public int coins = 0;
    public int esmeraldas = 0;
    public int rubis = 0;

    public TMP_Text timerMinutes;
    public TMP_Text timerSeconds;
    public TMP_Text timerSeconds100;

    public float GlobalTime => globalTimer;
    public int EnemiesKilled => enemiesKilled;

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
        StartTimer();
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    private void Update()
    {
        if (isRunning)
        {
            globalTimer += Time.deltaTime;  //Aumenta el tiempo global
            MostrarTiempo(globalTimer);
        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void CargarSiguienteNivelPorNombre()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
            // No se reinicia nada, solo seguimos corriendo
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
        enemiesKilled = 0;
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    public void OnEnemyKilled(Enemy enemy)
    {
        if (enemy == null) return;

        enemiesKilled++;
        AddScore(scorePerEnemy);

        Debug.Log($"Enemigo eliminado! Total: {enemiesKilled}, Score: {score}");
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
        int minutes = Mathf.FloorToInt(globalTimer / 60);
        int seconds = Mathf.FloorToInt(globalTimer % 60);
        Debug.Log("El tiempo total fue de : " + minutes + " minutos " + seconds + " segundos");
        Debug.Log("El puntaje total fue de : " + score + " puntos");
        Debug.Log("Enemigos eliminados: " + enemiesKilled);
    }

    public void SetTimerUI(TMP_Text min, TMP_Text sec, TMP_Text sec100)
    {
        timerMinutes = min;
        timerSeconds = sec;
        timerSeconds100 = sec100;
        MostrarTiempo(globalTimer);  //Actualiza con el tiempo acumulado
    }

    public void SetScoreUI(TMP_Text scoreTextUI)
    {
        scoreText = scoreTextUI;
        scoreText.text = score.ToString();
    }
}
