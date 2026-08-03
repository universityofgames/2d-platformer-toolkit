using UnityEngine;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Combat
{
    /// <summary>
    /// Deals damage to <see cref="IDamageable"/> objects on contact — spikes,
    /// enemy bodies, projectiles. Works with both trigger and solid colliders
    /// and optionally applies knockback away from the contact. Repeated hits
    /// are throttled by the victim's invulnerability window.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Combat/Contact Damager")]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ContactDamager : MonoBehaviour
    {
        private const float TopContactTolerance = 0.1f;

        [Tooltip("Hit points removed per hit.")]
        [SerializeField, Min(1)] private int damage = 1;

        [Tooltip("Layers that can be damaged.")]
        [SerializeField] private LayerMask targetLayers = ~0;

        [Tooltip("Skip contacts coming from above, so stomp attacks are not punished. Enable on stompable enemies.")]
        [SerializeField] private bool ignoreContactsFromAbove;

        [Header("Knockback")]
        [Tooltip("Horizontal knockback speed applied to the victim, away from this object.")]
        [SerializeField, Min(0f)] private float knockbackSpeed = 8f;

        [Tooltip("Upward knockback speed applied to the victim.")]
        [SerializeField, Min(0f)] private float knockbackUpwardSpeed = 5f;

        [Header("Feedback")]
        [Tooltip("Global freeze-frame on a successful hit, for impact feel. Zero disables it.")]
        [SerializeField, Range(0f, 0.2f)] private float hitStopDuration = 0.05f;

        private Collider2D ownCollider;

        private void Awake()
        {
            ownCollider = GetComponent<Collider2D>();
        }

        private void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.collider);

        private void OnCollisionStay2D(Collision2D collision) => TryDamage(collision.collider);

        private void OnTriggerEnter2D(Collider2D other) => TryDamage(other);

        private void OnTriggerStay2D(Collider2D other) => TryDamage(other);

        private void TryDamage(Collider2D victim)
        {
            if (!targetLayers.Contains(victim.gameObject.layer))
                return;

            if (ignoreContactsFromAbove && IsContactFromAbove(victim))
                return;

            IDamageable damageable = victim.GetComponentInParent<IDamageable>();
            if (damageable == null)
                return;

            var info = new DamageInfo(damage, transform.position, gameObject);
            if (!damageable.ApplyDamage(info))
                return;

            HitStop.Request(hitStopDuration);
            ApplyKnockback(victim);
        }

        private bool IsContactFromAbove(Collider2D victim)
        {
            Bounds bounds = ownCollider.bounds;
            return victim.bounds.min.y >= bounds.max.y - bounds.size.y * TopContactTolerance;
        }

        private void ApplyKnockback(Collider2D victim)
        {
            if (knockbackSpeed <= 0f && knockbackUpwardSpeed <= 0f)
                return;

            Rigidbody2D victimBody = victim.attachedRigidbody;
            if (victimBody == null || victimBody.bodyType != RigidbodyType2D.Dynamic)
                return;

            float direction = Mathf.Sign(victimBody.position.x - transform.position.x);
            victimBody.linearVelocity = new Vector2(direction * knockbackSpeed, knockbackUpwardSpeed);
        }
    }
}
