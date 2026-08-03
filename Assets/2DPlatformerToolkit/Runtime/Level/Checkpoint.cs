using System;
using UnityEngine;
using UnityEngine.Events;
using PlatformerToolkit.Characters;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Level
{
    /// <summary>
    /// Respawn point activated when the player touches it. The most recently
    /// activated checkpoint is exposed through <see cref="Active"/> and used
    /// by <see cref="Characters.PlayerRespawner"/>.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Level/Checkpoint")]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Checkpoint : MonoBehaviour
    {
        [Tooltip("Layers that can activate this checkpoint.")]
        [SerializeField] private LayerMask activatorLayers = ~0;

        [Tooltip("Spawn offset relative to the checkpoint position.")]
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        [Header("Events")]
        [SerializeField] private UnityEvent activated = new UnityEvent();
        [SerializeField] private UnityEvent deactivated = new UnityEvent();

        /// <summary>
        /// The most recently activated checkpoint, or null when none was reached yet.
        /// </summary>
        public static Checkpoint Active { get; private set; }

        /// <summary>
        /// Raised when any checkpoint becomes the active one.
        /// </summary>
        public static event Action<Checkpoint> CheckpointActivated;

        /// <summary>
        /// World position characters respawn at.
        /// </summary>
        public Vector2 RespawnPosition => (Vector2)transform.position + spawnOffset;

        /// <summary>Raised when this checkpoint becomes active.</summary>
        public UnityEvent Activated => activated;

        /// <summary>Raised when another checkpoint takes over.</summary>
        public UnityEvent Deactivated => deactivated;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!activatorLayers.Contains(other.gameObject.layer))
                return;

            if (other.GetComponentInParent<CharacterMotor2D>() == null)
                return;

            Activate();
        }

        /// <summary>
        /// Makes this the active checkpoint.
        /// </summary>
        public void Activate()
        {
            if (Active == this)
                return;

            if (Active != null)
                Active.deactivated.Invoke();

            Active = this;
            activated.Invoke();
            CheckpointActivated?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (Active == this)
                Active = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Active == this ? Color.green : new Color(1f, 0.6f, 0f);
            Gizmos.DrawWireSphere(RespawnPosition, 0.3f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Active = null;
            CheckpointActivated = null;
        }
    }
}
