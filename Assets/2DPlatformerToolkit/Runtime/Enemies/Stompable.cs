using UnityEngine;
using UnityEngine.Events;
using PlatformerToolkit.Characters;
using PlatformerToolkit.Combat;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Enemies
{
    /// <summary>
    /// Lets the player defeat this enemy by jumping on top of it. On a valid
    /// stomp the attacker is bounced upward and this object takes damage —
    /// or is destroyed when it has no <see cref="Health"/>.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Enemies/Stompable")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Stompable : MonoBehaviour
    {
        [Tooltip("Bounce height granted to the attacker, in world units.")]
        [SerializeField, Min(0f)] private float bounceHeight = 2.5f;

        [Tooltip("Damage dealt to this object per stomp. Zero kills instantly.")]
        [SerializeField, Min(0)] private int damagePerStomp = 1;

        [Tooltip("Fraction of the collider height (from the top) that counts as a stomp.")]
        [SerializeField, Range(0f, 0.5f)] private float topTolerance = 0.15f;

        [Tooltip("Global freeze-frame on a stomp, for impact feel. Zero disables it.")]
        [SerializeField, Range(0f, 0.2f)] private float hitStopDuration = 0.06f;

        [SerializeField] private UnityEvent stomped = new UnityEvent();

        private Collider2D bodyCollider;
        private Health health;

        /// <summary>
        /// Raised on every successful stomp.
        /// </summary>
        public UnityEvent Stomped => stomped;

        private void Awake()
        {
            bodyCollider = GetComponent<Collider2D>();
            health = GetComponent<Health>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryStomp(collision.collider);
        }

        private void TryStomp(Collider2D attacker)
        {
            CharacterMotor2D motor = attacker.GetComponentInParent<CharacterMotor2D>();
            if (motor == null)
                return;

            // The attacker's feet must be near the top of this collider.
            Bounds bounds = bodyCollider.bounds;
            float requiredHeight = bounds.max.y - bounds.size.y * topTolerance;
            if (attacker.bounds.min.y < requiredHeight)
                return;

            motor.Bounce(bounceHeight);
            HitStop.Request(hitStopDuration);
            stomped.Invoke();

            if (health != null)
            {
                if (damagePerStomp > 0)
                    health.ApplyDamage(new DamageInfo(damagePerStomp, attacker.transform.position, attacker.gameObject));
                else
                    health.Kill();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
