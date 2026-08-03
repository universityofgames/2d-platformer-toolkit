using UnityEngine;

namespace PlatformerToolkit.Characters
{
    /// <summary>
    /// Adds cartoon squash and stretch to a character driven by a
    /// <see cref="CharacterMotor2D"/>: the sprite stretches when launching off
    /// the ground and squashes on landing (scaled by impact speed), then
    /// springs back to its normal shape. Deformation is volume-preserving and
    /// composed with the motor's facing direction every frame.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Characters/Character Squash Stretch")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterMotor2D))]
    public sealed class CharacterSquashStretch : MonoBehaviour
    {
        private const float MinLaunchSpeed = 0.5f;

        [Tooltip("Transform to scale, ideally a child holding the sprite. Defaults to this transform.")]
        [SerializeField] private Transform visuals;

        [Tooltip("Vertical stretch applied when launching off the ground (jump, spring, stomp bounce).")]
        [SerializeField, Range(1f, 1.6f)] private float jumpStretch = 1.18f;

        [Tooltip("Vertical squash applied when landing at full impact speed.")]
        [SerializeField, Range(0.4f, 1f)] private float landSquash = 0.72f;

        [Tooltip("Fall speed that produces the strongest landing squash.")]
        [SerializeField, Min(0.1f)] private float maxImpactSpeed = 20f;

        [Tooltip("How quickly the sprite springs back to its normal shape.")]
        [SerializeField, Min(0.1f)] private float recoverySpeed = 9f;

        private CharacterMotor2D motor;
        private Vector3 baseScale;
        private float currentStretch = 1f;
        private float fallSpeed;

        private void Awake()
        {
            motor = GetComponent<CharacterMotor2D>();
            if (visuals == null)
                visuals = transform;

            baseScale = new Vector3(
                Mathf.Abs(visuals.localScale.x),
                Mathf.Abs(visuals.localScale.y),
                visuals.localScale.z);
        }

        private void OnEnable()
        {
            motor.Landed.AddListener(HandleLanded);
            motor.LeftGround.AddListener(HandleLeftGround);
            motor.Jumped.AddListener(HandleLeftGround);
        }

        private void OnDisable()
        {
            motor.Landed.RemoveListener(HandleLanded);
            motor.LeftGround.RemoveListener(HandleLeftGround);
            motor.Jumped.RemoveListener(HandleLeftGround);
            currentStretch = 1f;
            ApplyScale();
        }

        private void FixedUpdate()
        {
            if (!motor.IsGrounded)
                fallSpeed = Mathf.Max(0f, -motor.Velocity.y);
        }

        private void LateUpdate()
        {
            currentStretch = Mathf.Lerp(
                currentStretch, 1f, 1f - Mathf.Exp(-recoverySpeed * Time.deltaTime));
            ApplyScale();
        }

        private void HandleLanded()
        {
            float impact = Mathf.Clamp01(fallSpeed / maxImpactSpeed);
            currentStretch = Mathf.Lerp(1f, landSquash, impact);
            fallSpeed = 0f;
        }

        private void HandleLeftGround()
        {
            if (motor.Velocity.y > MinLaunchSpeed)
                currentStretch = jumpStretch;
        }

        private void ApplyScale()
        {
            // Volume-preserving: the width compensates the height change.
            float width = 1f / Mathf.Sqrt(Mathf.Max(0.01f, currentStretch));
            visuals.localScale = new Vector3(
                baseScale.x * width * motor.FacingDirection,
                baseScale.y * currentStretch,
                baseScale.z);
        }
    }
}
