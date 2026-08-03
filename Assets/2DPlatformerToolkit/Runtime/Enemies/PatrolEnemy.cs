using UnityEngine;

namespace PlatformerToolkit.Enemies
{
    /// <summary>
    /// Walks in a straight line and turns around at walls and ledges — the
    /// classic patrolling enemy. Combine with <see cref="Combat.Health"/>,
    /// <see cref="Combat.ContactDamager"/> and <see cref="Stompable"/>
    /// for a complete stompable enemy.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Enemies/Patrol Enemy")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PatrolEnemy : MonoBehaviour
    {
        private const float WallCheckDistance = 0.05f;
        private const float LedgeProbeDepth = 0.3f;

        [Tooltip("Walk speed, in units per second.")]
        [SerializeField, Min(0f)] private float moveSpeed = 2f;

        [Tooltip("Start walking to the right instead of the left.")]
        [SerializeField] private bool startMovingRight;

        [Tooltip("Layers treated as ground and walls.")]
        [SerializeField] private LayerMask groundMask = 1;

        [Tooltip("Turn around when reaching a ledge instead of walking off it.")]
        [SerializeField] private bool turnAtLedges = true;

        [Tooltip("Flip the transform to face the walk direction.")]
        [SerializeField] private bool faceMoveDirection = true;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private readonly RaycastHit2D[] castHits = new RaycastHit2D[4];
        private ContactFilter2D groundFilter;
        private int direction = -1;

        /// <summary>
        /// Current walk direction: 1 for right, -1 for left.
        /// </summary>
        public int Direction => direction;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            body.freezeRotation = true;

            groundFilter = new ContactFilter2D();
            groundFilter.SetLayerMask(groundMask);
            groundFilter.useTriggers = false;

            direction = startMovingRight ? 1 : -1;
            ApplyFacing();
        }

        private void FixedUpdate()
        {
            if (ShouldTurn())
            {
                direction = -direction;
                ApplyFacing();
            }

            Vector2 velocity = body.linearVelocity;
            velocity.x = direction * moveSpeed;
            body.linearVelocity = velocity;
        }

        private bool ShouldTurn()
        {
            // Wall ahead.
            int hitCount = body.Cast(new Vector2(direction, 0f), groundFilter, castHits, WallCheckDistance);
            for (int i = 0; i < hitCount; i++)
            {
                if (Mathf.Abs(castHits[i].normal.x) > 0.5f)
                    return true;
            }

            // Ledge ahead. Only turn while grounded, so the enemy is not
            // spinning in place when falling.
            if (turnAtLedges && IsGrounded())
            {
                Bounds bounds = bodyCollider.bounds;
                var probe = new Vector2(
                    (direction > 0 ? bounds.max.x : bounds.min.x) + direction * WallCheckDistance,
                    bounds.min.y);

                if (!Physics2D.Raycast(probe, Vector2.down, LedgeProbeDepth, groundMask))
                    return true;
            }

            return false;
        }

        private bool IsGrounded()
        {
            return body.Cast(Vector2.down, groundFilter, castHits, 0.1f) > 0;
        }

        private void ApplyFacing()
        {
            if (!faceMoveDirection)
                return;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }
    }
}
