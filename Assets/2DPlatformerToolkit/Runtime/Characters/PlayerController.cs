using UnityEngine;
using PlatformerToolkit.Combat;
using PlatformerToolkit.Level;

namespace PlatformerToolkit.Characters
{
    /// <summary>
    /// Reads player input and drives a <see cref="CharacterMotor2D"/>. Implements
    /// the classic platformer assists: coyote time, jump buffering, variable jump
    /// height and optional air (double) jumps.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Characters/Player Controller")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterMotor2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        private const float VerticalInputThreshold = 0.5f;

        [Header("Input")]
        [Tooltip("Input Manager axis used for horizontal movement.")]
        [SerializeField] private string horizontalAxis = "Horizontal";

        [Tooltip("Input Manager axis used for climbing, crouching and dropping through platforms.")]
        [SerializeField] private string verticalAxis = "Vertical";

        [Tooltip("Input Manager button used for jumping.")]
        [SerializeField] private string jumpButton = "Jump";

        [Tooltip("Input Manager button used for dashing. Leave empty to disable.")]
        [SerializeField] private string dashButton = "Fire1";

        [Header("Jump Assists")]
        [Tooltip("Grace period after leaving a ledge during which a jump is still allowed.")]
        [SerializeField, Range(0f, 0.5f)] private float coyoteTime = 0.1f;

        [Tooltip("How long a jump press is remembered while airborne, so a slightly early press still jumps on landing.")]
        [SerializeField, Range(0f, 0.5f)] private float jumpBufferTime = 0.15f;

        [Tooltip("Extra jumps allowed while airborne (1 = double jump).")]
        [SerializeField, Min(0)] private int maxAirJumps;

        [Tooltip("Releasing the jump button early shortens the jump.")]
        [SerializeField] private bool variableJumpHeight = true;

        [Header("Wall Jump")]
        [Tooltip("Allow jumping off walls the motor detects.")]
        [SerializeField] private bool enableWallJump = true;

        [Tooltip("Grace period after leaving a wall during which a wall jump is still allowed.")]
        [SerializeField, Range(0f, 0.5f)] private float wallCoyoteTime = 0.12f;

        [Tooltip("After a wall jump, input is overridden away from the wall for this long, so the jump always arcs out.")]
        [SerializeField, Range(0f, 0.5f)] private float wallJumpInputLock = 0.15f;

        [Header("Damage Response")]
        [Tooltip("How long input is suppressed after taking damage, letting the knockback carry the character.")]
        [SerializeField, Range(0f, 0.5f)] private float hitStunDuration = 0.25f;

        private CharacterMotor2D motor;
        private Health health;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private int airJumpsUsed;
        private float wallCoyoteCounter;
        private int lastWallDirection;
        private float inputLockCounter;
        private int lockedInputDirection;
        private float verticalInput;
        private Ladder currentLadder;

        /// <summary>
        /// The motor driven by this controller.
        /// </summary>
        public CharacterMotor2D Motor => motor;

        /// <summary>
        /// When false, all input is ignored — for cutscenes, dialogue or death.
        /// </summary>
        public bool InputEnabled { get; set; } = true;

        private void Awake()
        {
            motor = GetComponent<CharacterMotor2D>();
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (health != null)
                health.DamageTaken += HandleDamageTaken;
        }

        private void OnDisable()
        {
            if (health != null)
                health.DamageTaken -= HandleDamageTaken;
        }

        private void HandleDamageTaken(DamageInfo damage)
        {
            if (hitStunDuration <= 0f)
                return;

            lockedInputDirection = 0;
            inputLockCounter = Mathf.Max(inputLockCounter, hitStunDuration);
        }

        private void Update()
        {
            if (!InputEnabled)
            {
                motor.MoveInput = 0f;
                jumpBufferCounter = 0f;
                return;
            }

            if (inputLockCounter > 0f)
            {
                inputLockCounter -= Time.deltaTime;
                motor.MoveInput = lockedInputDirection;
            }
            else
            {
                motor.MoveInput = Input.GetAxisRaw(horizontalAxis);
            }

            verticalInput = Input.GetAxisRaw(verticalAxis);
            UpdateClimbing();
            UpdateCrouching();

            if (!string.IsNullOrEmpty(dashButton) && Input.GetButtonDown(dashButton))
                motor.TryDash();

            if (Input.GetButtonDown(jumpButton))
                jumpBufferCounter = jumpBufferTime;
            else
                jumpBufferCounter -= Time.deltaTime;

            if (motor.IsGrounded)
            {
                coyoteCounter = coyoteTime;
                airJumpsUsed = 0;
            }
            else
            {
                coyoteCounter -= Time.deltaTime;
            }

            if (!motor.IsGrounded && motor.WallDirection != 0)
            {
                wallCoyoteCounter = wallCoyoteTime;
                lastWallDirection = motor.WallDirection;
            }
            else
            {
                wallCoyoteCounter -= Time.deltaTime;
            }

            if (jumpBufferCounter > 0f)
                TryJump();

            if (variableJumpHeight && Input.GetButtonUp(jumpButton))
                motor.CutJump();
        }

        private void UpdateClimbing()
        {
            if (motor.IsClimbing)
            {
                motor.ClimbInput = verticalInput;
                if (currentLadder == null)
                    motor.StopClimb();
                return;
            }

            if (currentLadder != null && Mathf.Abs(verticalInput) > VerticalInputThreshold)
            {
                motor.StartClimb(currentLadder.ClimbCenterX);
                motor.ClimbInput = verticalInput;
            }
        }

        private void UpdateCrouching()
        {
            bool wantsCrouch = motor.IsGrounded && !motor.IsClimbing
                && verticalInput < -VerticalInputThreshold;
            motor.SetCrouching(wantsCrouch);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Ladder ladder = other.GetComponent<Ladder>();
            if (ladder != null)
                currentLadder = ladder;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (currentLadder != null && other.gameObject == currentLadder.gameObject)
            {
                currentLadder = null;
                if (motor.IsClimbing)
                    motor.StopClimb();
            }
        }

        private void TryJump()
        {
            // Down + jump on a one-way platform drops through it.
            if (motor.IsGrounded && verticalInput < -VerticalInputThreshold
                && motor.DropThroughPlatforms())
            {
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
                return;
            }

            if (motor.IsClimbing)
            {
                motor.Jump();
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
                return;
            }

            if (coyoteCounter > 0f)
            {
                motor.Jump();
            }
            else if (enableWallJump && wallCoyoteCounter > 0f)
            {
                motor.WallJump(lastWallDirection);
                lockedInputDirection = -lastWallDirection;
                inputLockCounter = wallJumpInputLock;
                wallCoyoteCounter = 0f;
            }
            else if (airJumpsUsed < maxAirJumps)
            {
                airJumpsUsed++;
                motor.Jump();
            }
            else
            {
                return;
            }

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }
}
