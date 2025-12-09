// ============================================================================
// File: Assets/Scripts/PetController.cs
// Handles NavMesh wandering, tap-to-move, and grab behavior for the pet
// ============================================================================

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PetController : MonoBehaviour
{
    [Header("Wandering")]
    [Tooltip("Radius around the current position within which the pet will wander.")]
    [SerializeField] private float wanderRadius = 2.5f;

    [Tooltip("How often (in seconds) to pick a new wander destination.")]
    [SerializeField] private float wanderInterval = 5f;

    [Tooltip("Distance at which we consider the destination reached.")]
    [SerializeField] private float stoppingDistance = 0.3f;

    public static PetController ActivePet { get; private set; }

    private NavMeshAgent agent;
    private bool isGrabbed;
    private float wanderTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // These settings help with AR scale issues
        agent.updateUpAxis = false;
        agent.updateRotation = true;
    }

    private void OnEnable()
    {
        if (ActivePet == null)
        {
            ActivePet = this;
        }
    }

    private void OnDisable()
    {
        if (ActivePet == this)
        {
            ActivePet = null;
        }
    }

    private void Update()
    {
        if (isGrabbed)
        {
            return;
        }

        wanderTimer += Time.deltaTime;

        bool needsNewDestination = !agent.hasPath || agent.remainingDistance <= stoppingDistance;

        if (wanderTimer >= wanderInterval && needsNewDestination)
        {
            if (TryGetRandomPointOnNavMesh(transform.position, wanderRadius, out Vector3 target))
            {
                agent.isStopped = false;
                agent.SetDestination(target);
            }

            wanderTimer = 0f;
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if (isGrabbed)
        {
            return;
        }

        wanderTimer = 0f;
        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    public void BeginGrab()
    {
        isGrabbed = true;
        agent.isStopped = true;
        agent.ResetPath();
    }

    public void UpdateGrab(Vector3 worldPosition)
    {
        // Warp agent along with transform to avoid desync
        transform.position = worldPosition;
        agent.Warp(worldPosition);
    }

    public void EndGrab()
    {
        isGrabbed = false;
        // After letting go, the pet will resume wandering logic on next Update()
        wanderTimer = 0f;
    }

    private static bool TryGetRandomPointOnNavMesh(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            var randomPoint = center + Random.insideUnitSphere * range;
            randomPoint.y = center.y;

            if (NavMesh.SamplePosition(randomPoint, out var hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }
}