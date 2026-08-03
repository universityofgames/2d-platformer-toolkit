using UnityEngine;
using UnityEngine.Events;

namespace PlatformerToolkit.Core
{
    /// <summary>
    /// Pauses and resumes gameplay time. Toggle it with the configured button
    /// or call <see cref="Pause"/>/<see cref="Resume"/> from UnityEvents and
    /// hook menu UI into the events below.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Core/Game Pauser")]
    [DisallowMultipleComponent]
    public sealed class GamePauser : MonoBehaviour
    {
        [Tooltip("Input Manager button toggling the pause. Leave empty to only pause from code or events.")]
        [SerializeField] private string pauseButton = "Cancel";

        [Header("Events")]
        [SerializeField] private UnityEvent paused = new UnityEvent();
        [SerializeField] private UnityEvent resumed = new UnityEvent();

        private float previousTimeScale = 1f;

        /// <summary>
        /// True while the game is paused by this component.
        /// </summary>
        public static bool IsPaused { get; private set; }

        /// <summary>Raised when the game pauses.</summary>
        public UnityEvent Paused => paused;

        /// <summary>Raised when the game resumes.</summary>
        public UnityEvent Resumed => resumed;

        private void Update()
        {
            if (!string.IsNullOrEmpty(pauseButton) && Input.GetButtonDown(pauseButton))
                Toggle();
        }

        /// <summary>Toggles between paused and running.</summary>
        public void Toggle()
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }

        /// <summary>Freezes gameplay time.</summary>
        public void Pause()
        {
            if (IsPaused)
                return;

            previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            IsPaused = true;
            paused.Invoke();
        }

        /// <summary>Restores gameplay time.</summary>
        public void Resume()
        {
            if (!IsPaused)
                return;

            Time.timeScale = previousTimeScale;
            IsPaused = false;
            resumed.Invoke();
        }

        private void OnDestroy()
        {
            if (IsPaused)
                Resume();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsPaused = false;
        }
    }
}
