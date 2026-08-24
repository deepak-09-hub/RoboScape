using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Transform backgroundA;
    [SerializeField] private Transform backgroundB;

    [Header("Left Border")]
    [SerializeField] private Transform leftBorderA;
    [SerializeField] private Transform leftBorderB;

    [Header("Right Border")]
    [SerializeField] private Transform rightBorderA;
    [SerializeField] private Transform rightBorderB;

    [Header("References")]
    [SerializeField] private GameSpeedController speedController;
    [SerializeField] private Camera worldCamera;

    [Header("Settings")]
    [SerializeField] private float speedMultiplier = 1f;

    private float backgroundHeight;
    private float leftBorderHeight;
    private float rightBorderHeight;

    private Vector3 backgroundAStart;
    private Vector3 backgroundBStart;

    private Vector3 leftBorderAStart;
    private Vector3 leftBorderBStart;

    private Vector3 rightBorderAStart;
    private Vector3 rightBorderBStart;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        // Save starting positions
        backgroundAStart = backgroundA.position;
        backgroundBStart = backgroundB.position;

        leftBorderAStart = leftBorderA.position;
        leftBorderBStart = leftBorderB.position;

        rightBorderAStart = rightBorderA.position;
        rightBorderBStart = rightBorderB.position;

        // Get individual tile heights
        backgroundHeight = GetSpriteHeight(backgroundA);
        leftBorderHeight = GetSpriteHeight(leftBorderA);
        rightBorderHeight = GetSpriteHeight(rightBorderA);
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

        // Background
        Move(backgroundA, movement);
        Move(backgroundB, movement);

        // Left border
        Move(leftBorderA, movement);
        Move(leftBorderB, movement);

        // Right border
        Move(rightBorderA, movement);
        Move(rightBorderB, movement);

        // Recycle background
        CheckAndRecycle(
            backgroundA,
            backgroundB,
            backgroundHeight
        );

        CheckAndRecycle(
            backgroundB,
            backgroundA,
            backgroundHeight
        );

        // Recycle left border
        CheckAndRecycle(
            leftBorderA,
            leftBorderB,
            leftBorderHeight
        );

        CheckAndRecycle(
            leftBorderB,
            leftBorderA,
            leftBorderHeight
        );

        // Recycle right border
        CheckAndRecycle(
            rightBorderA,
            rightBorderB,
            rightBorderHeight
        );

        CheckAndRecycle(
            rightBorderB,
            rightBorderA,
            rightBorderHeight
        );
    }

    private void Move(
        Transform target,
        Vector3 movement)
    {
        if (target == null)
        {
            return;
        }

        target.position += movement;
    }

    private void CheckAndRecycle(
        Transform tile,
        Transform otherTile,
        float tileHeight)
    {
        if (tile == null ||
            otherTile == null)
        {
            return;
        }

        SpriteRenderer renderer =
            tile.GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            renderer =
                tile.GetComponentInChildren<SpriteRenderer>();
        }

        if (renderer == null)
        {
            return;
        }

        float cameraDistance =
            Mathf.Abs(
                worldCamera.transform.position.z
            );

        float cameraBottom =
            worldCamera.ViewportToWorldPoint(
                new Vector3(
                    0.5f,
                    0f,
                    cameraDistance
                )
            ).y;

        // Completely below the screen
        if (renderer.bounds.max.y < cameraBottom)
        {
            tile.position =
                new Vector3(
                    tile.position.x,
                    otherTile.position.y +
                    tileHeight,
                    tile.position.z
                );
        }
    }

    private float GetSpriteHeight(
        Transform target)
    {
        if (target == null)
        {
            return 0f;
        }

        SpriteRenderer renderer =
            target.GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            renderer =
                target.GetComponentInChildren<SpriteRenderer>();
        }

        if (renderer == null)
        {
            Debug.LogWarning(
                target.name +
                " has no SpriteRenderer."
            );

            return 0f;
        }

        return renderer.bounds.size.y;
    }

    public void ResetBackground()
    {
        backgroundA.position = backgroundAStart;
        backgroundB.position = backgroundBStart;

        leftBorderA.position = leftBorderAStart;
        leftBorderB.position = leftBorderBStart;

        rightBorderA.position = rightBorderAStart;
        rightBorderB.position = rightBorderBStart;
    }
}