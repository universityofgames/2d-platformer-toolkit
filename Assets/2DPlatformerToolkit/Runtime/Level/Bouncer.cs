using UnityEngine;
using UnityEngine.Events;
using PlatformerToolkit.Characters;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Level
{
    /// <summary>
    /// Spring pad that launches characters and dynamic rigidbodies upward on contact.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Level/Bouncer")]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Bouncer : MonoBehaviour
    {
        [Tooltip("Peak height of the bounce, in world units.")]
        [SerializeField, Min(0.1f)] private float bounceHeight = 5f;

        [Tooltip("Layers that can be bounced.")]
        [SerializeField] private LayerMask affectedLayers = ~0;

        [Tooltip("Fraction of the collider height (from the top) that counts as landing on the pad. Side contacts never bounce.")]
        [SerializeField, Range(0f, 0.5f)] private float topTolerance = 0.25f;

        [SerializeField] private UnityEvent bounced = new UnityEvent();

        private Collider2D padCollider;

        /// <summary>
        /// Raised on every successful bounce.
        /// </summary>
        public UnityEvent Bounced => bounced;

        private void Awake()
        {
            padCollider = GetComponent<Collider2D>();
        }

        private void OnCollisionEnter2D(Collision2D collision) => TryBounce(collision.collider);

        private void OnTriggerEnter2D(Collider2D other) => TryBounce(other);

        private void TryBounce(Collider2D other)
        {
            if (!affectedLayers.Contains(other.gameObject.layer))
                return;

            // Only launch things landing on the pad, never side contacts.
            Bounds bounds = padCollider.bounds;
            if (other.bounds.min.y < bounds.max.y - bounds.size.y * topTolerance)
                return;

            CharacterMotor2D motor = other.GetComponentInParent<CharacterMotor2D>();
            if (motor != null)
            {
                motor.Bounce(bounceHeight);
                bounced.Invoke();
                return;
            }

            Rigidbody2D otherBody = other.attachedRigidbody;
            if (otherBody != null && otherBody.bodyType == RigidbodyType2D.Dynamic)
            {
                float gravity = -Physics2D.gravity.y * Mathf.Max(0.01f, otherBody.gravityScale);
                Vector2 velocity = otherBody.linearVelocity;
                velocity.y = Mathf.Sqrt(2f * gravity * bounceHeight);
                otherBody.linearVelocity = velocity;
                bounced.Invoke();
            }
        }
    }
}
