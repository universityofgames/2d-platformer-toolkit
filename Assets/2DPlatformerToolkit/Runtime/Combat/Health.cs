using System;
using UnityEngine;
using UnityEngine.Events;

namespace PlatformerToolkit.Combat
{
    /// <summary>
    /// Hit-point container with an invulnerability window, designer-facing events
    /// and optional destruction on death. Works for players and enemies alike.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Combat/Health")]
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [Tooltip("Maximum and starting hit points.")]
        [SerializeField, Min(1)] private int maxHealth = 3;

        [Tooltip("Seconds of invulnerability after taking a hit.")]
        [SerializeField, Min(0f)] private float invulnerabilityDuration = 1f;

        [Tooltip("Destroy the game object when health reaches zero. Typical for enemies; leave off for the player.")]
        [SerializeField] private bool destroyOnDeath;

        [Tooltip("Delay before the destroy, leaving time for death effects.")]
        [SerializeField, Min(0f)] private float destroyDelay = 0.1f;

        [Header("Events")]
        [SerializeField] private UnityEvent<int, int> healthChanged = new UnityEvent<int, int>();
        [SerializeField] private UnityEvent damaged = new UnityEvent();
        [SerializeField] private UnityEvent healed = new UnityEvent();
        [SerializeField] private UnityEvent died = new UnityEvent();

        private float invulnerableUntil;

        /// <summary>Maximum hit points.</summary>
        public int MaxHealth => maxHealth;

        /// <summary>Current hit points.</summary>
        public int CurrentHealth { get; private set; }

        /// <summary>True while current health is above zero.</summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>True while the post-hit invulnerability window is active.</summary>
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        /// <summary>Raised as (current, max) whenever the value changes.</summary>
        public UnityEvent<int, int> HealthChanged => healthChanged;

        /// <summary>Raised whenever damage is applied.</summary>
        public UnityEvent Damaged => damaged;

        /// <summary>Raised whenever health is restored.</summary>
        public UnityEvent Healed => healed;

        /// <summary>Raised once when health reaches zero.</summary>
        public UnityEvent Died => died;

        /// <summary>Raised with the damage payload whenever damage is applied.</summary>
        public event Action<DamageInfo> DamageTaken;

        /// <summary>Raised once when health reaches zero.</summary>
        public event Action Death;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        /// <inheritdoc />
        public bool ApplyDamage(in DamageInfo damage)
        {
            if (!IsAlive || IsInvulnerable || damage.Amount <= 0)
                return false;

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage.Amount);
            invulnerableUntil = Time.time + invulnerabilityDuration;

            healthChanged.Invoke(CurrentHealth, maxHealth);
            damaged.Invoke();
            DamageTaken?.Invoke(damage);

            if (CurrentHealth == 0)
                HandleDeath();

            return true;
        }

        /// <summary>
        /// Restores hit points, clamped to the maximum.
        /// </summary>
        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0 || CurrentHealth >= maxHealth)
                return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            healthChanged.Invoke(CurrentHealth, maxHealth);
            healed.Invoke();
        }

        /// <summary>
        /// Kills instantly, ignoring the invulnerability window.
        /// </summary>
        public void Kill()
        {
            if (!IsAlive)
                return;

            CurrentHealth = 0;
            healthChanged.Invoke(CurrentHealth, maxHealth);
            HandleDeath();
        }

        /// <summary>
        /// Restores full health, e.g. after a respawn.
        /// </summary>
        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            invulnerableUntil = 0f;
            healthChanged.Invoke(CurrentHealth, maxHealth);
        }

        /// <summary>
        /// Grants temporary invulnerability, e.g. right after a respawn.
        /// Extends the current window; never shortens it.
        /// </summary>
        public void GrantInvulnerability(float duration)
        {
            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + duration);
        }

        private void HandleDeath()
        {
            died.Invoke();
            Death?.Invoke();

            if (destroyOnDeath)
                Destroy(gameObject, destroyDelay);
        }
    }
}
