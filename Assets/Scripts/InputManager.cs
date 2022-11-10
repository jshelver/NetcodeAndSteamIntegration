using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Script References")]
    public static PlayerControls playerControls;
    public static event Action<InputActionMap> actionMapChange;

    [Header("Game Input Variables")]
    public static Vector2 movementInput;
    public static Vector2 lookInput;
    public static bool jumpInput;
    public static bool gameEscapeInput;

    [Header("UI Input Variables")]
    public static bool gameIsPaused;
    public static bool menuEscapeInput;


    void Awake()
    {
        playerControls = new PlayerControls();
    }

    void Update()
    {
        // Storing input in variables (player action map)
        movementInput = playerControls.Player.Movement.ReadValue<Vector2>();
        lookInput = playerControls.Player.Look.ReadValue<Vector2>();
        jumpInput = playerControls.Player.Jump.triggered;

        // Enter pause menu
        gameEscapeInput = playerControls.Player.Escape.triggered;
        if (gameEscapeInput)
        {
            gameIsPaused = true;
            SwitchActionMap(playerControls.UI);
        }

        // Exit pause menu
        menuEscapeInput = playerControls.UI.Escape.triggered;
        if (menuEscapeInput)
        {
            gameIsPaused = false;
            SwitchActionMap(playerControls.Player);
        }
    }

    void OnEnable()
    {
        // Only turn on the player action map at start
        playerControls.Player.Enable();
        gameIsPaused = false;
    }

    void OnDisable()
    {
        playerControls.Disable();
    }

    public static void SwitchActionMap(InputActionMap actionMap)
    {
        // Disables every action map
        playerControls.Disable();
        // Call the action map change event so scripts are aware of the change (optional)
        actionMapChange?.Invoke(actionMap);
        // Enable desired action map
        actionMap.Enable();
    }

}
