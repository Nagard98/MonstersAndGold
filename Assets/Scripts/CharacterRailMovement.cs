using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRailMovement : MonoBehaviour
{

    private CharacterController characterController;
    private Animator animator;
    private Vector3 lastPos;

    public AudioClip[] footsteps;
    [Range(0, 1)] public float footstepVolume;
    public FloatReference speed;
    public BezierCurveVariable bezCurve;
    public Vector3Variable playerPosition;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        lastPos = Vector3.zero;

        Vector3 nextPos = bezCurve.Value.MoveAlong(0);
        playerPosition.Value = nextPos;
        lastPos = nextPos;
        nextPos.y += 10f;

        SetCharacterPosition(nextPos);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Animate();

        //TO-DO: add updateT to derivative function
        //transform.forward = _quadBezierCurve.GetFirstDerivative(_t, firstPoint: _pathwayIndex).normalized;
    }

    private void Move()
    {
        Vector3 nextPos = bezCurve.Value.MoveAlong(speed.Value * Time.deltaTime);
        Vector3 dir = nextPos - lastPos;
        dir.y += (-9.81f) * Time.deltaTime;
        characterController.Move(dir);
        characterController.transform.forward = bezCurve.Value.GetFirstDerivative();
        lastPos = nextPos;

        playerPosition.Value = nextPos;
    }

    private void Animate()
    {
        animator.SetFloat("Speed", characterController.velocity.magnitude);
    }

    private void SetCharacterPosition(Vector3 position)
    {
        characterController.enabled = false;
        characterController.transform.position = position;
        characterController.enabled = true;
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            int index = UnityEngine.Random.Range(0, footsteps.Length);
            AudioSource.PlayClipAtPoint(footsteps[index], transform.position, footstepVolume);
        }
    }

}
