using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.AI;

public class ColliderCheck : MonoBehaviour
{
    [SerializeField] private float coneRadius = 5f;  
    [SerializeField] private float coneAngle = 60f; 
    [SerializeField] private LayerMask checkLayer;  
    [SerializeField] private Transform blackCatPlayHolder;  
    [SerializeField] private Transform yellowCatPlayHolder;
    private float spacing = 0.5f;
    private Dictionary<Collider, float> detectedObjects = new Dictionary<Collider, float>();
    private List<Transform> blackCats = new List<Transform>();
    private List<Transform> yellowCats = new List<Transform>();
    void Update()
    {
        CheckConeArea();
    }

    private void CheckConeArea()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, coneRadius, checkLayer);

        foreach (var hit in hits)
        {
            if (IsInCone(hit))
            {
                HandleDetectedObject(hit);
            }
            else
            {
                RemoveFromDetected(hit);
            }
        }
    }

    private bool IsInCone(Collider hit)
    {
        Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

        return angleToTarget <= coneAngle / 2f;
    }

    private void HandleDetectedObject(Collider hit)
    {
        if (!detectedObjects.ContainsKey(hit))
        {
            detectedObjects.Add(hit, 0f);
        }

        detectedObjects[hit] += Time.deltaTime;

        if (detectedObjects[hit] >= 1f)
        {
            MoveObjectToPlayHolder(hit);
        }
    }

    private void RemoveFromDetected(Collider hit)
    {
        if (detectedObjects.ContainsKey(hit))
        {
            detectedObjects.Remove(hit);
        }
    }

    private void MoveObjectToPlayHolder(Collider hit)
    {
        if (hit.CompareTag("BlackCat"))
        {
            MoveToPlayHolder(hit.transform, blackCatPlayHolder, CalculateBlackCatPosition(), blackCats);
        }
        else if (hit.CompareTag("YellowCat"))
        {
            MoveToPlayHolder(hit.transform, yellowCatPlayHolder, CalculateYellowCatPosition(), yellowCats);
        }
    }

    private void MoveToPlayHolder(Transform obj, Transform container, Vector3 targetPosition, List<Transform> list)
    {
        obj.SetParent(container);
        obj.position = targetPosition;
        obj.rotation = container.rotation;
        list.Add(obj);
        DisableNavMeshAgent(obj.GetComponent<Collider>());
    }

    private Vector3 CalculateBlackCatPosition()
    {
        return blackCatPlayHolder.position + new Vector3(0, 0, blackCats.Count * -spacing);
    }

    private Vector3 CalculateYellowCatPosition()
    {
        return yellowCatPlayHolder.position + new Vector3(0, yellowCats.Count * spacing, 0);
    }

    private void DisableNavMeshAgent(Collider hit)
    {
        NavMeshAgent navMeshAgent = hit.GetComponent<NavMeshAgent>();
        CatAI catAI = hit.GetComponent<CatAI>();
        if (navMeshAgent != null)
        {
            catAI.enabled = false;
            navMeshAgent.speed = 0;
            navMeshAgent.enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, coneRadius);

        Vector3 leftBoundary = Quaternion.Euler(0, -coneAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, coneAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * coneRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * coneRadius);
    }
}
