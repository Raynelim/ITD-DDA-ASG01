using UnityEngine;

public class PetStats : MonoBehaviour
{
    public int xp;
    public int happiness;

    public void AddXP(int amount)
    {
        xp += amount;
        Debug.Log("XP: " + xp);
    }

    public void AddHappiness(int amount)
    {
        happiness += amount;
        Debug.Log("Happiness: " + happiness);
    }
}
