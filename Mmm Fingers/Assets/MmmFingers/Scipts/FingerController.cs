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

    [Header("Hold To Start")]
    [SerializeField] private float holdDuration = 0.2f;

    private bool waitingForHold;
    private float holdTimer;
    private Vector2 latestHoldPosition;

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

            if (!IsPointerOverBlockingUI(screenPosition))
            {
                StartPendingHold(
                    screenPosition,
                    -1
                );
            }
        }

        // Waiting for the required hold time
        if (waitingForHold)
        {
            if (Input.GetMouseButton(0))
            {
                UpdatePendingHold(
                    Input.mousePosition
                );
            }
            else
            {
                // Released too early
                CancelPendingHold();
            }
        }

        // Normal gameplay movement
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
        // ------------------------------------------
        // BEFORE GAME/FINGER HAS STARTED
        // ------------------------------------------

        if (!fingerPressed)
        {
            // Look for a new finger press.
            if (!waitingForHold)
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

                    StartPendingHold(
                        touch.position,
                        touch.fingerId
                    );

                    return;
                }

                return;
            }

            // We already have a finger that
            // we're waiting to see held long enough.
            for (int i = 0;
                 i < Input.touchCount;
                 i++)
            {
                Touch touch =
                    Input.GetTouch(i);

                if (touch.fingerId != activeFingerId)
                {
                    continue;
                }

                if (touch.phase ==
                        TouchPhase.Ended ||
                    touch.phase ==
                        TouchPhase.Canceled)
                {
                    CancelPendingHold();
                    return;
                }

                latestHoldPosition =
                    touch.position;

                UpdatePendingHold(
                    touch.position
                );

                return;
            }

            // Finger vanished before hold finished.
            CancelPendingHold();

            return;
        }

        // ------------------------------------------
        // NORMAL GAMEPLAY
        // ------------------------------------------

        for (int i = 0;
             i < Input.touchCount;
             i++)
        {
            Touch touch =
                Input.GetTouch(i);

            if (touch.fingerId != activeFingerId)
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

    private void StartPendingHold(
    Vector2 screenPosition,
    int fingerId)
    {
        waitingForHold = true;
        holdTimer = 0f;

        latestHoldPosition =
            screenPosition;

        activeFingerId =
            fingerId;
    }

    private void UpdatePendingHold(
        Vector2 currentScreenPosition)
    {
        latestHoldPosition =
            currentScreenPosition;

        holdTimer += Time.deltaTime;

        if (holdTimer < holdDuration)
        {
            return;
        }

        waitingForHold = false;
        holdTimer = 0f;

        BeginFingerPress(
            latestHoldPosition
        );
    }

    private void CancelPendingHold()
    {
        waitingForHold = false;
        holdTimer = 0f;
        activeFingerId = -1;
    }

    private void BeginFingerPress(
    Vector2 screenPosition)
    {
        if (GameManager.Instance.CurrentState !=
                GameManager.GameState.Waiting &&
            GameManager.Instance.CurrentState !=
                GameManager.GameState.Playing)
        {
            CancelPendingHold();
            return;
        }

        Vector2 worldPosition =
            ScreenToWorldPosition(
                screenPosition
            );

        fingerPressed = true;

        body.position =
            worldPosition;

        SetFingerVisible(true);

        if (GameManager.Instance.CurrentState ==
            GameManager.GameState.Waiting)
        {
            GameManager.Instance.StartRun();
        }

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

        transform.position = targetPosition;
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

        CancelPendingHold();

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