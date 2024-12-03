using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatAI : MonoBehaviour
{
    [SerializeField] private float wanderRadius = 40f;
    private Animator _anim;

    private NavMeshAgent agent;
    private Vector3 startPosition;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();
        startPosition = transform.position;
        MoveToNewPosition();
    }

    private void Update()
    {
        UpdateAnimationParameters();

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            MoveToNewPosition();
        }
    }

    private void UpdateAnimationParameters()
    {
        float movementSpeed = agent.velocity.magnitude;

        _anim.SetBool("Run", movementSpeed > 0.1f);

        _anim.SetFloat("Movement Speed", movementSpeed);
    }

    private void MoveToNewPosition()
    {
        Vector3 newDestination = GetRandomPositionWithinRadius();
        agent.SetDestination(newDestination);
    }

    private Vector3 GetRandomPositionWithinRadius()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += startPosition;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }
}
