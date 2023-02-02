using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Makes the character move on a rail, which is defined by the bezier spline
public class CharacterRailMovement : MonoBehaviour
{
    private CharacterController _characterController;
    private Animator _animator;
    private Vector3 _lastPos;

    public AudioClip[] footsteps;
    [Range(0, 1)] public float footstepVolume;
    public FloatReference speed;
    public BezierCurveVariable bezCurve;
    public Vector3Variable playerPosition;
    private bool _isRunning;

    private GameStateVariable _gameState;

    private void OnEnable()
    {
        _gameState = Resources.Load<GameStateVariable>("GameState");
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
    }

    public void Init()
    {
        _isRunning = false;
        _lastPos = Vector3.zero;

        //Sets up the player at the initial position
        Vector3 orthoVector;
        Vector3 nextPos = bezCurve.Value.MoveLongDistance((EndlessPath.pathGenerator.LastIndex * PathGenerator.pathChunkSize) / 2f, out orthoVector);
        _lastPos = nextPos;
        nextPos.y += 10f;

        playerPosition.Value = GetGroundPosition(nextPos);
        SetCharacterPosition(playerPosition.Value);
        _characterController.transform.forward = Vector3.Cross(Vector3.up, orthoVector);
    }

    public void CleanUp()
    {
        _isRunning = false;
        _animator.SetFloat("Speed", 0);
    }

    public static Vector3 GetGroundPosition(Vector3 hoverPos)
    {
        Ray ray = new Ray(hoverPos, Vector3.down);
        RaycastHit raycastHit;
        Physics.Raycast(ray, out raycastHit);
        return raycastHit.point;
    }

    public void StartRunning()
    {
        _isRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isRunning && !_gameState.isPaused)
        {
            Move();
            Animate();
        }
    }

    private void Move()
    {
        Vector3 nextPos = bezCurve.Value.MoveAlong(speed.Value * Time.deltaTime);
        Vector3 dir = nextPos - _lastPos;
        dir.y += (-9.81f) * Time.deltaTime;
        _characterController.Move(dir);
        _characterController.transform.forward = bezCurve.Value.GetFirstDerivative();
        _lastPos = nextPos;

        playerPosition.Value = nextPos;
    }

    private void Animate()
    {
        _animator.SetFloat("Speed", _characterController.velocity.magnitude);
    }

    private void SetCharacterPosition(Vector3 position)
    {
        _characterController.enabled = false;
        _characterController.transform.position = position;
        _characterController.enabled = true;
    }

    //This function is called by a trigger from the character animation
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            int index = UnityEngine.Random.Range(0, footsteps.Length);
            AudioSource.PlayClipAtPoint(footsteps[index], transform.position, footstepVolume);
        }
    }

}
