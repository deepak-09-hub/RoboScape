using UnityEngine;

public class ObstacleDensityController : MonoBehaviour
{
    [Header("Density Settings")]

    [Tooltip("Distance between obstacles when the run begins.")]
    [SerializeField] private float startingSpacing = 7f;

    [Tooltip("Closest obstacles are allowed to become.")]
    [SerializeField] private float minimumSpacing = 4f;

    [Tooltip("How quickly the gap decreases per second.")]
    [SerializeField] private float spacingDecreasePerSecond = 0.02f;

    public float CurrentSpacing { get; private set; }

    private void Awake()
    {
        ResetDensity();
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying)
        {
            return;
        }

        CurrentSpacing = Mathf.MoveTowards(
            CurrentSpacing,
            minimumSpacing,
            spacingDecreasePerSecond * Time.deltaTime
        );
    }

    public void ResetDensity()
    {
        CurrentSpacing = startingSpacing;
    }
}