using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ScrollingObstacle : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator obstacleAnimator;

    [Tooltip("Leave empty if the animation plays automatically.")]
    [SerializeField] private string animationBool;

    [Header("Movement")]
    [Tooltip("Allows individual obstacles to be slightly faster or slower.")]
    [SerializeField] private float speedMultiplier = 1f;

    [SerializeField] private bool destroyBelowScreen = true;

    private Rigidbody2D body;

    private GameSpeedController speedController;

    private float destroyPositionY;
    private bool initialized;


    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        // Animator should normally be on a CHILD object.
        if (obstacleAnimator == null)
        {
            obstacleAnimator = GetComponentInChildren<Animator>();
        }

        StartObstacleAnimation();
    }


    private void StartObstacleAnimation()
    {
        if (obstacleAnimator == null)
        {
            return;
        }

        // If you're using a bool to start the animation.
        if (!string.IsNullOrWhiteSpace(animationBool))
        {
            obstacleAnimator.SetBool(
                animationBool,
                true
            );
        }
    }


    public void Initialize(
        GameSpeedController gameSpeedController,
        float bottomDestroyPosition)
    {
        speedController = gameSpeedController;
        destroyPositionY = bottomDestroyPosition;

        initialized = true;
    }


    private void FixedUpdate()
    {
        if (!initialized)
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

        MoveDown();

        CheckForDestroy();
    }


    private void MoveDown()
    {
        float movementSpeed =
            speedController.CurrentSpeed *
            speedMultiplier;

        Vector2 movement =
            Vector2.down *
            movementSpeed *
            Time.fixedDeltaTime;

        Vector2 targetPosition =
            body.position + movement;

        body.MovePosition(targetPosition);
    }


    private void CheckForDestroy()
    {
        if (!destroyBelowScreen)
        {
            return;
        }

        if (body.position.y <= destroyPositionY)
        {
            Destroy(gameObject);
        }
    }
}