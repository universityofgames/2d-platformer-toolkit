using UnityEngine;
using PlatformerToolkit.Core;

namespace PlatformerToolkit.Combat
{
    /// <summary>
    /// Trigger volume that instantly kills anything with a <see cref="Health"/>
    /// component entering it. Use for pits, lava and level boundaries.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Combat/Kill Zone")]
    [RequireComponent(typeof(Collider2D))]
    public sealed class KillZone : MonoBehaviour
    {
        [Tooltip("Layers affected by this zone.")]
        [SerializeField] private LayerMask affectedLayers = ~0;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!affectedLayers.Contains(other.gameObject.layer))
                return;

            Health health = other.GetComponentInParent<Health>();
            if (health != null)
                health.Kill();
        }
    }
}
