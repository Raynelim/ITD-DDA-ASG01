using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Display (Optional)")]
    public TMP_Text userInfoText;

    void Start()
    {
        LoadUserData();
    }

    void LoadUserData()
    {
        // Access the user data loaded from Firebase
        UserProgress userData = FirebaseController.currentUserData;

        if (userData != null)
        {
            Debug.Log("=== Game Loaded with User Data ===");
            Debug.Log("Email: " + userData.email);
            Debug.Log("Pet Type: " + userData.pet);
            Debug.Log("Pet Name: " + userData.petName);
            Debug.Log("Level: " + userData.level);
            Debug.Log("XP: " + userData.xp);

            // Display user info on screen (if text field is assigned)
            if (userInfoText != null)
            {
                userInfoText.text = $"Welcome!\n" +
                                   $"Pet: {userData.petName} ({userData.pet})\n" +
                                   $"Level: {userData.level}\n" +
                                   $"XP: {userData.xp}";
            }

            // Initialize game with user's stats
            InitializeGameWithUserData(userData);
        }
        else
        {
            Debug.LogError("No user data found! User might not be logged in.");
        }
    }

    void InitializeGameWithUserData(UserProgress userData)
    {
        // TODO: Use this data to set up the game
        // For example:
        // - Spawn the correct pet based on userData.pet
        // - Set the pet's level to userData.level
        // - Load the pet's name as userData.petName
        // - Set experience points to userData.xp

        Debug.Log($"Game initialized for {userData.petName} at level {userData.level}");
    }
}
