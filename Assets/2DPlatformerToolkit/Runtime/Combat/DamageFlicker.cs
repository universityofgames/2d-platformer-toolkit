using UnityEngine;

namespace PlatformerToolkit.Combat
{
    /// <summary>
    /// Classic invulnerability flicker: while the attached <see cref="Health"/>
    /// is inside its invulnerability window — after a hit or a respawn — the
    /// sprites blink, so the player can read exactly when they are safe and
    /// when they can be hit again.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Combat/Damage Flicker")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class DamageFlicker : MonoBehaviour
    {
        [Tooltip("Renderers to flicker. Defaults to all sprite renderers on this object and its children.")]
        [SerializeField] private SpriteRenderer[] renderers;

        [Tooltip("Blinks per second during the invulnerability window.")]
        [SerializeField, Min(1f)] private float flickerFrequency = 12f;

        [Tooltip("Sprite alpha in the dimmed half of each blink.")]
        [SerializeField, Range(0f, 1f)] private float dimmedAlpha = 0.25f;

        private Health health;
        private bool dimmed;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            bool shouldDim = health.IsAlive && health.IsInvulnerable
                && Mathf.FloorToInt(Time.unscaledTime * flickerFrequency) % 2 == 0;

            if (shouldDim == dimmed)
                return;

            dimmed = shouldDim;
            SetAlpha(shouldDim ? dimmedAlpha : 1f);
        }

        private void OnDisable()
        {
            dimmed = false;
            SetAlpha(1f);
        }

        private void SetAlpha(float alpha)
        {
            foreach (SpriteRenderer spriteRenderer in renderers)
            {
                if (spriteRenderer == null)
                    continue;

                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }
    }
}
