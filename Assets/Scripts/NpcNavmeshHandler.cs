// File: Assets/Scripts/NpcNavmeshHandler.cs
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NpcNavmeshHandler : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderDistance = 10f;
    public float wanderSpeed = 2f;
    public float wanderIntervalMin = 2f;
    public float wanderIntervalMax = 5f;

    [Header("Player Click Move Settings")]
    public float clickMoveSpeed = 3.5f;
    public LayerMask clickGroundMask;

    private NavMeshAgent agent;
    private Coroutine wanderRoutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        wanderRoutine = StartCoroutine(WanderLoop());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (RaycastClickPoint(out Vector3 point))
            {
                if (wanderRoutine != null) StopCoroutine(wanderRoutine);
                agent.speed = clickMoveSpeed;        // Why: distinguish behavior speeds
                agent.SetDestination(point);
                wanderRoutine = StartCoroutine(RestartWanderAfterArrival());
            }
        }
    }

    private bool RaycastClickPoint(out Vector3 point)
    {
        point = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, clickGroundMask))
        {
            point = hit.point;
            return true;
        }
        return false;
    }

    private IEnumerator WanderLoop()
    {
        while (true)
        {
            float wait = Random.Range(wanderIntervalMin, wanderIntervalMax);
            yield return new WaitForSeconds(wait);

            Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderDistance;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderDistance, NavMesh.AllAreas))
            {
                agent.speed = wanderSpeed;          // Why: ensures clear wander behavior
                agent.SetDestination(hit.position);
            }
        }
    }

    private IEnumerator RestartWanderAfterArrival()
    {
        while (agent.pathPending) yield return null;
        while (agent.remainingDistance > agent.stoppingDistance) yield return null;

        wanderRoutine = StartCoroutine(WanderLoop());
    }
}
