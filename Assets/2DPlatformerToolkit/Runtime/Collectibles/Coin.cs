using UnityEngine;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Collectibles
{
    /// <summary>
    /// Currency pick-up that adds its value to the <see cref="GameSession"/> coin total.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Collectibles/Coin")]
    public sealed class Coin : Collectible
    {
        [Tooltip("Coins added to the session total.")]
        [SerializeField, Min(1)] private int value = 1;

        protected override bool OnCollected(GameObject collector)
        {
            GameSession.Instance.AddCoins(value);
            return true;
        }
    }
}
