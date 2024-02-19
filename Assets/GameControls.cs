using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameControls : MonoBehaviour
{
    public InputActionReference endWorkoutAction;
    public InputActionReference startWorkoutAction;
    public InputActionReference calibrate;
    public float HoldDuration;
    public Image loadCircle;

    public UnityEvent RightTriggerHeld;
    public UnityEvent LeftTriggerHeld;

    // Start is called before the first frame update
    void Start()
    {
        endWorkoutAction.action.performed += HeldRightTrigger;
        startWorkoutAction.action.performed += HeldLeftTrigger;
        calibrate.action.performed += Calibrate;

        endWorkoutAction.action.started += AnimateCircleLoad;
        startWorkoutAction.action.started += AnimateCircleLoad;


        endWorkoutAction.action.Disable();
    }

    private void Calibrate(InputAction.CallbackContext obj)
    {
        Debug.Log("Calibrating...");
    }

    private void AnimateCircleLoad(InputAction.CallbackContext obj)
    {
        StartCoroutine(AnimationCoroutine());
    }

    private IEnumerator AnimationCoroutine()
    {
        while (true)
        {
            if (startWorkoutAction.action.inProgress || endWorkoutAction.action.inProgress)
            {
                loadCircle.fillAmount += Mathf.Clamp01(Time.deltaTime / (HoldDuration));
                yield return null;
            }
            else
            {
                loadCircle.fillAmount = 0.0f;
                yield break;
            }
            
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void HeldLeftTrigger(InputAction.CallbackContext obj)
    {
        loadCircle.fillAmount = 0.0f;
        Debug.Log("Held Left Trigger");
        LeftTriggerHeld.Invoke();
        startWorkoutAction.action.Disable();
        endWorkoutAction.action.Enable();
    }

    private void HeldRightTrigger(InputAction.CallbackContext obj)
    {
        loadCircle.fillAmount = 0.0f;
        Debug.Log("Held Right Trigger");
        RightTriggerHeld.Invoke();
        endWorkoutAction.action.Disable();
        startWorkoutAction.action.Enable();
    }
}
