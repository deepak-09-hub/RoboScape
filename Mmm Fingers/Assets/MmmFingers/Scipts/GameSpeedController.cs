using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float startingSpeed = 3f;

    [Tooltip("The speed will never exceed this value.")]
    [SerializeField] private float maximumSpeed = 5f;

    [Tooltip("How much speed is added every second.")]
    [SerializeField] private float speedIncreasePerSecond = 0.015f;

    public float CurrentSpeed { get; private set; }

    private void Awake()
    {
        ResetSpeed();
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying)
        {
            return;
        }

        CurrentSpeed = Mathf.MoveTowards(
            CurrentSpeed,
            maximumSpeed,
            speedIncreasePerSecond * Time.deltaTime
        );
    }

    public void ResetSpeed()
    {
        CurrentSpeed = startingSpeed;
    }

    public void SetSpeedSettings(
        float newStartingSpeed,
        float newMaximumSpeed,
        float newIncreasePerSecond)
    {
        startingSpeed = Mathf.Max(0.1f, newStartingSpeed);
        maximumSpeed = Mathf.Max(startingSpeed, newMaximumSpeed);
        speedIncreasePerSecond = Mathf.Max(0f, newIncreasePerSecond);

        ResetSpeed();
    }
}