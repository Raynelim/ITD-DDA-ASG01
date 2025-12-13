using UnityEngine;

public class WorldConsumable : MonoBehaviour
{
    public ConsumableData data;

    private bool consumed = false;

    public void Consume(PetStats stats)
    {
        if (consumed) return;
        consumed = true;

        var animator = stats.GetComponent<PetAnimatorController>();
        if (animator != null)
        {
            animator.PlayPickup();
        }

        // Note: Battery is already deducted when spawned via drag handler
        // Just apply the effects here
        stats.AddXP(data.xpBoost);
        stats.AddHappiness(data.happinessBoost);

        Debug.Log($"Pet consumed {data.consumableName} - +{data.xpBoost} XP, +{data.happinessBoost} Happiness");

        Destroy(gameObject, 2f); // matches pickup animation length
    }
}
