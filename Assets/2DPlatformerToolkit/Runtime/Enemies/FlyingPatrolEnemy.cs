using UnityEngine;

namespace PlatformerToolkit.Enemies
{
    /// <summary>
    /// Airborne enemy that patrols between waypoints, unaffected by gravity.
    /// Combine with <see cref="Combat.Health"/>,
    /// <see cref="Combat.ContactDamager"/> and <see cref="Stompable"/>.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Enemies/Flying Patrol Enemy")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class FlyingPatrolEnemy : MonoBehaviour
    {
        /// <summary>How the enemy traverses its waypoints.</summary>
        public enum PathMode
        {
            /// <summary>After the last waypoint, continue with the first.</summary>
            Loop,

            /// <summary>Reverse direction at both ends of the path.</summary>
            PingPong,
        }

        private const float ArrivalToleranceSqr = 0.0001f;

        [Tooltip("Waypoints relative to the starting position. The first should stay at (0, 0).")]
        [SerializeField] private Vector2[] waypoints = { Vector2.zero, new Vector2(4f, 0f) };

        [Tooltip("Flight speed, in units per second.")]
        [SerializeField, Min(0f)] private float moveSpeed = 2.5f;

        [Tooltip("Seconds to pause at each waypoint.")]
        [SerializeField, Min(0f)] private float waitTime;

        [SerializeField] private PathMode pathMode = PathMode.PingPong;

        [Tooltip("Flip the transform to face the flight direction.")]
        [SerializeField] private bool faceMoveDirection = true;

        private Rigidbody2D body;
        private Vector2 origin;
        private int targetIndex;
        private int pathDirection = 1;
        private float waitUntil;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            origin = body.position;
            targetIndex = waypoints != null && waypoints.Length > 1 ? 1 : 0;
        }

        private void Reset()
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        }

        private void FixedUpdate()
        {
            if (waypoints == null || waypoints.Length < 2 || Time.time < waitUntil)
                return;

            Vector2 target = origin + waypoints[targetIndex];
            Vector2 next = Vector2.MoveTowards(body.position, target, moveSpeed * Time.fixedDeltaTime);

            ApplyFacing(next.x - body.position.x);
            body.MovePosition(next);

            if ((target - next).sqrMagnitude < ArrivalToleranceSqr)
            {
                waitUntil = Time.time + waitTime;
                AdvanceTarget();
            }
        }

        private void AdvanceTarget()
        {
            if (pathMode == PathMode.Loop)
            {
                targetIndex = (targetIndex + 1) % waypoints.Length;
                return;
            }

            if (targetIndex + pathDirection < 0 || targetIndex + pathDirection >= waypoints.Length)
                pathDirection = -pathDirection;

            targetIndex += pathDirection;
        }

        private void ApplyFacing(float deltaX)
        {
            if (!faceMoveDirection || Mathf.Abs(deltaX) < 0.0001f)
                return;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (deltaX > 0f ? 1f : -1f);
            transform.localScale = scale;
        }

        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            Vector2 basePosition = Application.isPlaying ? origin : (Vector2)transform.position;
            Gizmos.color = Color.magenta;

            for (int i = 0; i < waypoints.Length; i++)
            {
                Vector2 point = basePosition + waypoints[i];
                Gizmos.DrawWireSphere(point, 0.15f);
                if (i > 0)
                    Gizmos.DrawLine(basePosition + waypoints[i - 1], point);
            }

            if (pathMode == PathMode.Loop && waypoints.Length > 2)
                Gizmos.DrawLine(basePosition + waypoints[waypoints.Length - 1], basePosition + waypoints[0]);
        }
    }
}
