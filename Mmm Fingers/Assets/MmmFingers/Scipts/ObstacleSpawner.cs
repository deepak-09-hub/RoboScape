using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private Transform obstacleContainer;

    [Header("Spawn Position")]
    [SerializeField] private float topSpawnOffset = 1.5f;
    [SerializeField] private float bottomDestroyOffset = 1.5f;
    [SerializeField] private float horizontalPadding = 0.5f;
    [SerializeField] private PlayAreaBounds playAreaBounds;

    [Header("Spawn Timing")]
    [SerializeField] private float firstSpawnDelay = 0.8f;
    [SerializeField]
    private Vector2 spawnIntervalRange =
        new Vector2(1.2f, 1.8f);

    [Header("Difficulty")]
    [SerializeField] private GameSpeedController speedController;

    private Coroutine spawnRoutine;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (obstacleContainer == null)
        {
            obstacleContainer = transform;
        }
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null)
        {
            return;
        }

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
        {
            return;
        }

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private IEnumerator SpawnRoutine()
    {
        if (firstSpawnDelay > 0f)
        {
            yield return new WaitForSeconds(firstSpawnDelay);
        }

        while (GameManager.Instance != null &&
               GameManager.Instance.IsPlaying)
        {
            SpawnObstacle();

            float minimumInterval = Mathf.Min(
                spawnIntervalRange.x,
                spawnIntervalRange.y
            );

            float maximumInterval = Mathf.Max(
                spawnIntervalRange.x,
                spawnIntervalRange.y
            );

            float delay = Random.Range(
                minimumInterval,
                maximumInterval
            );

            yield return new WaitForSeconds(delay);
        }

        spawnRoutine = null;
    }

    public void SpawnObstacle()
    {
        if (obstaclePrefabs == null ||
            obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("No obstacle prefabs assigned.");
            return;
        }

        GetCameraWorldBounds(
            out float left,
            out float right,
            out float top,
            out float bottom
        );

        GameObject selectedPrefab =
            obstaclePrefabs[
                Random.Range(0, obstaclePrefabs.Length)
            ];

        float spawnY = top + topSpawnOffset;

        GameObject obstacle = Instantiate(
            selectedPrefab,
            new Vector3(
                0f,
                spawnY,
                selectedPrefab.transform.position.z
            ),
            selectedPrefab.transform.rotation,
            obstacleContainer
        );

        // PUT THE NEW BOUNDS CODE HERE

        Collider2D[] colliders =
            obstacle.GetComponentsInChildren<Collider2D>();

        float leftExtent = 0f;
        float rightExtent = 0f;

        if (colliders.Length > 0)
        {
            Bounds combinedBounds = colliders[0].bounds;

            for (int i = 1; i < colliders.Length; i++)
            {
                combinedBounds.Encapsulate(
                    colliders[i].bounds
                );
            }

            leftExtent =
                obstacle.transform.position.x -
                combinedBounds.min.x;

            rightExtent =
                combinedBounds.max.x -
                obstacle.transform.position.x;
        }

        ObstacleSpawnInfo spawnInfo =
            obstacle.GetComponent<ObstacleSpawnInfo>();

        float animationPadding = 0f;

        if (spawnInfo != null)
        {
            animationPadding =
                spawnInfo.AnimationPaddingX;
        }

        float minimumX =
            playAreaBounds.LeftWorldX +
            leftExtent +
            animationPadding;

        float maximumX =
            playAreaBounds.RightWorldX -
            rightExtent -
            animationPadding;

        float spawnX =
            Random.Range(
                minimumX,
                maximumX
            );

        Vector3 finalPosition =
            obstacle.transform.position;

        finalPosition.x = spawnX;

        obstacle.transform.position =
            finalPosition;

        // THEN CONTINUE AS BEFORE

        ScrollingObstacle scrollingObstacle =
            obstacle.GetComponent<ScrollingObstacle>();

        if (scrollingObstacle == null)
        {
            Debug.LogWarning(
                obstacle.name +
                " does not contain ScrollingObstacle."
            );

            return;
        }

        float destroyY =
            bottom - bottomDestroyOffset;

        scrollingObstacle.Initialize(
            speedController,
            destroyY
        );
    }

    public void ClearObstacles()
    {
        if (obstacleContainer == null)
        {
            return;
        }

        for (int i = obstacleContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                obstacleContainer.GetChild(i).gameObject
            );
        }
    }

    private void GetCameraWorldBounds(
        out float left,
        out float right,
        out float top,
        out float bottom)
    {
        float cameraDistance =
            Mathf.Abs(worldCamera.transform.position.z);

        Vector3 bottomLeft =
            worldCamera.ViewportToWorldPoint(
                new Vector3(0f, 0f, cameraDistance)
            );

        Vector3 topRight =
            worldCamera.ViewportToWorldPoint(
                new Vector3(1f, 1f, cameraDistance)
            );

        left = bottomLeft.x;
        right = topRight.x;
        bottom = bottomLeft.y;
        top = topRight.y;
    }

    //public void SetDifficulty(
    //    float newObstacleSpeed,
    //    Vector2 newSpawnIntervalRange)
    //{
    //    obstacleSpeed = Mathf.Max(0.1f, newObstacleSpeed);
    //    spawnIntervalRange = newSpawnIntervalRange;
    //}
}