using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PetAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int PickupHash = Animator.StringToHash("IsPickingUp");

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat(SpeedHash, speed);
    }

    public void PlayPickup()
    {
        animator.SetTrigger(PickupHash);
    }
}
