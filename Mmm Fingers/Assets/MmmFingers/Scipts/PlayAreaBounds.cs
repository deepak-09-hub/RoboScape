using UnityEngine;

public class PlayAreaBounds : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;

    [Header("Horizontal Play Area")]
    [Range(0f, 1f)]
    [SerializeField] private float leftViewportX = 0.1f;

    [Range(0f, 1f)]
    [SerializeField] private float rightViewportX = 0.9f;

    public float LeftWorldX
    {
        get
        {
            return worldCamera.ViewportToWorldPoint(
                new Vector3(leftViewportX, 0.5f, GetCameraDistance())
            ).x;
        }
    }

    public float RightWorldX
    {
        get
        {
            return worldCamera.ViewportToWorldPoint(
                new Vector3(rightViewportX, 0.5f, GetCameraDistance())
            ).x;
        }
    }

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }
    }

    private float GetCameraDistance()
    {
        return Mathf.Abs(worldCamera.transform.position.z);
    }
}