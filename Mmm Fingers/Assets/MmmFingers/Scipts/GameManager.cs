using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Waiting,
        Playing,
        Settings
    }

    [Header("Game References")]
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private FingerController fingerController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameSpeedController speedController;

    [Header("UI")]
    [SerializeField] private GameUIManager uiManager;

    public GameState CurrentState { get; private set; }
        = GameState.Waiting;

    public bool IsPlaying =>
        CurrentState == GameState.Playing;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ReturnToTapToPlay();
    }

    // --------------------------------------------------
    // WAITING / TAP TO PLAY
    // --------------------------------------------------

    public void ReturnToTapToPlay()
    {
        CleanupRun();

        CurrentState = GameState.Waiting;

        if (fingerController != null)
        {
            fingerController.DisableFinger();
        }

        if (scoreManager != null)
        {
            scoreManager.PrepareForWaiting();
        }

        if (uiManager != null)
        {
            uiManager.ShowTapToPlay();
        }
    }

    // --------------------------------------------------
    // START GAME
    // --------------------------------------------------

    public void StartRun()
    {
        if (CurrentState != GameState.Waiting)
        {
            return;
        }

        CurrentState = GameState.Playing;

        if (speedController != null)
        {
            speedController.ResetSpeed();
        }

        if (scoreManager != null)
        {
            scoreManager.StartScoring();
        }

        if (obstacleSpawner != null)
        {
            obstacleSpawner.StartSpawning();
        }

        if (uiManager != null)
        {
            uiManager.ShowGameplay();
        }
    }

    // --------------------------------------------------
    // GAME OVER
    // --------------------------------------------------

    public void GameOver()
    {
        if (CurrentState != GameState.Playing)
        {
            return;
        }

        AudioManager.Instance?.VibrateOnLose();

        if (obstacleSpawner != null)
        {
            obstacleSpawner.StopSpawning();
        }

        if (scoreManager != null)
        {
            // This checks and saves the high score.
            scoreManager.StopScoring();
        }

        if (fingerController != null)
        {
            fingerController.DisableFinger();
        }

        CleanupRun();

        CurrentState = GameState.Waiting;

        if (scoreManager != null)
        {
            scoreManager.PrepareForWaiting();
        }

        if (uiManager != null)
        {
            uiManager.ShowTapToPlay();
        }
    }

    // --------------------------------------------------
    // SETTINGS
    // --------------------------------------------------

    public void OpenSettings()
    {
        if (CurrentState != GameState.Waiting)
        {
            return;
        }

        CurrentState = GameState.Settings;

        if (fingerController != null)
        {
            fingerController.DisableFinger();
        }

        if (uiManager != null)
        {
            uiManager.ShowSettings();
        }
    }

    public void CloseSettings()
    {
        if (CurrentState != GameState.Settings)
        {
            return;
        }

        CurrentState = GameState.Waiting;

        if (uiManager != null)
        {
            uiManager.ShowTapToPlay();
        }
    }

    // --------------------------------------------------
    // CLEANUP
    // --------------------------------------------------

    private void CleanupRun()
    {
        if (obstacleSpawner != null)
        {
            obstacleSpawner.StopSpawning();
            obstacleSpawner.ClearObstacles();
        }

        if (speedController != null)
        {
            speedController.ResetSpeed();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}