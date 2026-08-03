using System.Collections.Generic;
using UnityEngine;

namespace PlatformerToolkit.Characters
{
    /// <summary>
    /// Bridges a <see cref="CharacterMotor2D"/> to an <see cref="Animator"/>,
    /// keeping animation parameters in sync with the motor state. Parameters
    /// missing from the animator controller are skipped silently, so hook up
    /// only the ones your animations need.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Characters/Player Animator")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterMotor2D))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        [Tooltip("Animator to drive. Defaults to one found on this object or its children.")]
        [SerializeField] private Animator animator;

        [Header("Parameter Names")]
        [Tooltip("Float parameter receiving the absolute horizontal speed.")]
        [SerializeField] private string speedParameter = "Speed";

        [Tooltip("Bool parameter set while the character is airborne.")]
        [SerializeField] private string airborneParameter = "IsJumping";

        [Tooltip("Bool parameter set while crouching. Optional.")]
        [SerializeField] private string crouchParameter = "IsCrouching";

        [Tooltip("Bool parameter set while climbing a ladder. Optional.")]
        [SerializeField] private string climbParameter = "IsClimbing";

        [Tooltip("Bool parameter set while sliding down a wall. Optional.")]
        [SerializeField] private string wallSlideParameter = "IsWallSliding";

        [Tooltip("Bool parameter set while dashing. Optional.")]
        [SerializeField] private string dashParameter = "IsDashing";

        private CharacterMotor2D motor;
        private readonly HashSet<int> availableParameters = new HashSet<int>();
        private int speedHash;
        private int airborneHash;
        private int crouchHash;
        private int climbHash;
        private int wallSlideHash;
        private int dashHash;

        private void Awake()
        {
            motor = GetComponent<CharacterMotor2D>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            speedHash = Animator.StringToHash(speedParameter);
            airborneHash = Animator.StringToHash(airborneParameter);
            crouchHash = Animator.StringToHash(crouchParameter);
            climbHash = Animator.StringToHash(climbParameter);
            wallSlideHash = Animator.StringToHash(wallSlideParameter);
            dashHash = Animator.StringToHash(dashParameter);

            CacheAvailableParameters();
        }

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (animator == null)
                return;

            SetFloat(speedHash, Mathf.Abs(motor.Velocity.x));
            SetBool(airborneHash, !motor.IsGrounded && !motor.IsClimbing);
            SetBool(crouchHash, motor.IsCrouching);
            SetBool(climbHash, motor.IsClimbing);
            SetBool(wallSlideHash, motor.IsWallSliding);
            SetBool(dashHash, motor.IsDashing);
        }

        private void CacheAvailableParameters()
        {
            availableParameters.Clear();
            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
                availableParameters.Add(parameter.nameHash);
        }

        private void SetFloat(int hash, float value)
        {
            if (availableParameters.Contains(hash))
                animator.SetFloat(hash, value);
        }

        private void SetBool(int hash, bool value)
        {
            if (availableParameters.Contains(hash))
                animator.SetBool(hash, value);
        }
    }
}
