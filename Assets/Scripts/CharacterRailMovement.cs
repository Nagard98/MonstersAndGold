using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterRailMovement : MonoBehaviour
{

    public float _speed = 2f;
    public Transform destination;
    private NavMeshAgent _navMeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.SetDestination(destination.position);
    }

    // Update is called once per frame
    void Update()
    {

        _navMeshAgent.speed = _speed;

    }
}
