using UnityEngine;
using UnityEngine.Events;
using PlatformerToolkit.Characters;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Level
{
    /// <summary>
    /// Solid door that opens when a player character touches it while the
    /// session holds a key (collected via <see cref="Collectibles.KeyPickup"/>).
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Level/Locked Door")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class LockedDoor : MonoBehaviour
    {
        [Tooltip("Destroy the door when opened, instead of just disabling it.")]
        [SerializeField] private bool destroyOnOpen;

        [SerializeField] private UnityEvent opened = new UnityEvent();

        private bool isOpen;

        /// <summary>
        /// Raised once when the door opens.
        /// </summary>
        public UnityEvent Opened => opened;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryOpen(collision.collider);
        }

        private void TryOpen(Collider2D other)
        {
            if (isOpen || other.GetComponentInParent<CharacterMotor2D>() == null)
                return;

            if (!GameSession.Instance.TryUseKey())
                return;

            isOpen = true;
            opened.Invoke();

            if (destroyOnOpen)
            {
                Destroy(gameObject);
                return;
            }

            foreach (Collider2D doorCollider in GetComponents<Collider2D>())
                doorCollider.enabled = false;
            foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
                spriteRenderer.enabled = false;
        }
    }
}
