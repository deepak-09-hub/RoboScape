using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FingerController : MonoBehaviour
{
    [Header("References")]
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private SpriteRenderer fingerRenderer;
    [SerializeField] private Collider2D fingerCollider;
    [SerializeField] private PlayAreaBounds playAreaBounds;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float fallbackCastRadius = 0.2f;

    [Header("Movement")]
    [Tooltip("Moves the player slightly above the real finger position.")]
    [SerializeField] private Vector2 touchOffset = Vector2.zero;

    [SerializeField] private bool clampInsideCamera = true;
    [SerializeField] private float cameraEdgePadding = 0.1f;

    [Header("Rules")]
    [SerializeField] private bool loseWhenFingerReleased = true;

    private Rigidbody2D body;

    private bool fingerPressed;
    private int activeFingerId = -1;

    private readonly List<RaycastResult> uiRaycastResults =
        new List<RaycastResult>();

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (fingerCollider == null)
        {
            fingerCollider = GetComponent<Collider2D>();
        }

        DisableFinger();
    }

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

#if UNITY_EDITOR

        // Device Simulator may provide touch input.
        // Normal Game View normally provides mouse input.
        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }

#elif UNITY_ANDROID || UNITY_IOS

        HandleTouchInput();

#else

        HandleMouseInput();

#endif
    }

    // ==================================================
    // MOUSE
    // ==================================================

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 screenPosition =
                Input.mousePosition;

            if (!IsPointerOverBlockingUI(
                    screenPosition))
            {
                BeginFingerPress(
                    screenPosition, 0.15f
                );
            }
        }

        if (fingerPressed &&
            Input.GetMouseButton(0))
        {
            MoveFinger(
                Input.mousePosition
            );
        }

        if (fingerPressed &&
            Input.GetMouseButtonUp(0))
        {
            EndFingerPress();
        }
    }

    // ==================================================
    // TOUCH
    // ==================================================

    private void HandleTouchInput()
    {
        if (!fingerPressed)
        {
            for (int i = 0;
                 i < Input.touchCount;
                 i++)
            {
                Touch touch =
                    Input.GetTouch(i);

                if (touch.phase !=
                    TouchPhase.Began)
                {
                    continue;
                }

                if (IsPointerOverBlockingUI(
                        touch.position,
                        touch.fingerId))
                {
                    continue;
                }

                activeFingerId =
                    touch.fingerId;

                BeginFingerPress(
                    touch.position, 0.15f
                );

                return;
            }

            return;
        }

        for (int i = 0;
             i < Input.touchCount;
             i++)
        {
            Touch touch =
                Input.GetTouch(i);

            if (touch.fingerId !=
                activeFingerId)
            {
                continue;
            }

            switch (touch.phase)
            {
                case TouchPhase.Began:
                case TouchPhase.Moved:
                case TouchPhase.Stationary:

                    MoveFinger(
                        touch.position
                    );

                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:

                    EndFingerPress();

                    break;
            }

            return;
        }
    }

    // ==================================================
    // BEGIN
    // ==================================================

    private void BeginFingerPress(Vector2 screenPosition, float delay)
    {
        StartCoroutine(BeginFingerPressRoutine(
            screenPosition,
            delay
        ));
    }

    private IEnumerator BeginFingerPressRoutine(
        Vector2 screenPosition, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameManager.Instance.CurrentState !=
                GameManager.GameState.Waiting &&
            GameManager.Instance.CurrentState !=
                GameManager.GameState.Playing)
        {
           yield return null;
        }

        Vector2 worldPosition =
            ScreenToWorldPosition(
                screenPosition
            );

        fingerPressed = true;

        body.position =
            worldPosition;

        SetFingerVisible(true);

        // The SAME touch that places the finger
        // also starts the game.
        if (GameManager.Instance.CurrentState ==
            GameManager.GameState.Waiting)
        {
            GameManager.Instance.StartRun();
        }

        // Player may have placed the finger
        // directly onto an obstacle.
        if (HitsObstacle(
                worldPosition,
                worldPosition))
        {
            LoseGame();
        }
    }

    // ==================================================
    // MOVE
    // ==================================================

    private void MoveFinger(
        Vector2 screenPosition)
    {
        if (!fingerPressed)
        {
            return;
        }

        Vector2 currentPosition =
            body.position;

        Vector2 targetPosition =
            ScreenToWorldPosition(
                screenPosition
            );

        if (GameManager.Instance.IsPlaying &&
            HitsObstacle(
                currentPosition,
                targetPosition))
        {
            LoseGame();
            return;
        }

        body.position =
            targetPosition;
    }

    // ==================================================
    // RELEASE
    // ==================================================

    private void EndFingerPress()
    {
        if (!fingerPressed)
        {
            return;
        }

        fingerPressed = false;
        activeFingerId = -1;

        if (loseWhenFingerReleased &&
            GameManager.Instance.IsPlaying)
        {
            LoseGame();
            return;
        }

        DisableFinger();
    }

    // ==================================================
    // SCREEN -> WORLD
    // ==================================================

    private Vector2 ScreenToWorldPosition(
        Vector2 screenPosition)
    {
        float cameraDistance =
            Mathf.Abs(
                worldCamera.transform.position.z -
                transform.position.z
            );

        Vector3 worldPosition =
            worldCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    cameraDistance
                )
            );

        Vector2 finalPosition =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );

        finalPosition += touchOffset;

        if (clampInsideCamera)
        {
            finalPosition =
                ClampToCamera(
                    finalPosition
                );
        }

        return finalPosition;
    }

    // ==================================================
    // CAMERA CLAMP
    // ==================================================

    private Vector2 ClampToCamera(
        Vector2 position)
    {
        float cameraDistance =
            Mathf.Abs(
                worldCamera.transform.position.z -
                transform.position.z
            );

        Vector3 bottomLeft =
            worldCamera.ViewportToWorldPoint(
                new Vector3(
                    0f,
                    0f,
                    cameraDistance
                )
            );

        Vector3 topRight =
            worldCamera.ViewportToWorldPoint(
                new Vector3(
                    1f,
                    1f,
                    cameraDistance
                )
            );

        float radius =
            GetCastRadius();

        position.x = Mathf.Clamp(
            position.x,
            playAreaBounds.LeftWorldX + radius + cameraEdgePadding,
            playAreaBounds.RightWorldX - radius - cameraEdgePadding
        );

        position.y =
            Mathf.Clamp(
                position.y,
                bottomLeft.y +
                    radius +
                    cameraEdgePadding,
                topRight.y -
                    radius -
                    cameraEdgePadding
            );

        return position;
    }

    // ==================================================
    // OBSTACLE COLLISION
    // ==================================================

    private bool HitsObstacle(
        Vector2 start,
        Vector2 target)
    {
        float radius =
            GetCastRadius();

        Collider2D overlap =
            Physics2D.OverlapCircle(
                target,
                radius,
                obstacleLayer
            );

        if (overlap != null)
        {
            return true;
        }

        Vector2 movement =
            target - start;

        float distance =
            movement.magnitude;

        if (distance <= 0.001f)
        {
            return false;
        }

        RaycastHit2D hit =
            Physics2D.CircleCast(
                start,
                radius,
                movement.normalized,
                distance,
                obstacleLayer
            );

        return hit.collider != null;
    }

    private float GetCastRadius()
    {
        if (fingerCollider is
            CircleCollider2D circleCollider)
        {
            float largestScale =
                Mathf.Max(
                    transform.lossyScale.x,
                    transform.lossyScale.y
                );

            return
                circleCollider.radius *
                largestScale;
        }

        return fallbackCastRadius;
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (IsInObstacleLayer(
                other.gameObject))
        {
            LoseGame();
        }
    }

    private bool IsInObstacleLayer(
        GameObject target)
    {
        return
            (obstacleLayer.value &
             (1 << target.layer)) != 0;
    }

    // ==================================================
    // LOSE
    // ==================================================

    private void LoseGame()
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying)
        {
            return;
        }

        GameManager.Instance.GameOver();
    }

    // ==================================================
    // VISUAL
    // ==================================================

    private void SetFingerVisible(
        bool visible)
    {
        if (fingerRenderer != null)
        {
            fingerRenderer.enabled =
                visible;
        }

        if (fingerCollider != null)
        {
            fingerCollider.enabled =
                visible;
        }
    }

    public void DisableFinger()
    {
        fingerPressed = false;
        activeFingerId = -1;

        SetFingerVisible(false);
    }

    // ==================================================
    // UI BLOCKING
    // ==================================================

    private bool IsPointerOverBlockingUI(
        Vector2 screenPosition,
        int pointerId = -1)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData pointerData =
            new PointerEventData(
                EventSystem.current
            );

        pointerData.position =
            screenPosition;

        pointerData.pointerId =
            pointerId;

        uiRaycastResults.Clear();

        EventSystem.current.RaycastAll(
            pointerData,
            uiRaycastResults
        );

        for (int i = 0;
             i < uiRaycastResults.Count;
             i++)
        {
            GameObject hitObject =
                uiRaycastResults[i]
                    .gameObject;

            /*
             * Decorative Images/Text/Panels
             * are allowed.
             *
             * Actual interactive UI
             * blocks gameplay input.
             */
            Selectable selectable =
                hitObject.GetComponentInParent<
                    Selectable
                >();

            if (selectable != null &&
                selectable.IsInteractable())
            {
                return true;
            }
        }

        return false;
    }
}