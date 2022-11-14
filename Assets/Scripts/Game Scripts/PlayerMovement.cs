using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform groundCheckerTransform;
    [SerializeField] LayerMask groundLayer;

    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float runSpeed = 9f;
    [SerializeField] float speedChangeSmoothTime = 0.1f;
    float moveSpeed;
    Vector3 movementDirection, speedChangeVelocity, adjustedMovementDirection;


    [Header("Jump/Gravity Settings")]
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -20f;
    [HideInInspector] public static bool isGrounded;
    float yVelocity;

    void Start()
    {

    }

    void Update()
    {
        // If you aren't the owner of the object, then return
        if (!IsOwner) return;

        HandleMovement(MovementInput(InputManager.movementInput), gravity);
    }

    private Vector3 MovementInput(Vector2 movementInput)
    {
        return new Vector3(movementInput.x, 0f, movementInput.y).normalized;
    }

    private void HandleMovement(Vector3 targetMovementDirection, float gravity)
    {
        HandleGravity(gravity, InputManager.jumpInput);

        // Set the correct movement speed
        moveSpeed = walkSpeed;

        // Check if on slope
        if (Physics.Raycast(characterController.transform.position, Vector3.down, out RaycastHit hit, 2f, groundLayer))
        {
            targetMovementDirection = ConvertMovementDirectionToSlopeAngle(targetMovementDirection, hit);
        }
        // Smooth speed transition
        movementDirection = Vector3.SmoothDamp(movementDirection, targetMovementDirection * moveSpeed * Time.deltaTime, ref speedChangeVelocity, speedChangeSmoothTime);
        // Convert movement direction from local to world space
        adjustedMovementDirection = transform.TransformDirection(movementDirection);

        // Applies movement to character controller
        characterController.Move(adjustedMovementDirection);
    }

    private void IsGroundedChecker()
    {
        // Checks if grounded
        isGrounded = (Physics.CheckSphere(groundCheckerTransform.position, characterController.radius, groundLayer));
    }

    private void HandleGravity(float gravity, bool jumpInput)
    {
        IsGroundedChecker();
        // If jump button is pressed, and player is grounded then call jump function
        if (jumpInput && isGrounded)
        {
            Jump();
        }

        // Keeps gravity constant when grounded
        if (isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        // Gradually increases gravity when in air
        yVelocity += gravity * Time.deltaTime;
        characterController.Move(Vector3.up * yVelocity * Time.deltaTime);
        // print(yVelocity);
    }

    private void Jump()
    {
        // Sets the y velocity so that the players jump height is jumpHeight
        yVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
    }

    private Vector3 ConvertMovementDirectionToSlopeAngle(Vector3 movementDir, RaycastHit hit)
    {
        // Store the slope's normal in local space (needs to be local because our movement direction is currently in local space)
        Vector3 slopeNormal = transform.InverseTransformDirection(hit.normal);

        // Get the angle between relative up and slope's normal
        float groundSlopeAngle = Vector3.Angle(slopeNormal, transform.up);
        if (groundSlopeAngle != 0f)
        {
            // Basically gives the amount of rotation to get from transform.up to slopeNormal
            Quaternion slopeAngleRotation = Quaternion.FromToRotation(transform.up, slopeNormal);
            // Multiply movementDir by this Quaternion so now movementDir is perpendicular to the slope's normal, instead of transform.up
            movementDir = slopeAngleRotation * movementDir;
        }
        return movementDir;
    }
}
