using UnityEngine;

namespace PlatformerToolkit.Core
{
    /// <summary>
    /// Convenience extensions for <see cref="LayerMask"/>.
    /// </summary>
    public static class LayerMaskExtensions
    {
        /// <summary>
        /// Returns true when the given layer index is enabled in the mask.
        /// </summary>
        public static bool Contains(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
