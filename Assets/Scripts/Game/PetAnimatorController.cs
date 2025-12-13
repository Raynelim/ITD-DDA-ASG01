using UnityEngine;
using UnityEngine.AI;

public class PetAnimatorController : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private Animator animator;
    private EvolutionModelSwitcher modelSwitcher;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int PickupHash = Animator.StringToHash("IsPickingUp");

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        modelSwitcher = GetComponent<EvolutionModelSwitcher>();
        RefreshAnimator();
    }

    void Update()
    {
        if (animator == null || agent == null) return;
        animator.SetFloat(SpeedHash, agent.velocity.magnitude);
    }

    public void PlayPickup()
    {
        if (animator != null)
            animator.SetTrigger(PickupHash);
    }

    public void RefreshAnimator()
    {
        if (modelSwitcher == null) return;
        animator = modelSwitcher.GetActiveAnimator();
    }
}
