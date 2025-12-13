using UnityEngine;

public class WorldConsumable : MonoBehaviour
{
    [SerializeField] public ConsumableData data;

    private bool consumed;

    public void Consume(PetStats petStats)
    {
        if (consumed || data == null || petStats == null) return;
        consumed = true;

        // Play pickup animation
        var animator = petStats.GetComponent<PetAnimatorController>();
        if (animator != null)
        {
            animator.PlayPickup();
        }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
        // Note: Battery is already deducted when spawned via drag handler
        // Just apply the effects here
        stats.AddXP(data.xpBoost);
        stats.AddHappiness(data.happinessBoost);

        Debug.Log($"Pet consumed {data.consumableName} - +{data.xpBoost} XP, +{data.happinessBoost} Happiness");

        Destroy(gameObject, 2f); // matches pickup animation length
=======
        // Route stats through GameManager (single source of truth)
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            if (data.xpBoost > 0)
                gameManager.AddXP(data.xpBoost);

        }

        Destroy(gameObject, 2.5f); // match pickup animation length
>>>>>>> Stashed changes
=======
        // Route stats through GameManager (single source of truth)
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            if (data.xpBoost > 0)
                gameManager.AddXP(data.xpBoost);

        }

        Destroy(gameObject, 2.5f); // match pickup animation length
>>>>>>> Stashed changes
    }
}
