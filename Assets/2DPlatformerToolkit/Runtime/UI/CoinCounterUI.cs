using TMPro;
using UnityEngine;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.UI
{
    /// <summary>
    /// Displays the session coin total on a TextMeshPro label.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/UI/Coin Counter UI")]
    [DisallowMultipleComponent]
    public sealed class CoinCounterUI : MonoBehaviour
    {
        [Tooltip("Label receiving the coin total. Defaults to a TMP_Text on this object.")]
        [SerializeField] private TMP_Text label;

        [Tooltip("Display format; {0} is replaced with the coin total.")]
        [SerializeField] private string format = "{0}";

        private void Awake()
        {
            if (label == null)
                label = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            GameSession.CoinsChanged += HandleCoinsChanged;
            HandleCoinsChanged(GameSession.Instance.Coins);
        }

        private void OnDisable()
        {
            GameSession.CoinsChanged -= HandleCoinsChanged;
        }

        private void HandleCoinsChanged(int total)
        {
            if (label != null)
                label.text = string.Format(format, total);
        }
    }
}
