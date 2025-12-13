using UnityEngine;

public class PetStats : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene!");
        }
    }

    // Called when consumable is eaten
    public void AddXP(int amount)
    {
        if (gameManager == null) return;

        gameManager.AddXP(amount);
        Debug.Log($"Pet gained {amount} XP via GameManager");
    }
}
