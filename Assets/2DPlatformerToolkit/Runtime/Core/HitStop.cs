using UnityEngine;

namespace PlatformerToolkit.Core
{
    /// <summary>
    /// Tiny global freeze-frames ("hit stop") for impact feedback. Call
    /// <see cref="Request"/> with a short duration (around 0.05 seconds);
    /// overlapping requests extend the freeze instead of stacking.
    /// </summary>
    public static class HitStop
    {
        private static HitStopRunner runner;

        /// <summary>
        /// Freezes gameplay time for the given duration, in unscaled seconds.
        /// </summary>
        public static void Request(float duration)
        {
            if (duration <= 0f || !Application.isPlaying)
                return;

            if (runner == null)
            {
                var runnerObject = new GameObject("HitStop")
                {
                    hideFlags = HideFlags.HideInHierarchy,
                };
                Object.DontDestroyOnLoad(runnerObject);
                runner = runnerObject.AddComponent<HitStopRunner>();
            }

            runner.Extend(duration);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            runner = null;
        }

        [AddComponentMenu("")]
        private sealed class HitStopRunner : MonoBehaviour
        {
            private float resumeAt;
            private float previousTimeScale = 1f;
            private bool frozen;

            public void Extend(float duration)
            {
                resumeAt = Mathf.Max(resumeAt, Time.unscaledTime + duration);
                if (frozen)
                    return;

                previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
                Time.timeScale = 0f;
                frozen = true;
            }

            private void Update()
            {
                // Never fight a menu pause — resume once the game unpauses.
                if (GamePauser.IsPaused)
                    return;

                if (frozen && Time.unscaledTime >= resumeAt)
                {
                    Time.timeScale = previousTimeScale;
                    frozen = false;
                }
            }
        }
    }
}
