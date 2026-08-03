using UnityEngine;
using UnityEngine.Events;
using PlatformerToolkit.Characters;

namespace PlatformerToolkit.Combat
{
    /// <summary>
    /// Spawns <see cref="Projectile"/>s along the fire point's right axis —
    /// rotate the fire point (or this object) to aim. Fires automatically at
    /// an interval, optionally only while a player character is in range, or
    /// manually through <see cref="Fire"/> (also callable from UnityEvents).
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Combat/Projectile Launcher")]
    [DisallowMultipleComponent]
    public sealed class ProjectileLauncher : MonoBehaviour
    {
        [Tooltip("Projectile prefab to spawn.")]
        [SerializeField] private Projectile projectilePrefab;

        [Tooltip("Spawn point and direction (its right axis). Defaults to this transform.")]
        [SerializeField] private Transform firePoint;

        [Header("Auto Fire")]
        [Tooltip("Fire automatically at the interval below.")]
        [SerializeField] private bool autoFire = true;

        [Tooltip("Seconds between automatic shots.")]
        [SerializeField, Min(0.1f)] private float fireInterval = 2f;

        [Tooltip("Fire only while a player character is within this range. Zero fires always.")]
        [SerializeField, Min(0f)] private float activationRange;

        [SerializeField] private UnityEvent fired = new UnityEvent();

        private CharacterMotor2D playerMotor;
        private float nextFireAt;

        /// <summary>
        /// Raised for every spawned projectile.
        /// </summary>
        public UnityEvent Fired => fired;

        private void Awake()
        {
            if (firePoint == null)
                firePoint = transform;
        }

        private void Update()
        {
            if (!autoFire || Time.time < nextFireAt || !TargetInRange())
                return;

            Fire();
        }

        /// <summary>
        /// Spawns a single projectile. Safe to wire into UnityEvents.
        /// </summary>
        public void Fire()
        {
            if (projectilePrefab == null)
                return;

            Projectile projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            projectile.Launch(firePoint.right, gameObject);
            nextFireAt = Time.time + fireInterval;
            fired.Invoke();
        }

        private bool TargetInRange()
        {
            if (activationRange <= 0f)
                return true;

            if (playerMotor == null)
            {
                playerMotor = FindAnyObjectByType<CharacterMotor2D>();
                if (playerMotor == null)
                    return false;
            }

            return Vector2.Distance(playerMotor.transform.position, firePoint.position) <= activationRange;
        }

        private void OnDrawGizmosSelected()
        {
            Transform point = firePoint != null ? firePoint : transform;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(point.position, point.position + point.right * 1.5f);
            if (activationRange > 0f)
                Gizmos.DrawWireSphere(point.position, activationRange);
        }
    }
}
