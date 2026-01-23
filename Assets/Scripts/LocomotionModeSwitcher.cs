

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

public class LocomotionModeSwitcher : MonoBehaviour
{
    [Header("Locomotion Components")]
    public ContinuousMoveProvider continuousMoveProvider;
    public GravityProvider gravityProvider;
    public CharacterController characterController;

    [Header("Settings")]
    [Tooltip("Speed multiplier when flying.")]
    public float flyingSpeedMultiplier = 2.0f;
    public float verticalAscentSpeed = 4.0f;
    private float originalMoveSpeed;

    [Header("Input Actions")]
    [Tooltip("The input action to toggle flying/walking mode.")]
    public InputActionProperty toggleFlyAction;
    public InputActionProperty altitudeAction;

    private bool isFlying = true;

    void Start()
    {
        if (continuousMoveProvider != null)
        {
            originalMoveSpeed = continuousMoveProvider.moveSpeed;
        }

        SetFlyingMode();
    }

    private void OnEnable()
    {
        // Subscribe to the input action event.
        toggleFlyAction.action.performed += OnToggleFlyAction;
        toggleFlyAction.action.Enable();

        altitudeAction.action.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe from the input action event.
        toggleFlyAction.action.performed -= OnToggleFlyAction;
        toggleFlyAction.action.Disable();

        altitudeAction.action.Disable();
    }

    private void OnToggleFlyAction(InputAction.CallbackContext context)
    {
        isFlying = !isFlying;
        if (isFlying)
        {
            SetFlyingMode();
        }
        else
        {
            SetWalkingMode();
        }
    }

    private void SetFlyingMode()
    {
        // Disable gravity
        if (gravityProvider != null)
        {
            gravityProvider.enabled = false;
        }

        // Enable 3D movement on the move provider
        if (continuousMoveProvider != null)
        {
            // continuousMoveProvider.enableFly changes the y direction too,
            // and we want the change in altitude to be handled separately
            continuousMoveProvider.enableFly = false;
            continuousMoveProvider.moveSpeed = originalMoveSpeed * flyingSpeedMultiplier;
        }

        Debug.Log("Switched to Flying Mode");
    }

    private void SetWalkingMode()
    {
        // Enable gravity
        if (gravityProvider != null)
        {
            gravityProvider.enabled = true;
        }
        // Disable 3D movement on the move provider
        if (continuousMoveProvider != null)
        {
            continuousMoveProvider.enableFly = false;
            continuousMoveProvider.moveSpeed = originalMoveSpeed;
        }
        Debug.Log("Switched to Walking Mode");
    }

    void HandleAltitudeControl()
    {
        // Read input from joystick
        Vector2 input = altitudeAction.action.ReadValue<Vector2>();

        // We only care about the y-axis for elevation control
        float elevationInput = input.y;

        if(Mathf.Abs(elevationInput) > 0.1f)
        {
            // Create a vertical movement vector
            Vector3 verticalMovement = Vector3.up * elevationInput * verticalAscentSpeed * Time.deltaTime;

            // Move the character
            characterController.Move(verticalMovement);
        }
    }

    void Update()
    {
        if (isFlying && characterController != null)
        {
            HandleAltitudeControl();
        }
    }
}
