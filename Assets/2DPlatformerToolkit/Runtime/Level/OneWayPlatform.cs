using UnityEngine;

namespace PlatformerToolkit.Level
{
    /// <summary>
    /// Platform that can be jumped through from below and stood on from above.
    /// Configures a <see cref="PlatformEffector2D"/> automatically; pressing
    /// down + jump on it drops the player through (handled by
    /// <see cref="Characters.PlayerController"/>).
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Level/One Way Platform")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class OneWayPlatform : MonoBehaviour
    {
        [Tooltip("Arc over which the platform is solid, centred on its top surface.")]
        [SerializeField, Range(30f, 180f)] private float surfaceArc = 160f;

        private void Awake()
        {
            Collider2D platformCollider = GetComponent<Collider2D>();
            platformCollider.usedByEffector = true;

            PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
            if (effector == null)
                effector = gameObject.AddComponent<PlatformEffector2D>();

            effector.useOneWay = true;
            effector.surfaceArc = surfaceArc;
        }
    }
}
