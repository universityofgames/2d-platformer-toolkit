using UnityEngine;
using UnityEngine.UI;
using PlatformerToolkit.Combat;

namespace PlatformerToolkit.UI
{
    /// <summary>
    /// Classic heart-based health display. Assign one image per hit point plus
    /// full/empty sprites; the display then tracks a <see cref="Health"/> component.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/UI/Health Display UI")]
    [DisallowMultipleComponent]
    public sealed class HealthDisplayUI : MonoBehaviour
    {
        [Tooltip("Health to display, e.g. the player's.")]
        [SerializeField] private Health target;

        [Tooltip("One image per hit point, ordered left to right.")]
        [SerializeField] private Image[] icons = new Image[0];

        [Tooltip("Sprite shown for a remaining hit point.")]
        [SerializeField] private Sprite fullSprite;

        [Tooltip("Sprite shown for a lost hit point.")]
        [SerializeField] private Sprite emptySprite;

        [Tooltip("Hide icons beyond the current maximum health.")]
        [SerializeField] private bool hideExcessIcons = true;

        private void OnEnable()
        {
            if (target == null)
                return;

            target.HealthChanged.AddListener(HandleHealthChanged);
            HandleHealthChanged(target.CurrentHealth, target.MaxHealth);
        }

        private void Start()
        {
            // Awake/OnEnable order between objects is undefined during scene
            // load, so OnEnable may have read the health before it was
            // initialised. Re-sync once everything has awoken.
            if (target != null)
                HandleHealthChanged(target.CurrentHealth, target.MaxHealth);
        }

        private void OnDisable()
        {
            if (target != null)
                target.HealthChanged.RemoveListener(HandleHealthChanged);
        }

        private void HandleHealthChanged(int current, int max)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                Image icon = icons[i];
                if (icon == null)
                    continue;

                icon.enabled = i < max || !hideExcessIcons;
                icon.sprite = i < current ? fullSprite : emptySprite;
            }
        }
    }
}
