using UnityEngine;

public class WorldConsumable : MonoBehaviour
{
    public ConsumableData data;

    private bool consumed = false;

    public void Consume(PetStats stats)
    {
        if (consumed) return;
        consumed = true;

        stats.AddXP(data.xpBoost);
        stats.AddHappiness(data.happinessBoost);

        Destroy(gameObject, 0.1f);
    }
}
