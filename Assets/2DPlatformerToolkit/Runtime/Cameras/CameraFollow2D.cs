using UnityEngine;

namespace PlatformerToolkit.Cameras
{
    /// <summary>
    /// Smooth 2D follow camera with look-ahead and optional world bounds.
    /// A lightweight alternative to Cinemachine for simple projects.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Cameras/Camera Follow 2D")]
    [DisallowMultipleComponent]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [Tooltip("Transform to follow, usually the player.")]
        [SerializeField] private Transform target;

        [Tooltip("Approximate time to catch up with the target, in seconds.")]
        [SerializeField, Range(0.01f, 1f)] private float smoothTime = 0.15f;

        [Tooltip("Offset applied to the target position.")]
        [SerializeField] private Vector2 offset = Vector2.zero;

        [Header("Look-Ahead")]
        [Tooltip("How far the camera looks ahead in the target's move direction.")]
        [SerializeField, Min(0f)] private float lookAheadDistance = 1f;

        [Tooltip("How quickly the look-ahead shifts, in units per second.")]
        [SerializeField, Min(0.01f)] private float lookAheadSpeed = 4f;

        [Header("Bounds")]
        [Tooltip("Clamp the camera view inside the world bounds below.")]
        [SerializeField] private bool useBounds;

        [SerializeField] private Vector2 boundsMin = new Vector2(-10f, -10f);
        [SerializeField] private Vector2 boundsMax = new Vector2(10f, 10f);

        private Camera attachedCamera;
        private Vector3 smoothVelocity;
        private float currentLookAhead;
        private Vector3 previousTargetPosition;

        /// <summary>
        /// Transform followed by the camera.
        /// </summary>
        public Transform Target
        {
            get => target;
            set
            {
                target = value;
                if (value != null)
                    previousTargetPosition = value.position;
            }
        }

        private void Awake()
        {
            attachedCamera = GetComponent<Camera>();
            if (target != null)
                previousTargetPosition = target.position;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            float targetDeltaX = target.position.x - previousTargetPosition.x;
            previousTargetPosition = target.position;

            float desiredLookAhead = 0f;
            if (Mathf.Abs(targetDeltaX) > 0.001f)
                desiredLookAhead = Mathf.Sign(targetDeltaX) * lookAheadDistance;

            currentLookAhead = Mathf.MoveTowards(currentLookAhead, desiredLookAhead, lookAheadSpeed * Time.deltaTime);

            var desired = new Vector3(
                target.position.x + offset.x + currentLookAhead,
                target.position.y + offset.y,
                transform.position.z);

            Vector3 next = Vector3.SmoothDamp(transform.position, desired, ref smoothVelocity, smoothTime);

            if (useBounds)
                next = ClampToBounds(next);

            transform.position = next;
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            float halfHeight = 0f;
            float halfWidth = 0f;

            if (attachedCamera != null && attachedCamera.orthographic)
            {
                halfHeight = attachedCamera.orthographicSize;
                halfWidth = halfHeight * attachedCamera.aspect;
            }

            position.x = Mathf.Clamp(position.x, boundsMin.x + halfWidth, Mathf.Max(boundsMin.x + halfWidth, boundsMax.x - halfWidth));
            position.y = Mathf.Clamp(position.y, boundsMin.y + halfHeight, Mathf.Max(boundsMin.y + halfHeight, boundsMax.y - halfHeight));
            return position;
        }

        private void OnDrawGizmosSelected()
        {
            if (!useBounds)
                return;

            Gizmos.color = Color.yellow;
            var center = new Vector3((boundsMin.x + boundsMax.x) * 0.5f, (boundsMin.y + boundsMax.y) * 0.5f, 0f);
            var size = new Vector3(boundsMax.x - boundsMin.x, boundsMax.y - boundsMin.y, 0f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
