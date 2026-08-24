using UnityEngine;

public class ObstacleSpawnInfo : MonoBehaviour
{
    [Tooltip("Extra horizontal space needed because of this obstacle's animation.")]
    [SerializeField] private float animationPaddingX = 0f;

    public float AnimationPaddingX => animationPaddingX;
}