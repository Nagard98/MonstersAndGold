using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    public EnemyVariable enemyInfo;
    private Vector3[] _navPoints;
    public FloatVariable playerHP;
    public Inventory inventory;
    public float senseDistance;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;

    public UnityEvent Attack, ShieldBrake;

    private int _indexNav;
    private bool _alreadyAttacked;

    public Vector3[] NavPoints { get { return _navPoints; } set { _navPoints = value; } }

    void Start()
    {
        _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _indexNav = 0;
        _alreadyAttacked = false;
    }

    void Update()
    {
        //Checks if player is in sensing radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, senseDistance, 1 << 7);
        if (!_alreadyAttacked && colliders.Length > 0)
        {
            Transform player = colliders[0].transform;
            Hit(player);
        }
        else
        {
            Wander();
        }
        _animator.SetFloat("Speed", _navMeshAgent.velocity.magnitude);
    }

    private void Hit(Transform player)
    {
        _navMeshAgent.isStopped = true;
        _navMeshAgent.transform.LookAt(player);
        _animator.SetTrigger("Attack");
        _alreadyAttacked = true;

        int startingShieldCount = inventory.Items.Count;
        int brokenShieldTier = TryBrakeShield();
        //Checks if there was a shield that could protect the player from the attack
        if(!(brokenShieldTier < startingShieldCount + 1))
        {
            playerHP.Value -= enemyInfo.atkDamage;
            Attack.Invoke();
        }
        else
        {
            inventory.LastTierTouched = brokenShieldTier;
            ShieldBrake.Invoke();
        }
        
        
    }

    private int TryBrakeShield()
    {
        int smallestEffectiveShieldTier = inventory.Items.Count + 1;
        ShieldVariable tmpToRemove = null;
        foreach (ShieldVariable shield in inventory.Items)
        {
            if (shield.Tier >= enemyInfo.Tier)
            {
                if (smallestEffectiveShieldTier > shield.Tier)
                {
                    smallestEffectiveShieldTier = shield.Tier;
                    tmpToRemove = shield;
                }
            }
        }
        inventory.Items.Remove(tmpToRemove);
        return smallestEffectiveShieldTier;
    }

    private void Wander()
    {
        if (ReachedDestinationOrGaveUp())
        {
            _navMeshAgent.SetDestination(_navPoints[_indexNav % _navPoints.Length]);
            _indexNav += 1;
        }
    }


    public bool ReachedDestinationOrGaveUp()
    {

        if (!_navMeshAgent.pathPending)
        {
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                if (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
