using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using PlatformerToolkit.Combat;
using PlatformerToolkit.Level;

namespace PlatformerToolkit.Characters
{
    /// <summary>
    /// Respawns the player at the last activated <see cref="Checkpoint"/> — or at
    /// the starting position when none was reached — after death. Optionally
    /// kills the player when falling below a world-space Y limit.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Characters/Player Respawner")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class PlayerRespawner : MonoBehaviour
    {
        [Tooltip("Seconds between death and respawn.")]
        [SerializeField, Min(0f)] private float respawnDelay = 0.75f;

        [Tooltip("Invulnerability granted right after a respawn, so the player is not hit again instantly.")]
        [SerializeField, Min(0f)] private float respawnInvulnerability = 1.5f;

        [Header("Fall Limit")]
        [Tooltip("Kill the player when falling below the fall limit.")]
        [SerializeField] private bool useFallLimit = true;

        [Tooltip("World-space Y coordinate below which the player dies.")]
        [SerializeField] private float fallLimitY = -15f;

        [Header("Events")]
        [SerializeField] private UnityEvent respawned = new UnityEvent();

        private Health health;
        private CharacterMotor2D motor;
        private PlayerController controller;
        private Rigidbody2D body;
        private Vector2 initialSpawnPosition;

        /// <summary>
        /// Raised right after the player is placed back at the spawn point.
        /// </summary>
        public UnityEvent Respawned => respawned;

        private void Awake()
        {
            health = GetComponent<Health>();
            motor = GetComponent<CharacterMotor2D>();
            controller = GetComponent<PlayerController>();
            body = GetComponent<Rigidbody2D>();
            initialSpawnPosition = transform.position;
        }

        private void OnEnable()
        {
            health.Death += HandleDeath;
        }

        private void OnDisable()
        {
            health.Death -= HandleDeath;
        }

        private void Update()
        {
            if (useFallLimit && health.IsAlive && transform.position.y < fallLimitY)
                health.Kill();
        }

        private void HandleDeath()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            if (controller != null)
                controller.InputEnabled = false;

            if (motor != null)
            {
                motor.MoveInput = 0f;
                motor.SetVelocity(Vector2.zero);
            }

            yield return new WaitForSeconds(respawnDelay);

            Vector2 spawnPosition = Checkpoint.Active != null
                ? Checkpoint.Active.RespawnPosition
                : initialSpawnPosition;

            // Teleport with interpolation suspended: an interpolated rigidbody
            // renders a sweep between the old and new pose for a frame,
            // which shows the character in both places at once.
            var previousInterpolation = RigidbodyInterpolation2D.None;
            if (body != null)
            {
                previousInterpolation = body.interpolation;
                body.interpolation = RigidbodyInterpolation2D.None;
                body.linearVelocity = Vector2.zero;
                body.position = spawnPosition;
            }

            transform.position = spawnPosition;

            health.ResetHealth();
            health.GrantInvulnerability(respawnInvulnerability);

            if (controller != null)
                controller.InputEnabled = true;

            respawned.Invoke();

            if (body != null)
            {
                // Restore after a physics step, once the interpolation buffer
                // has been re-seeded at the new position.
                yield return new WaitForFixedUpdate();
                body.interpolation = previousInterpolation;
            }
        }
    }
}
