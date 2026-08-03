using UnityEngine;
using PlatformerToolkit.Combat;

namespace PlatformerToolkit.Collectibles
{
    /// <summary>
    /// Heart pick-up that restores health. Rejected when the collector
    /// is already at full health, so it stays available.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Collectibles/Health Pickup")]
    public sealed class HealthPickup : Collectible
    {
        [Tooltip("Hit points restored.")]
        [SerializeField, Min(1)] private int amount = 1;

        protected override bool OnCollected(GameObject collector)
        {
            Health health = collector.GetComponentInParent<Health>();
            if (health == null || !health.IsAlive || health.CurrentHealth >= health.MaxHealth)
                return false;

            health.Heal(amount);
            return true;
        }
    }
}
