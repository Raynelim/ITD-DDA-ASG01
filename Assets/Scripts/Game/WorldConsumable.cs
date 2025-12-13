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

        stats.AddXP(data.xpBoost);
        stats.AddHappiness(data.happinessBoost);

        Destroy(gameObject, 2f); // matches pickup animation length
}

}
