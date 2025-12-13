using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Pet Models - Robofox Evolution Stages")]
    public GameObject robofoxStage1Prefab;
    public GameObject robofoxStage2Prefab;
    public GameObject robofoxStage3Prefab;

    [Header("Pet Models - Robocat Evolution Stages")]
    public GameObject robocatStage1Prefab;
    public GameObject robocatStage2Prefab;
    public GameObject robocatStage3Prefab;

    [Header("Pet Spawn")]
    public Transform petSpawnPoint; // Where to spawn the pet

    [Header("Evolution Settings")]
    public int stage2LevelRequirement = 10; // Level needed for stage 2
    public int stage3LevelRequirement = 20; // Level needed for stage 3
    public int xpPerLevel = 10; // XP needed per level

    [Header("Evolution UI")]
    public GameObject evolutionPanel; // Panel that shows "Your pet is evolving!"
    public CanvasGroup evolutionCanvasGroup; // For fade in/out
    public TMP_Text evolutionText; // Text saying "Your pet is evolving!"
    public float evolutionDisplayDuration = 3f; // How long to show the evolution message

    [Header("UI Elements")]
    public TMP_Text petNameText; // Display pet name
    public TMP_Text levelText; // Display level
    public TMP_Text xpText; // Display XP
    public TMP_Text smallBatteryText; // Display small battery count
    public TMP_Text mediumBatteryText; // Display medium battery count
    public TMP_Text largeBatteryText; // Display large battery count

    [Header("Buttons")]
    public GameObject feedButton;
    public GameObject sleepButton;
    public GameObject logoutButton;

    [Header("Logout Confirmation")]
    public GameObject logoutConfirmationPanel; // "Are you sure you want to log out?"
    public GameObject yesButton; // Confirm logout
    public GameObject noButton; // Cancel logout

    private DatabaseReference dbReference;
    private GameObject currentPet;

    void Start()
    {
        // Initialize Firebase Database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Hide evolution panel initially
        if (evolutionPanel != null)
            evolutionPanel.SetActive(false);

        // Hide logout confirmation panel initially
        if (logoutConfirmationPanel != null)
            logoutConfirmationPanel.SetActive(false);

        // Load user data and spawn pet
        LoadPlayerData();
    }

    void LoadPlayerData()
    {
        // Check if user data exists
        if (string.IsNullOrEmpty(UserDataManager.currentPlayerKey))
        {
            Debug.LogError("No user data loaded! UserDataManager is empty.");
            return;
        }

        Debug.Log($"Loading player: {UserDataManager.currentPlayerKey}");
        Debug.Log($"Pet: {UserDataManager.currentPetName} ({UserDataManager.currentPetType}) - Stage {UserDataManager.currentPetStage}");
        Debug.Log($"Level: {UserDataManager.currentLevel}, XP: {UserDataManager.currentXP}");

        // Spawn the correct pet model with current stage (no auto-evolution on load)
        SpawnPet(UserDataManager.currentPetType, UserDataManager.currentPetStage);

        // Update UI
        UpdateUI();
    }

    // Call this method when XP or level changes in-game
    public void CheckForEvolution()
    {
        int currentLevel = UserDataManager.currentLevel;
        int currentStage = UserDataManager.currentPetStage;
        int newStage = currentStage;

        // Determine what stage the pet should be at based on level
        if (currentLevel >= stage3LevelRequirement)
        {
            newStage = 3;
        }
        else if (currentLevel >= stage2LevelRequirement)
        {
            newStage = 2;
        }
        else
        {
            newStage = 1;
        }

        // If stage changed, trigger evolution animation
        if (newStage > currentStage)
        {
            Debug.Log($"Pet is evolving from Stage {currentStage} to Stage {newStage}!");
            StartCoroutine(EvolutionSequence(currentStage, newStage));
        }
    }

    IEnumerator EvolutionSequence(int oldStage, int newStage)
    {
        // Update the stage in UserDataManager
        UserDataManager.currentPetStage = newStage;

        // Show evolution panel with fade in
        if (evolutionPanel != null)
        {
            evolutionPanel.SetActive(true);
            
            if (evolutionText != null)
                evolutionText.text = "Your pet is evolving!";

            // Fade in
            if (evolutionCanvasGroup != null)
            {
                float elapsed = 0f;
                float fadeDuration = 0.5f;
                
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    evolutionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                    yield return null;
                }
                evolutionCanvasGroup.alpha = 1f;
            }

            // Wait to show the message
            yield return new WaitForSeconds(evolutionDisplayDuration);

            // Fade out
            if (evolutionCanvasGroup != null)
            {
                float elapsed = 0f;
                float fadeDuration = 0.5f;
                
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    evolutionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                    yield return null;
                }
                evolutionCanvasGroup.alpha = 0f;
            }

            evolutionPanel.SetActive(false);
        }
        else
        {
            // If no evolution panel, just wait a moment
            yield return new WaitForSeconds(1f);
        }

        // Despawn old pet and spawn new evolved pet
        SpawnPet(UserDataManager.currentPetType, newStage);

        // Save the evolution
        AutoSave();
    }

    void SpawnPet(string petType, int stage)
    {
        // Destroy existing pet if any
        if (currentPet != null)
        {
            Destroy(currentPet);
        }

        GameObject prefabToSpawn = null;

        // Select the correct prefab based on type and stage
        if (petType == "robofox")
        {
            switch (stage)
            {
                case 1:
                    prefabToSpawn = robofoxStage1Prefab;
                    break;
                case 2:
                    prefabToSpawn = robofoxStage2Prefab;
                    break;
                case 3:
                    prefabToSpawn = robofoxStage3Prefab;
                    break;
            }
        }
        else if (petType == "robocat")
        {
            switch (stage)
            {
                case 1:
                    prefabToSpawn = robocatStage1Prefab;
                    break;
                case 2:
                    prefabToSpawn = robocatStage2Prefab;
                    break;
                case 3:
                    prefabToSpawn = robocatStage3Prefab;
                    break;
            }
        }

        // Spawn the pet
        if (prefabToSpawn != null)
        {
            currentPet = Instantiate(prefabToSpawn, petSpawnPoint.position, petSpawnPoint.rotation);
            Debug.Log($"Spawned {petType} Stage {stage}");
        }
        else
        {
            Debug.LogError($"Pet prefab not assigned for {petType} Stage {stage}!");
        }
    }

    void UpdateUI()
    {
        // Always read from UserDataManager for real-time data
        // Update pet info
        if (petNameText != null)
            petNameText.text = UserDataManager.currentPetName;

        // Calculate current level from total XP
        int calculatedLevel = (UserDataManager.currentXP / xpPerLevel) + 1; // +1 because level starts at 1
        int currentLevelXP = UserDataManager.currentXP % xpPerLevel; // XP within current level

        if (levelText != null)
            levelText.text = "Level: " + calculatedLevel;

        if (xpText != null)
            xpText.text = currentLevelXP + "/" + xpPerLevel + " XP";

        // Update inventory (batteries) with "x" prefix for quantity display
        if (smallBatteryText != null)
            smallBatteryText.text = "x" + UserDataManager.currentSmallBattery;

        if (mediumBatteryText != null)
            mediumBatteryText.text = "x" + UserDataManager.currentMediumBattery;

        if (largeBatteryText != null)
            largeBatteryText.text = "x" + UserDataManager.currentLargeBattery;

        Debug.Log($"UI Updated: {UserDataManager.currentPetName} | Level {calculatedLevel} | XP {currentLevelXP}/{xpPerLevel} (Total: {UserDataManager.currentXP})");
    }

    // ================= BUTTON FUNCTIONS =================

    public void OnFeedButtonPress()
    {
        Debug.Log("Feed button pressed - Opening feed menu");
        // TODO: Open feeding UI/menu
        // This will be implemented in the eating script
    }

    public void OnSleepButtonPress()
    {
        Debug.Log("Sleep button pressed - Saving game");
        StartCoroutine(SavePlayerData());
    }

    // ================= LOGOUT SYSTEM =================

    public void OnLogoutButtonPress()
    {
        Debug.Log("Logout button pressed - Showing confirmation");
        if (logoutConfirmationPanel != null)
        {
            logoutConfirmationPanel.SetActive(true);
        }
    }

    public void OnLogoutConfirmYes()
    {
        Debug.Log("Logout confirmed - Saving and returning to login scene");
        StartCoroutine(LogoutSequence());
    }

    public void OnLogoutConfirmNo()
    {
        Debug.Log("Logout cancelled");
        if (logoutConfirmationPanel != null)
        {
            logoutConfirmationPanel.SetActive(false);
        }
    }

    IEnumerator LogoutSequence()
    {
        // Hide confirmation panel
        if (logoutConfirmationPanel != null)
            logoutConfirmationPanel.SetActive(false);

        // Save current game state to Firebase
        yield return StartCoroutine(SavePlayerData());

        // Clear user data from UserDataManager
        UserDataManager.ClearUserData();
        Debug.Log("User data cleared from UserDataManager");

        // Load LoginRegisterScene
        SceneManager.LoadScene("LoginRegisterScene");
    }

    // ================= SAVE SYSTEM =================

    public IEnumerator SavePlayerData()
    {
        Debug.Log("=== Saving player data to Firebase ===");
        Debug.Log($"Player: {UserDataManager.currentPlayerKey}");
        Debug.Log($"Level: {UserDataManager.currentLevel}, XP: {UserDataManager.currentXP}, Stage: {UserDataManager.currentPetStage}");
        Debug.Log($"Batteries - Small: {UserDataManager.currentSmallBattery}, Medium: {UserDataManager.currentMediumBattery}, Large: {UserDataManager.currentLargeBattery}");

        // Create UserProgress object with current data from UserDataManager
        UserProgress saveData = new UserProgress(
            UserDataManager.currentFirebaseId,
            UserDataManager.currentUserEmail,
            UserDataManager.currentPetType,
            UserDataManager.currentPetName,
            UserDataManager.currentPetStage,
            UserDataManager.currentLevel,
            UserDataManager.currentXP,
            UserDataManager.currentSmallBattery,
            UserDataManager.currentMediumBattery,
            UserDataManager.currentLargeBattery
        );

        string json = JsonUtility.ToJson(saveData);
        Debug.Log($"JSON to save: {json}");

        // Save to Firebase: players/player{N}/
        var saveTask = dbReference.Child("players").Child(UserDataManager.currentPlayerKey).SetRawJsonValueAsync(json);
        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.Exception != null)
        {
            Debug.LogError("Failed to save: " + saveTask.Exception);
        }
        else
        {
            Debug.Log($"✓ Game saved successfully to Firebase for {UserDataManager.currentPlayerKey}");
        }
    }

    // ================= PUBLIC METHODS FOR OTHER SCRIPTS =================

    // Call this when pet gains XP (from feeding, etc.)
    public void AddXP(int amount)
    {
        int oldLevel = UserDataManager.currentLevel;
        
        // Add to total XP
        UserDataManager.currentXP += amount;
        Debug.Log($"Added {amount} XP. Total XP: {UserDataManager.currentXP}");
        
        // Calculate new level from total XP
        int newLevel = (UserDataManager.currentXP / xpPerLevel) + 1; // +1 because level starts at 1
        
        // Check if leveled up
        if (newLevel > oldLevel)
        {
            UserDataManager.currentLevel = newLevel;
            Debug.Log($"Level up! Now level {UserDataManager.currentLevel}");
            
            // Check for evolution
            CheckForEvolution();
        }
        
        RefreshUI();
        AutoSave();
    }

    // Update battery counts in real-time
    public void AddBattery(string batteryType, int amount)
    {
        switch (batteryType.ToLower())
        {
            case "small":
                UserDataManager.currentSmallBattery += amount;
                Debug.Log($"Small Battery: {UserDataManager.currentSmallBattery}");
                break;
            case "medium":
                UserDataManager.currentMediumBattery += amount;
                Debug.Log($"Medium Battery: {UserDataManager.currentMediumBattery}");
                break;
            case "large":
                UserDataManager.currentLargeBattery += amount;
                Debug.Log($"Large Battery: {UserDataManager.currentLargeBattery}");
                break;
        }
        
        RefreshUI();
        AutoSave();
    }

    // Use battery for feeding
    public bool UseBattery(string batteryType, int amount = 1)
    {
        bool success = false;
        
        switch (batteryType.ToLower())
        {
            case "small":
                if (UserDataManager.currentSmallBattery >= amount)
                {
                    UserDataManager.currentSmallBattery -= amount;
                    success = true;
                }
                break;
            case "medium":
                if (UserDataManager.currentMediumBattery >= amount)
                {
                    UserDataManager.currentMediumBattery -= amount;
                    success = true;
                }
                break;
            case "large":
                if (UserDataManager.currentLargeBattery >= amount)
                {
                    UserDataManager.currentLargeBattery -= amount;
                    success = true;
                }
                break;
        }
        
        if (success)
        {
            Debug.Log($"Used {amount} {batteryType} battery");
            RefreshUI();
            AutoSave();
        }
        else
        {
            Debug.LogWarning($"Not enough {batteryType} batteries!");
        }
        
        return success;
    }

    // Call this after feeding or gaining XP to update UI
    public void RefreshUI()
    {
        UpdateUI();
    }

    // Call this to auto-save after important actions
    public void AutoSave()
    {
        StartCoroutine(SavePlayerData());
    }
}
