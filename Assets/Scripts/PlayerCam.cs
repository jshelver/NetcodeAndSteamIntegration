using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCam : NetworkBehaviour
{
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform cameraHolder;
    Camera mainCamera;

    [Header("Look Settings")]
    [SerializeField] float mouseSensitivity = 25f;
    [SerializeField] float viewAngle = 80f;
    float mouseX, mouseY;
    float xRotation, yRotation;

    void Start()
    {

        // Parent the main camera under the camera holder
        mainCamera = Camera.main;
        mainCamera.transform.parent = cameraHolder;
        // Reset all local space changes to zero (camera holder transform is modified instead)
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    void Update()
    {
        if (!IsOwner) return;

        SetLookInput(InputManager.lookInput);
        HandleLook();
        HandleCursor();
    }

    private void SetLookInput(Vector2 lookInput)
    {
        // Get mouse inputs
        mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
    }

    private void HandleLook()
    {
        // Add input to rotation
        yRotation += mouseX;
        xRotation -= mouseY;
        // Clamp the vertical look angle
        xRotation = Mathf.Clamp(xRotation, -viewAngle, viewAngle);

        // Player model only rotates horizontally
        characterController.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        // Camera rotates vertically because it is already being rotated horizontally with the player
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleCursor()
    {
        if (InputManager.gameIsPaused)
        {
            // Showing and unlocking the cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Hiding and locking the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
