using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    public EnemyVariable enemyInfo;
    public Vector3[] navPoints;
    public FloatVariable playerHP;
    public float senseDistance;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    public UnityEvent Attack;

    private int indexNav;
    private bool alreadyAttacked;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        indexNav = 0;
        alreadyAttacked = false;
        senseDistance = 5f;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, senseDistance, 1 << 7);
        if (!alreadyAttacked && colliders.Length > 0)
        {
            Transform player = colliders[0].transform;
            Hit(player);
        }
        else
        {
            Wander();
        }

        animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
    }

    private void Hit(Transform player)
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.transform.LookAt(player);
        animator.SetTrigger("Attack");
        alreadyAttacked = true;
        Attack.Invoke();
    }

    private void Wander()
    {
        if (ReachedDestinationOrGaveUp())
        {
            navMeshAgent.SetDestination(navPoints[indexNav % navPoints.Length]);
            indexNav += 1;
        }
    }

    public bool ReachedDestinationOrGaveUp()
    {

        if (!navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
