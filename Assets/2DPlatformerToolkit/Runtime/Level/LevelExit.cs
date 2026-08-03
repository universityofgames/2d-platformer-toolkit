using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using PlatformerToolkit.Characters;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Level
{
    /// <summary>
    /// End-of-level trigger. Raises an event and optionally loads the next scene.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Level/Level Exit")]
    [RequireComponent(typeof(Collider2D))]
    public sealed class LevelExit : MonoBehaviour
    {
        [Tooltip("Layers that can finish the level.")]
        [SerializeField] private LayerMask activatorLayers = ~0;

        [Tooltip("Scene to load when the exit is reached. Leave empty to only raise the event.")]
        [SerializeField] private string nextSceneName = string.Empty;

        [Tooltip("Delay before the scene load, leaving time for effects.")]
        [SerializeField, Min(0f)] private float loadDelay = 0.5f;

        [SerializeField] private UnityEvent exitReached = new UnityEvent();

        private bool consumed;

        /// <summary>
        /// Raised once when the player reaches the exit.
        /// </summary>
        public UnityEvent ExitReached => exitReached;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed || !activatorLayers.Contains(other.gameObject.layer))
                return;

            if (other.GetComponentInParent<PlayerController>() == null)
                return;

            consumed = true;
            exitReached.Invoke();

            if (!string.IsNullOrEmpty(nextSceneName))
                StartCoroutine(LoadNextScene());
        }

        private IEnumerator LoadNextScene()
        {
            yield return new WaitForSeconds(loadDelay);
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
