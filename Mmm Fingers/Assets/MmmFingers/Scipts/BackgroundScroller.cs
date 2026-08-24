using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform backgroundA;
    [SerializeField] private Transform backgroundB;
    [SerializeField] private GameSpeedController speedController;
    [SerializeField] private Camera worldCamera;

    [Header("Settings")]
    [SerializeField] private float speedMultiplier = 1f;

    private float backgroundHeight;

    private Vector3 startPositionA;
    private Vector3 startPositionB;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        startPositionA = backgroundA.position;
        startPositionB = backgroundB.position;

        SpriteRenderer renderer =
            backgroundA.GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            renderer =
                backgroundA.GetComponentInChildren<SpriteRenderer>();
        }

        if (renderer != null)
        {
            backgroundHeight = renderer.bounds.size.y;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (speedController == null)
        {
            return;
        }

        float speed =
            speedController.CurrentSpeed *
            speedMultiplier;

        Vector3 movement =
            Vector3.down *
            speed *
            Time.deltaTime;

        backgroundA.position += movement;
        backgroundB.position += movement;

        CheckAndRecycle(backgroundA, backgroundB);
        CheckAndRecycle(backgroundB, backgroundA);
    }

    private void CheckAndRecycle(
        Transform background,
        Transform otherBackground)
    {
        SpriteRenderer renderer =
            background.GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            renderer =
                background.GetComponentInChildren<SpriteRenderer>();
        }

        if (renderer == null)
        {
            return;
        }

        float cameraDistance =
            Mathf.Abs(worldCamera.transform.position.z);

        float cameraBottom =
            worldCamera.ViewportToWorldPoint(
                new Vector3(
                    0.5f,
                    0f,
                    cameraDistance
                )
            ).y;

        // Background has completely passed below the screen.
        if (renderer.bounds.max.y < cameraBottom)
        {
            background.position =
                new Vector3(
                    background.position.x,
                    otherBackground.position.y +
                    backgroundHeight,
                    background.position.z
                );
        }
    }

    public void ResetBackground()
    {
        backgroundA.position = startPositionA;
        backgroundB.position = startPositionB;
    }
}