using UnityEngine;
using UnityEngine.Events;
using PlatformerToolkit.Characters;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Collectibles
{
    /// <summary>
    /// Base class for pick-ups. Handles trigger detection, pickup feedback
    /// (sound, effect, event) and removal. Derive from it and implement
    /// <see cref="OnCollected"/> to grant the reward.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class Collectible : MonoBehaviour
    {
        [Header("Filtering")]
        [Tooltip("Layers allowed to collect this pick-up.")]
        [SerializeField] private LayerMask collectorLayers = ~0;

        [Tooltip("Only objects with a CharacterMotor2D can collect this pick-up.")]
        [SerializeField] private bool charactersOnly = true;

        [Header("Feedback")]
        [Tooltip("Sound played at the pick-up position.")]
        [SerializeField] private AudioClip pickupSound;

        [Tooltip("Effect prefab spawned at the pick-up position.")]
        [SerializeField] private GameObject pickupEffect;

        [SerializeField] private UnityEvent collected = new UnityEvent();

        /// <summary>
        /// Raised after a successful pick-up, just before the object is destroyed.
        /// </summary>
        public UnityEvent Collected => collected;

        protected virtual void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!collectorLayers.Contains(other.gameObject.layer))
                return;

            if (charactersOnly && other.GetComponentInParent<CharacterMotor2D>() == null)
                return;

            if (!OnCollected(other.gameObject))
                return;

            PlayFeedback();
            collected.Invoke();
            Destroy(gameObject);
        }

        /// <summary>
        /// Grants the reward. Return false to reject the pick-up,
        /// e.g. when health is already full.
        /// </summary>
        protected abstract bool OnCollected(GameObject collector);

        private void PlayFeedback()
        {
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
    }
}
