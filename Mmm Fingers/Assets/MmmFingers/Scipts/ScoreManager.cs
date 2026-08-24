using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";

    [Header("References")]
    [SerializeField] private GameSpeedController speedController;

    [Header("UI")]
    [Tooltip("Shown only during gameplay.")]
    [SerializeField] private TMP_Text gameplayScoreText;

    [Tooltip("Shown only on the Tap To Play panel.")]
    [SerializeField] private TMP_Text highScoreText;

    [Header("Score Settings")]
    [SerializeField] private float pointsPerWorldUnit = 1f;

    private float preciseScore;
    private int currentScore;
    private int highScore;

    private bool isScoring;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;

    private void Awake()
    {
        LoadHighScore();

        ResetRunScore();
        UpdateAllUI();
    }

    private void Update()
    {
        if (!isScoring)
        {
            return;
        }

        if (GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (speedController == null)
        {
            return;
        }

        IncreaseScore();
    }

    // --------------------------------------------------
    // RUN
    // --------------------------------------------------

    public void StartScoring()
    {
        ResetRunScore();

        isScoring = true;

        UpdateGameplayScoreUI();
    }

    public void StopScoring()
    {
        if (!isScoring)
        {
            return;
        }

        isScoring = false;

        CheckHighScore();

        UpdateHighScoreUI();
    }

    public void PrepareForWaiting()
    {
        ResetRunScore();

        UpdateGameplayScoreUI();
        UpdateHighScoreUI();
    }

    // --------------------------------------------------
    // SCORE
    // --------------------------------------------------

    private void IncreaseScore()
    {
        float distanceThisFrame =
            speedController.CurrentSpeed *
            Time.deltaTime;

        preciseScore +=
            distanceThisFrame *
            pointsPerWorldUnit;

        int newDisplayedScore =
            Mathf.FloorToInt(preciseScore);

        if (newDisplayedScore == currentScore)
        {
            return;
        }

        currentScore = newDisplayedScore;

        UpdateGameplayScoreUI();
    }

    private void ResetRunScore()
    {
        preciseScore = 0f;
        currentScore = 0;
        isScoring = false;
    }

    // --------------------------------------------------
    // HIGH SCORE
    // --------------------------------------------------

    private void CheckHighScore()
    {
        if (currentScore <= highScore)
        {
            return;
        }

        highScore = currentScore;

        PlayerPrefs.SetInt(
            HighScoreKey,
            highScore
        );

        PlayerPrefs.Save();
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(
            HighScoreKey,
            0
        );
    }

    // --------------------------------------------------
    // UI
    // --------------------------------------------------

    private void UpdateGameplayScoreUI()
    {
        if (gameplayScoreText != null)
        {
            gameplayScoreText.text =
                currentScore.ToString();
        }
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text =
                highScore.ToString();
        }
    }

    private void UpdateAllUI()
    {
        UpdateGameplayScoreUI();
        UpdateHighScoreUI();
    }

    // --------------------------------------------------
    // TESTING
    // --------------------------------------------------

    [ContextMenu("Delete High Score")]
    private void DeleteHighScore()
    {
        PlayerPrefs.DeleteKey(HighScoreKey);
        PlayerPrefs.Save();

        highScore = 0;

        UpdateHighScoreUI();
    }
}