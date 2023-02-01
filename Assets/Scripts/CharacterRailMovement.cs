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
    private bool isRunning;

    private void OnEnable()
    {
        isRunning = false;
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        lastPos = Vector3.zero;

        Vector3 orthoVector;
        Vector3 nextPos = bezCurve.Value.MoveLongDistance((EndlessPath.pathGenerator.LastIndex * PathGenerator.pathChunkSize) / 2f, out orthoVector);
        lastPos = nextPos;
        nextPos.y += 10f;

        playerPosition.Value = GetGroundPosition(nextPos);
        SetCharacterPosition(playerPosition.Value);
    }

    //TO-DO: move to other class
    public static Vector3 GetGroundPosition(Vector3 hoverPos)
    {
        Ray ray = new Ray(hoverPos, Vector3.down);
        RaycastHit raycastHit;
        Physics.Raycast(ray, out raycastHit);
        return raycastHit.point;
    }


    public void StartRunning()
    {
        isRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            Move();
            Animate();
        }


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
