using UnityEngine;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Collectibles
{
    /// <summary>
    /// Key pick-up. Keys are stored in the <see cref="GameSession"/> and
    /// consumed by <see cref="Level.LockedDoor"/>s.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Collectibles/Key Pickup")]
    public sealed class KeyPickup : Collectible
    {
        protected override bool OnCollected(GameObject collector)
        {
            GameSession.Instance.AddKeys(1);
            return true;
        }
    }
}
