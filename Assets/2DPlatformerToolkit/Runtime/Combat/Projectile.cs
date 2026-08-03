using UnityEngine;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Combat
{
    /// <summary>
    /// Simple projectile: flies in a straight line, damages the first
    /// <see cref="IDamageable"/> it touches and destroys itself on any solid
    /// contact. Spawn it through a <see cref="ProjectileLauncher"/>.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Combat/Projectile")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Projectile : MonoBehaviour
    {
        [Tooltip("Flight speed, in units per second.")]
        [SerializeField, Min(0.1f)] private float speed = 14f;

        [Tooltip("Seconds before the projectile despawns on its own.")]
        [SerializeField, Min(0.1f)] private float lifetime = 3f;

        [Tooltip("Hit points removed on impact.")]
        [SerializeField, Min(0)] private int damage = 1;

        [Tooltip("Layers that can be damaged.")]
        [SerializeField] private LayerMask targetLayers = ~0;

        [Tooltip("Effect prefab spawned at the impact point.")]
        [SerializeField] private GameObject hitEffect;

        private Rigidbody2D body;
        private GameObject owner;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void Reset()
        {
            var rigidbody2d = GetComponent<Rigidbody2D>();
            rigidbody2d.gravityScale = 0f;
            GetComponent<Collider2D>().isTrigger = true;
        }

        /// <summary>
        /// Fires the projectile. The owner (and its children) are ignored on impact.
        /// </summary>
        public void Launch(Vector2 direction, GameObject launchOwner = null)
        {
            owner = launchOwner;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            body.linearVelocity = direction * speed;
            transform.right = direction;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore the shooter and other trigger zones (pick-ups, checkpoints).
            if (other.isTrigger || IsOwner(other))
                return;

            if (targetLayers.Contains(other.gameObject.layer))
            {
                IDamageable damageable = other.GetComponentInParent<IDamageable>();
                if (damageable != null && damage > 0)
                    damageable.ApplyDamage(new DamageInfo(damage, transform.position, owner != null ? owner : gameObject));
            }

            if (hitEffect != null)
                Instantiate(hitEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }

        private bool IsOwner(Collider2D other)
        {
            if (owner == null)
                return false;

            if (other.gameObject == owner)
                return true;

            Rigidbody2D otherBody = other.attachedRigidbody;
            return otherBody != null && otherBody.gameObject == owner;
        }
    }
}
