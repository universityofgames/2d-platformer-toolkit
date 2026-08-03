using UnityEngine;

namespace PlatformerToolkit.Combat
{
    /// <summary>
    /// Payload describing a single instance of damage.
    /// </summary>
    public readonly struct DamageInfo
    {
        /// <summary>Amount of health to remove.</summary>
        public readonly int Amount;

        /// <summary>World position the damage originated from.</summary>
        public readonly Vector2 SourcePosition;

        /// <summary>Object that dealt the damage. May be null.</summary>
        public readonly GameObject Source;

        public DamageInfo(int amount, Vector2 sourcePosition, GameObject source = null)
        {
            Amount = amount;
            SourcePosition = sourcePosition;
            Source = source;
        }
    }

    /// <summary>
    /// Implemented by anything that can receive damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies damage. Returns true when the damage was actually applied,
        /// false when it was rejected (dead, invulnerable, zero amount).
        /// </summary>
        bool ApplyDamage(in DamageInfo damage);
    }
}
