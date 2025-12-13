// ============================================================================
// File: Assets/Scripts/PetController.cs
// Handles NavMesh wandering, tap-to-move, and grab behavior for the pet
// ============================================================================

using UnityEngine;
using UnityEngine.AI;
using System.Linq;

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
    private bool isMovingToUserTap;
    private float wanderTimer;
    private bool isInteracting;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // AR-friendly NavMesh settings
        agent.updateUpAxis = false;   // AR planes are flat
        agent.updateRotation = true;
        agent.stoppingDistance = stoppingDistance;
    }

    private void OnEnable()
    {
        if (ActivePet == null)
            ActivePet = this;
    }

    private void OnDisable()
    {
        if (ActivePet == this)
            ActivePet = null;
    }

    private void Update()
    {
        if (isGrabbed || isInteracting)
            return;


        // If moving to a user tap, stop wandering until arrival
        if (isMovingToUserTap)
        {
            if (!agent.pathPending && agent.remainingDistance <= stoppingDistance)
            {
                isMovingToUserTap = false;
                wanderTimer = 0f;
            }
            return;
        }

        // ---- Wandering Logic ----
        wanderTimer += Time.deltaTime;
        bool destinationReached = !agent.hasPath || agent.remainingDistance <= stoppingDistance;

        if (wanderTimer >= wanderInterval && destinationReached)
        {
            if (TryGetRandomPointOnNavMesh(transform.position, wanderRadius, out Vector3 target))
            {
                agent.isStopped = false;
                agent.SetDestination(target);
            }

            wanderTimer = 0f;
        }
    }

    // ======================================================================
    // User Tap Movement
    // ======================================================================
    public void MoveTo(Vector3 destination)
    {
        if (isGrabbed)
            return;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
        {
            isMovingToUserTap = true;
            wanderTimer = 0f;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    // ======================================================================
    // Grab & Drag Controls
    // ======================================================================
    public void BeginGrab()
    {
        isGrabbed = true;
        isMovingToUserTap = false;

        agent.isStopped = true;
        agent.ResetPath();
    }

    public void UpdateGrab(Vector3 worldPosition)
    {
        transform.position = worldPosition;
        agent.Warp(worldPosition);   // Keep NavMeshAgent synced
    }

    public void EndGrab()
    {
        isGrabbed = false;
        wanderTimer = 0f;
    }

    // ======================================================================
    // Utility
    // ======================================================================
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

    private void LateUpdate()
    {
      if (isGrabbed) return;

        // Look for consumables
        WorldConsumable nearest = FindNearestConsumable();
        if (nearest != null)
        {
            float dist = Vector3.Distance(transform.position, nearest.transform.position);

             if (dist > stoppingDistance)
            {
                agent.SetDestination(nearest.transform.position);
            }
            else
            {
                // At the object → consume it
                nearest.Consume(GetComponent<PetStats>());
                wanderTimer = 0; 
                isInteracting = true;
                Invoke(nameof(EndInteraction), 0.6f);

            }
        }
    }

    private WorldConsumable FindNearestConsumable()
    {
        WorldConsumable[] all = FindObjectsOfType<WorldConsumable>();
        if (all.Length == 0) return null;

        return all
            .OrderBy(c => Vector3.Distance(transform.position, c.transform.position))
            .FirstOrDefault();
    }

    private void EndInteraction()
    {
        isInteracting = false;
    }

}
