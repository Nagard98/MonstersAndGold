using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class GameControls : MonoBehaviour
{
    public InputActionReference endWorkoutAction;
    public InputActionReference startWorkoutAction;
    public InputActionReference calibrate;

    public UnityEvent RightTriggerHeld;
    public UnityEvent LeftTriggerHeld;

    // Start is called before the first frame update
    void Start()
    {
        endWorkoutAction.action.performed += HeldRightTrigger;
        startWorkoutAction.action.performed += HeldLeftTrigger;
        calibrate.action.performed += Calibrate;

        endWorkoutAction.action.Disable();
    }

    private void Calibrate(InputAction.CallbackContext obj)
    {
        Debug.Log("Calibrating...");
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void HeldLeftTrigger(InputAction.CallbackContext obj)
    {
        Debug.Log("Held Left Trigger");
        LeftTriggerHeld.Invoke();
        startWorkoutAction.action.Disable();
        endWorkoutAction.action.Enable();
    }

    private void HeldRightTrigger(InputAction.CallbackContext obj)
    {
        Debug.Log("Held Right Trigger");
        RightTriggerHeld.Invoke();
        endWorkoutAction.action.Disable();
        startWorkoutAction.action.Enable();
    }
}
