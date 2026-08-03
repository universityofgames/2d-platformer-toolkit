using System;
using UnityEngine;
using UnityEngine.Events;

namespace PlatformerToolkit.Characters
{
    /// <summary>
    /// Physics-based motor for 2D platformer characters. Handles horizontal
    /// movement with separate ground/air acceleration, jumping with variable
    /// height, gravity shaping for a snappy arc and robust ground detection.
    /// Feed it input from a controller (player or AI); it never reads input
    /// itself, which keeps it reusable for enemies, replays and cutscenes.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Characters/Character Motor 2D")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class CharacterMotor2D : MonoBehaviour
    {
        private const float InputDeadZone = 0.01f;
        private const float GroundedVerticalSpeedThreshold = 0.05f;
        private const float MinGroundNormalY = 0.5f;

        [Header("Run")]
        [Tooltip("Maximum horizontal speed, in world units per second.")]
        [SerializeField, Min(0f)] private float runSpeed = 8f;

        [Tooltip("Horizontal acceleration while grounded, in units per second squared.")]
        [SerializeField, Min(0f)] private float groundAcceleration = 90f;

        [Tooltip("Horizontal deceleration while grounded and no input is applied.")]
        [SerializeField, Min(0f)] private float groundDeceleration = 110f;

        [Tooltip("Horizontal acceleration while airborne.")]
        [SerializeField, Min(0f)] private float airAcceleration = 75f;

        [Tooltip("Horizontal deceleration while airborne and no input is applied.")]
        [SerializeField, Min(0f)] private float airDeceleration = 30f;

        [Tooltip("Acceleration used when reversing direction — higher values make turns feel snappy instead of slippery.")]
        [SerializeField, Min(0f)] private float turnAcceleration = 180f;

        [Header("Jump")]
        [Tooltip("Peak height of a full jump, in world units.")]
        [SerializeField, Min(0.1f)] private float jumpHeight = 3.3f;

        [Tooltip("Time to reach the peak of a full jump, in seconds. Together with Jump Height this defines the character's gravity — the motor drives the rigidbody's gravity scale from these two values, so the serialized gravity scale is ignored. Shorter times make the whole game feel snappier; the best platformers sit around 0.35-0.45.")]
        [SerializeField, Range(0.15f, 0.8f)] private float timeToApex = 0.38f;

        [Tooltip("Fraction of upward velocity kept when a jump is cut short (variable jump height).")]
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.4f;

        [Tooltip("Gravity multiplier applied while falling, for a snappier arc.")]
        [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.7f;

        [Tooltip("Maximum fall speed, in units per second.")]
        [SerializeField, Min(0f)] private float maxFallSpeed = 20f;

        [Header("Apex Assist")]
        [Tooltip("Vertical speed window around the jump apex where the assists below kick in.")]
        [SerializeField, Min(0f)] private float apexThreshold = 2.2f;

        [Tooltip("Gravity multiplier near the apex — values below 1 add hang time, giving the jump a floaty, controllable peak.")]
        [SerializeField, Range(0.1f, 1f)] private float apexGravityMultiplier = 0.55f;

        [Tooltip("Air acceleration multiplier near the apex, for precise mid-air corrections.")]
        [SerializeField, Min(1f)] private float apexControlMultiplier = 1.5f;

        [Header("Wall Interaction")]
        [Tooltip("Slide down walls at a limited speed while airborne and pressing toward them, enabling wall jumps.")]
        [SerializeField] private bool enableWallSlide = true;

        [Tooltip("Maximum slide speed down a wall, in units per second.")]
        [SerializeField, Min(0f)] private float wallSlideSpeed = 3f;

        [Tooltip("Peak height of a wall jump, in world units.")]
        [SerializeField, Min(0.1f)] private float wallJumpHeight = 3f;

        [Tooltip("Horizontal launch speed away from the wall.")]
        [SerializeField, Min(0f)] private float wallJumpPushSpeed = 9f;

        [Header("Corner Correction")]
        [Tooltip("When a rising jump clips a platform corner with the edge of the head, nudge the character around it instead of stopping the jump.")]
        [SerializeField] private bool enableCornerCorrection = true;

        [Tooltip("Maximum horizontal nudge, in world units.")]
        [SerializeField, Range(0.05f, 0.5f)] private float cornerCorrectionDistance = 0.25f;

        [Header("Dash")]
        [Tooltip("Enable the horizontal dash.")]
        [SerializeField] private bool enableDash = true;

        [Tooltip("Dash speed, in units per second.")]
        [SerializeField, Min(1f)] private float dashSpeed = 18f;

        [Tooltip("Dash duration, in seconds. Gravity is suspended for the whole dash.")]
        [SerializeField, Range(0.05f, 0.5f)] private float dashDuration = 0.15f;

        [Tooltip("Cooldown after a dash ends before the next one is allowed.")]
        [SerializeField, Min(0f)] private float dashCooldown = 0.4f;

        [Tooltip("Dashes allowed while airborne before landing again.")]
        [SerializeField, Min(0)] private int maxAirDashes = 1;

        [Header("Crouch")]
        [Tooltip("Movement speed multiplier while crouching.")]
        [SerializeField, Range(0f, 1f)] private float crouchSpeedMultiplier = 0.5f;

        [Tooltip("Fraction of the collider height kept while crouching. Requires a BoxCollider2D.")]
        [SerializeField, Range(0.3f, 0.9f)] private float crouchHeightFraction = 0.55f;

        [Header("Climbing")]
        [Tooltip("Vertical climb speed on ladders, in units per second.")]
        [SerializeField, Min(0.5f)] private float climbSpeed = 4f;

        [Header("Ground Detection")]
        [Tooltip("Layers treated as ground.")]
        [SerializeField] private LayerMask groundMask = 1;

        [Tooltip("Distance below the collider used to detect ground contact.")]
        [SerializeField, Range(0.01f, 0.3f)] private float groundCheckDistance = 0.06f;

        [Header("Collision")]
        [Tooltip("Assign a frictionless physics material at runtime so the character never sticks to walls while pressing against them. Disabled when the rigidbody already has a material.")]
        [SerializeField] private bool preventWallSticking = true;

        [Header("Facing")]
        [Tooltip("Flip the transform horizontally to face the current move direction.")]
        [SerializeField] private bool faceMoveDirection = true;

        [Header("Events")]
        [SerializeField] private UnityEvent landed = new UnityEvent();
        [SerializeField] private UnityEvent leftGround = new UnityEvent();
        [SerializeField] private UnityEvent jumped = new UnityEvent();
        [SerializeField] private UnityEvent dashed = new UnityEvent();

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];
        private ContactFilter2D groundFilter;
        private float dashEndsAt;
        private float dashReadyAt;
        private int dashDirection;
        private int airDashesUsed;
        private float climbCenterX;
        private Vector2 standingColliderSize;
        private Vector2 standingColliderOffset;

        /// <summary>
        /// Horizontal input in the -1..1 range. Set this every frame from a controller.
        /// </summary>
        public float MoveInput { get; set; }

        /// <summary>
        /// True while the character stands on ground.
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        /// Current rigidbody velocity.
        /// </summary>
        public Vector2 Velocity => body.linearVelocity;

        /// <summary>
        /// Horizontal facing: 1 for right, -1 for left.
        /// </summary>
        public int FacingDirection { get; private set; } = 1;

        /// <summary>
        /// Maximum horizontal speed, in world units per second.
        /// </summary>
        public float RunSpeed => runSpeed;

        /// <summary>
        /// Gravity magnitude (units per second squared) derived from
        /// <see cref="jumpHeight"/> and <see cref="timeToApex"/>. The motor
        /// drives the rigidbody's gravity scale from this value.
        /// </summary>
        public float BaseGravity => 2f * jumpHeight / (timeToApex * timeToApex);

        /// <summary>
        /// Direction of the wall touched while airborne: -1 left, 1 right, 0 none.
        /// </summary>
        public int WallDirection { get; private set; }

        /// <summary>
        /// True while sliding down a wall.
        /// </summary>
        public bool IsWallSliding { get; private set; }

        /// <summary>
        /// True during a dash.
        /// </summary>
        public bool IsDashing { get; private set; }

        /// <summary>
        /// True while crouching.
        /// </summary>
        public bool IsCrouching { get; private set; }

        /// <summary>
        /// True while climbing a ladder.
        /// </summary>
        public bool IsClimbing { get; private set; }

        /// <summary>
        /// Vertical input in the -1..1 range used while climbing.
        /// </summary>
        public float ClimbInput { get; set; }

        /// <summary>
        /// Raised when a dash starts.
        /// </summary>
        public UnityEvent Dashed => dashed;

        /// <summary>
        /// Raised when the character lands on ground.
        /// </summary>
        public UnityEvent Landed => landed;

        /// <summary>
        /// Raised when the character leaves the ground, by jumping or falling.
        /// </summary>
        public UnityEvent LeftGround => leftGround;

        /// <summary>
        /// Raised when a jump is performed through <see cref="Jump()"/>.
        /// </summary>
        public UnityEvent Jumped => jumped;

        /// <summary>
        /// Raised whenever <see cref="IsGrounded"/> changes.
        /// </summary>
        public event Action<bool> GroundedChanged;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            body.freezeRotation = true;

            // Without this, the default 0.4 friction lets the character hang on
            // walls: pressing into a wall while falling generates enough
            // friction to cancel gravity.
            if (preventWallSticking && body.sharedMaterial == null)
            {
                body.sharedMaterial = new PhysicsMaterial2D("Frictionless (runtime)")
                {
                    friction = 0f,
                    bounciness = 0f,
                };
            }

            groundFilter = new ContactFilter2D();
            groundFilter.SetLayerMask(groundMask);
            groundFilter.useTriggers = false;
        }

        private void FixedUpdate()
        {
            if (IsDashing && Time.time >= dashEndsAt)
                EndDash();

            UpdateGrounded();
            UpdateWallContact();

            if (IsClimbing)
            {
                ApplyClimb();
                return;
            }

            if (IsDashing)
            {
                ApplyDashVelocity();
                return;
            }

            ApplyHorizontalMovement();
            ApplyGravityShaping();
            ApplyWallSlide();
            ApplyCornerCorrection();
            UpdateFacing();
        }

        /// <summary>
        /// Performs a full-height jump.
        /// </summary>
        public void Jump()
        {
            Jump(jumpHeight);
        }

        /// <summary>
        /// Jumps to the given peak height, in world units.
        /// </summary>
        public void Jump(float height)
        {
            Launch(JumpSpeedForHeight(height));
            jumped.Invoke();
        }

        /// <summary>
        /// Launches the character upward to the given peak height without raising
        /// <see cref="Jumped"/>. Used by springs and stomp bounces.
        /// </summary>
        public void Bounce(float height)
        {
            Launch(JumpSpeedForHeight(height));
        }

        /// <summary>
        /// Cuts the current jump short by scaling down upward velocity.
        /// Call when the jump button is released for variable jump height.
        /// </summary>
        public void CutJump()
        {
            Vector2 velocity = body.linearVelocity;
            if (velocity.y > 0f)
            {
                velocity.y *= jumpCutMultiplier;
                body.linearVelocity = velocity;
            }
        }

        /// <summary>
        /// Overrides the current velocity. Useful for knockback and respawning.
        /// </summary>
        public void SetVelocity(Vector2 velocity)
        {
            body.linearVelocity = velocity;
        }

        /// <summary>
        /// Launches up and away from a wall. Pass the wall side:
        /// -1 when the wall is on the left, 1 when it is on the right.
        /// </summary>
        public void WallJump(int wallDirection)
        {
            Vector2 velocity = body.linearVelocity;
            velocity.x = -Mathf.Sign(wallDirection) * wallJumpPushSpeed;
            velocity.y = JumpSpeedForHeight(wallJumpHeight);
            body.linearVelocity = velocity;
            SetGrounded(false);
            jumped.Invoke();
        }

        /// <summary>
        /// Starts a dash in the facing direction. Returns false while dashing,
        /// on cooldown or when the air-dash budget is spent.
        /// </summary>
        public bool TryDash()
        {
            if (!enableDash || IsDashing || Time.time < dashReadyAt)
                return false;

            if (!IsGrounded && airDashesUsed >= maxAirDashes)
                return false;

            if (IsClimbing)
                StopClimb();
            if (IsCrouching)
                SetCrouching(false);

            if (!IsGrounded)
                airDashesUsed++;

            IsDashing = true;
            dashDirection = FacingDirection;
            dashEndsAt = Time.time + dashDuration;
            ApplyDashVelocity();
            dashed.Invoke();
            return true;
        }

        /// <summary>
        /// Crouches or stands up. Standing up is refused while a ceiling
        /// blocks it. Collider resizing requires a BoxCollider2D.
        /// </summary>
        public void SetCrouching(bool crouch)
        {
            if (IsCrouching == crouch)
                return;

            if (crouch && (!IsGrounded || IsDashing || IsClimbing))
                return;

            var box = bodyCollider as BoxCollider2D;
            if (crouch)
            {
                IsCrouching = true;
                if (box != null)
                {
                    standingColliderSize = box.size;
                    standingColliderOffset = box.offset;
                    float removed = box.size.y * (1f - crouchHeightFraction);
                    box.size = new Vector2(box.size.x, box.size.y - removed);
                    box.offset = new Vector2(box.offset.x, box.offset.y - removed * 0.5f);
                }
                return;
            }

            if (box != null)
            {
                // Refuse to stand while a ceiling is in the way.
                float clearance = standingColliderSize.y - box.size.y;
                if (body.Cast(Vector2.up, groundFilter, groundHits, clearance + 0.02f) > 0)
                    return;

                box.size = standingColliderSize;
                box.offset = standingColliderOffset;
            }

            IsCrouching = false;
        }

        /// <summary>
        /// Starts climbing, keeping the character horizontally centred on
        /// <paramref name="centerX"/>. Drive it with <see cref="ClimbInput"/>.
        /// </summary>
        public void StartClimb(float centerX)
        {
            if (IsDashing)
                EndDash();
            if (IsCrouching)
                SetCrouching(false);

            IsClimbing = true;
            climbCenterX = centerX;
        }

        /// <summary>
        /// Stops climbing and hands control back to regular movement.
        /// </summary>
        public void StopClimb()
        {
            IsClimbing = false;
        }

        /// <summary>
        /// Drops through one-way platforms directly below (platforms using a
        /// PlatformEffector2D). Returns true when at least one was found.
        /// </summary>
        public bool DropThroughPlatforms()
        {
            int hitCount = body.Cast(Vector2.down, groundFilter, groundHits, groundCheckDistance + 0.05f);
            bool any = false;
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D platform = groundHits[i].collider;
                if (platform != null && platform.GetComponent<PlatformEffector2D>() != null)
                {
                    StartCoroutine(IgnorePlatformRoutine(platform));
                    any = true;
                }
            }

            return any;
        }

        private System.Collections.IEnumerator IgnorePlatformRoutine(Collider2D platform)
        {
            Physics2D.IgnoreCollision(bodyCollider, platform, true);
            yield return new WaitForSeconds(0.35f);
            if (platform != null)
                Physics2D.IgnoreCollision(bodyCollider, platform, false);
        }

        private void ApplyDashVelocity()
        {
            body.gravityScale = 0f;
            body.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
        }

        private void EndDash()
        {
            IsDashing = false;
            dashReadyAt = Time.time + dashCooldown;

            Vector2 velocity = body.linearVelocity;
            velocity.x = Mathf.Clamp(velocity.x, -runSpeed, runSpeed);
            body.linearVelocity = velocity;
        }

        private void ApplyClimb()
        {
            body.gravityScale = 0f;

            float vertical = Mathf.Clamp(ClimbInput, -1f, 1f) * climbSpeed;
            float horizontal = Mathf.Clamp((climbCenterX - body.position.x) * 10f, -climbSpeed, climbSpeed);
            body.linearVelocity = new Vector2(horizontal, vertical);

            // Stepped down onto solid ground — let go of the ladder.
            if (IsGrounded && ClimbInput < -InputDeadZone)
                StopClimb();
        }

        private void Launch(float verticalSpeed)
        {
            IsClimbing = false;
            if (IsCrouching)
                SetCrouching(false);

            Vector2 velocity = body.linearVelocity;
            velocity.y = verticalSpeed;
            body.linearVelocity = velocity;
            SetGrounded(false);
        }

        private float JumpSpeedForHeight(float height)
        {
            return Mathf.Sqrt(2f * BaseGravity * Mathf.Max(0.01f, height));
        }

        private void UpdateGrounded()
        {
            bool grounded = false;

            if (body.linearVelocity.y <= GroundedVerticalSpeedThreshold)
            {
                int hitCount = body.Cast(Vector2.down, groundFilter, groundHits, groundCheckDistance);
                for (int i = 0; i < hitCount; i++)
                {
                    if (groundHits[i].normal.y > MinGroundNormalY)
                    {
                        grounded = true;
                        break;
                    }
                }
            }

            SetGrounded(grounded);
        }

        private void SetGrounded(bool grounded)
        {
            if (IsGrounded == grounded)
                return;

            IsGrounded = grounded;
            GroundedChanged?.Invoke(grounded);

            if (grounded)
            {
                airDashesUsed = 0;
                landed.Invoke();
            }
            else
            {
                leftGround.Invoke();
            }
        }

        private void ApplyHorizontalMovement()
        {
            float input = Mathf.Clamp(MoveInput, -1f, 1f);
            float targetSpeed = input * runSpeed;
            bool accelerating = Mathf.Abs(targetSpeed) > InputDeadZone;

            Vector2 velocity = body.linearVelocity;
            bool turning = accelerating && velocity.x * targetSpeed < -0.01f;

            if (IsCrouching)
                targetSpeed *= crouchSpeedMultiplier;

            float rate;
            if (turning)
                rate = turnAcceleration;
            else if (accelerating)
                rate = IsGrounded ? groundAcceleration : airAcceleration;
            else
                rate = IsGrounded ? groundDeceleration : airDeceleration;

            if (!IsGrounded && Mathf.Abs(velocity.y) < apexThreshold)
                rate *= apexControlMultiplier;

            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, rate * Time.fixedDeltaTime);
            body.linearVelocity = velocity;
        }

        private void ApplyGravityShaping()
        {
            Vector2 velocity = body.linearVelocity;
            float multiplier = 1f;

            if (!IsGrounded)
            {
                if (Mathf.Abs(velocity.y) < apexThreshold)
                    multiplier = apexGravityMultiplier;
                else if (velocity.y < 0f)
                    multiplier = fallGravityMultiplier;
            }

            float physicsGravity = Mathf.Max(0.01f, Mathf.Abs(Physics2D.gravity.y));
            body.gravityScale = BaseGravity * multiplier / physicsGravity;

            if (velocity.y < -maxFallSpeed)
            {
                velocity.y = -maxFallSpeed;
                body.linearVelocity = velocity;
            }
        }

        private void UpdateWallContact()
        {
            WallDirection = 0;
            if (IsGrounded)
                return;

            if (TouchesWall(-1))
                WallDirection = -1;
            else if (TouchesWall(1))
                WallDirection = 1;
        }

        private bool TouchesWall(int direction)
        {
            int hitCount = body.Cast(new Vector2(direction, 0f), groundFilter, groundHits, 0.05f);
            for (int i = 0; i < hitCount; i++)
            {
                if (Mathf.Abs(groundHits[i].normal.x) > 0.7f)
                    return true;
            }

            return false;
        }

        private void ApplyWallSlide()
        {
            IsWallSliding = false;
            if (!enableWallSlide || IsGrounded || WallDirection == 0)
                return;

            // Slide only while pressing toward the wall.
            if (Mathf.Clamp(MoveInput, -1f, 1f) * WallDirection < InputDeadZone)
                return;

            Vector2 velocity = body.linearVelocity;
            if (velocity.y >= 0f)
                return;

            IsWallSliding = true;
            if (velocity.y < -wallSlideSpeed)
            {
                velocity.y = -wallSlideSpeed;
                body.linearVelocity = velocity;
            }
        }

        private void ApplyCornerCorrection()
        {
            if (!enableCornerCorrection || IsGrounded)
                return;

            Vector2 velocity = body.linearVelocity;
            if (velocity.y <= 0f)
                return;

            Bounds bounds = bodyCollider.bounds;
            float top = bounds.max.y;
            float castDistance = Mathf.Max(0.05f, velocity.y * Time.fixedDeltaTime * 2f);

            // A blocked head centre means a genuine ceiling — leave it alone.
            if (Physics2D.Raycast(new Vector2(bounds.center.x, top), Vector2.up, castDistance, groundMask))
                return;

            bool leftBlocked = Physics2D.Raycast(new Vector2(bounds.min.x, top), Vector2.up, castDistance, groundMask);
            bool rightBlocked = Physics2D.Raycast(new Vector2(bounds.max.x, top), Vector2.up, castDistance, groundMask);
            if (leftBlocked == rightBlocked)
                return;

            float direction = leftBlocked ? 1f : -1f;
            float blockedX = leftBlocked ? bounds.min.x : bounds.max.x;

            // Find the smallest sideways nudge that lets the jump continue.
            for (float shift = 0.05f; shift <= cornerCorrectionDistance; shift += 0.05f)
            {
                var probe = new Vector2(blockedX + direction * shift, top);
                if (!Physics2D.Raycast(probe, Vector2.up, castDistance, groundMask))
                {
                    body.position += new Vector2(direction * (shift + 0.02f), 0f);
                    return;
                }
            }
        }

        private void UpdateFacing()
        {
            if (Mathf.Abs(MoveInput) < InputDeadZone)
                return;

            int direction = MoveInput > 0f ? 1 : -1;
            if (direction == FacingDirection)
                return;

            FacingDirection = direction;
            if (!faceMoveDirection)
                return;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }

        private void OnValidate()
        {
            if (Application.isPlaying && body != null)
                groundFilter.SetLayerMask(groundMask);
        }
    }
}
