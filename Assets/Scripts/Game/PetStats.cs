using UnityEngine;

public class PetStats : MonoBehaviour
{
    public int xp;
    public int happiness;

    public void AddXP(int amount)
    {
        xp += amount;
        Debug.Log("Pet local XP: " + xp);
        
        // Update GameManager and save to Firebase
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.AddXP(amount);
        }
        else
        {
            Debug.LogError("GameManager not found! XP not saved to Firebase.");
        }
    }

}
