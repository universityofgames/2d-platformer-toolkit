using UnityEngine;

namespace PlatformerToolkit.Level
{
    /// <summary>
    /// Climbable zone. Characters entering the trigger can climb it with
    /// vertical input; the <see cref="Characters.PlayerController"/> handles
    /// the input side automatically.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Level/Ladder")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Ladder : MonoBehaviour
    {
        private Collider2D zone;

        /// <summary>
        /// World-space X coordinate climbers are kept centred on.
        /// </summary>
        public float ClimbCenterX
        {
            get
            {
                if (zone == null)
                    zone = GetComponent<Collider2D>();
                return zone.bounds.center.x;
            }
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnDrawGizmosSelected()
        {
            Collider2D collider2d = GetComponent<Collider2D>();
            if (collider2d == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(collider2d.bounds.center, collider2d.bounds.size);
        }
    }
}
