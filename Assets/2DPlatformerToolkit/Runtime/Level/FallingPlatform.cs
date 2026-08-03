using UnityEngine;
using UnityEngine.Events;

namespace PlatformerToolkit.Level
{
    /// <summary>
    /// Platform that shakes when stood on, then crumbles and falls away —
    /// and optionally respawns at its original position.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Level/Falling Platform")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class FallingPlatform : MonoBehaviour
    {
        private enum State
        {
            Idle,
            Shaking,
            Falling,
            Hidden,
        }

        private const float RiderProbeHeight = 0.1f;
        private const float FallDuration = 2f;

        [Tooltip("How long the platform shakes before falling.")]
        [SerializeField, Min(0f)] private float shakeTime = 0.7f;

        [Tooltip("Horizontal shake amplitude, in world units.")]
        [SerializeField, Range(0f, 0.2f)] private float shakeAmplitude = 0.05f;

        [Tooltip("Downward acceleration while falling.")]
        [SerializeField, Min(1f)] private float fallGravity = 30f;

        [Tooltip("Seconds until the platform reappears. Zero destroys it instead.")]
        [SerializeField, Min(0f)] private float respawnDelay = 3.5f;

        [Tooltip("Layers that can trigger the platform.")]
        [SerializeField] private LayerMask riderLayers = ~0;

        [Header("Events")]
        [SerializeField] private UnityEvent startedShaking = new UnityEvent();
        [SerializeField] private UnityEvent dropped = new UnityEvent();
        [SerializeField] private UnityEvent restored = new UnityEvent();

        private Rigidbody2D body;
        private Collider2D platformCollider;
        private SpriteRenderer[] renderers;
        private readonly System.Collections.Generic.List<Collider2D> riderBuffer =
            new System.Collections.Generic.List<Collider2D>(4);
        private ContactFilter2D riderFilter;
        private Vector2 origin;
        private State state = State.Idle;
        private float stateEndsAt;
        private float fallSpeed;

        /// <summary>Raised when a rider triggers the shake.</summary>
        public UnityEvent StartedShaking => startedShaking;

        /// <summary>Raised when the platform starts falling.</summary>
        public UnityEvent Dropped => dropped;

        /// <summary>Raised when the platform reappears.</summary>
        public UnityEvent Restored => restored;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            platformCollider = GetComponent<Collider2D>();
            renderers = GetComponentsInChildren<SpriteRenderer>();
            body.bodyType = RigidbodyType2D.Kinematic;
            origin = body.position;

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
            switch (state)
            {
                case State.Idle:
                    if (HasRider())
                    {
                        state = State.Shaking;
                        stateEndsAt = Time.time + shakeTime;
                        startedShaking.Invoke();
                    }
                    break;

                case State.Shaking:
                    body.MovePosition(origin + new Vector2(
                        Mathf.Sin(Time.time * 45f) * shakeAmplitude, 0f));
                    if (Time.time >= stateEndsAt)
                    {
                        state = State.Falling;
                        stateEndsAt = Time.time + FallDuration;
                        fallSpeed = 0f;
                        dropped.Invoke();
                    }
                    break;

                case State.Falling:
                    fallSpeed += fallGravity * Time.fixedDeltaTime;
                    body.MovePosition(body.position + Vector2.down * (fallSpeed * Time.fixedDeltaTime));
                    if (Time.time >= stateEndsAt)
                        Hide();
                    break;

                case State.Hidden:
                    if (Time.time >= stateEndsAt)
                        Restore();
                    break;
            }
        }

        private bool HasRider()
        {
            Bounds bounds = platformCollider.bounds;
            var areaCenter = new Vector2(bounds.center.x, bounds.max.y + RiderProbeHeight * 0.5f);
            var areaSize = new Vector2(bounds.size.x, RiderProbeHeight);

            int count = Physics2D.OverlapBox(areaCenter, areaSize, 0f, riderFilter, riderBuffer);
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D rider = riderBuffer[i].attachedRigidbody;
                if (rider != null && rider != body)
                    return true;
            }

            return false;
        }

        private void Hide()
        {
            if (respawnDelay <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            state = State.Hidden;
            stateEndsAt = Time.time + respawnDelay;
            platformCollider.enabled = false;
            SetRenderersVisible(false);
        }

        private void Restore()
        {
            state = State.Idle;
            fallSpeed = 0f;
            body.position = origin;
            platformCollider.enabled = true;
            SetRenderersVisible(true);
            restored.Invoke();
        }

        private void SetRenderersVisible(bool visible)
        {
            foreach (SpriteRenderer spriteRenderer in renderers)
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = visible;
            }
        }
    }
}
