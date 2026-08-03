using System.Collections.Generic;
using UnityEngine;

namespace PlatformerToolkit.Level
{
    /// <summary>
    /// Kinematic platform that travels between waypoints and carries any
    /// dynamic rigidbody standing on top of it.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Level/Moving Platform")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class MovingPlatform : MonoBehaviour
    {
        /// <summary>How the platform traverses its waypoints.</summary>
        public enum PathMode
        {
            /// <summary>After the last waypoint, continue with the first.</summary>
            Loop,

            /// <summary>Reverse direction at both ends of the path.</summary>
            PingPong,
        }

        private const float ArrivalToleranceSqr = 0.0001f;
        private const float RiderProbeHeight = 0.1f;

        [Tooltip("Waypoints relative to the platform's starting position. The first should stay at (0, 0).")]
        [SerializeField] private Vector2[] waypoints = { Vector2.zero, new Vector2(4f, 0f) };

        [Tooltip("Travel speed, in units per second.")]
        [SerializeField, Min(0f)] private float moveSpeed = 2f;

        [Tooltip("Seconds to pause at each waypoint.")]
        [SerializeField, Min(0f)] private float waitTime = 0.4f;

        [SerializeField] private PathMode pathMode = PathMode.PingPong;

        [Tooltip("Layers of riders carried by the platform.")]
        [SerializeField] private LayerMask riderLayers = ~0;

        private Rigidbody2D body;
        private Collider2D platformCollider;
        private readonly List<Collider2D> riderBuffer = new List<Collider2D>(8);
        private ContactFilter2D riderFilter;
        private Vector2 origin;
        private int targetIndex;
        private int pathDirection = 1;
        private float waitUntil;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            platformCollider = GetComponent<Collider2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            origin = body.position;
            targetIndex = waypoints != null && waypoints.Length > 1 ? 1 : 0;

            riderFilter = new ContactFilter2D();
            riderFilter.SetLayerMask(riderLayers);
            riderFilter.useTriggers = false;
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
            Vector2 delta = next - body.position;

            CarryRiders(delta);
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

        private void CarryRiders(Vector2 delta)
        {
            if (delta == Vector2.zero)
                return;

            Bounds bounds = platformCollider.bounds;
            var areaCenter = new Vector2(bounds.center.x, bounds.max.y + RiderProbeHeight * 0.5f);
            var areaSize = new Vector2(bounds.size.x, RiderProbeHeight);

            int count = Physics2D.OverlapBox(areaCenter, areaSize, 0f, riderFilter, riderBuffer);
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D rider = riderBuffer[i].attachedRigidbody;
                if (rider != null && rider != body && rider.bodyType == RigidbodyType2D.Dynamic)
                    rider.position += delta;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            Vector2 basePosition = Application.isPlaying ? origin : (Vector2)transform.position;
            Gizmos.color = Color.cyan;

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
