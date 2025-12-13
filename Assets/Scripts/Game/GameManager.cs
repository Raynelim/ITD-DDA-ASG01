using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Pet Models")]
    public GameObject robofoxPrefab;
    public GameObject robocatPrefab;

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

    [Header("Timer UI Elements")]
    public TMP_Text smallBatteryTimerText; // Countdown for next small battery
    public TMP_Text mediumBatteryTimerText; // Countdown for next medium battery
    public TMP_Text largeBatteryTimerText; // Countdown for next large battery

    [Header("Battery Collection Timers")]
    public float smallBatteryInterval = 10f; // 10 seconds
    public float mediumBatteryInterval = 30f; // 30 seconds
    public float largeBatteryInterval = 60f; // 60 seconds (1 minute)

    [Header("Passive XP Gain")]
    public float passiveXPInterval = 300f; // 300 seconds (5 minutes)
    public int passiveXPAmount = 1; // XP gained per interval

    [Header("Buttons")]
    public GameObject feedButton;
    public GameObject sleepButton;
    public GameObject logoutButton;

    [Header("Expandable Menu")]
    public GameObject menuPanel; // The expandable menu containing buttons and timers
    public GameObject menuToggleButton; // Button to open/close the menu
    public TMP_Text menuToggleButtonText; // Text on toggle button ("Menu" or "Close")
    private bool isMenuOpen = false;

    [Header("Logout Confirmation")]
    public GameObject logoutConfirmationPanel; // "Are you sure you want to log out?"
    public GameObject yesButton; // Confirm logout
    public GameObject noButton; // Cancel logout

    private DatabaseReference dbReference;
    private GameObject currentPet;

    // Battery collection timers
    private float smallBatteryTimer;
    private float mediumBatteryTimer;
    private float largeBatteryTimer;

    // Passive XP timer
    private float passiveXPTimer;
    private bool isSleepModeActive = false;

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

        // Hide menu panel initially (collapsed)
        if (menuPanel != null)
            menuPanel.SetActive(false);
        isMenuOpen = false;
        UpdateMenuToggleButtonText();

        // Initialize battery collection timers
        smallBatteryTimer = smallBatteryInterval;
        mediumBatteryTimer = mediumBatteryInterval;
        largeBatteryTimer = largeBatteryInterval;

        // Initialize passive XP timer
        passiveXPTimer = passiveXPInterval;

        // Load user data and spawn pet
        LoadPlayerData();
    }

    void Update()
    {
        // Update battery collection timers
        UpdateBatteryTimers();

        // Update passive XP timer
        UpdatePassiveXP();
    }

    void UpdateBatteryTimers()
    {
        // Small Battery Timer
        smallBatteryTimer -= Time.deltaTime;
        if (smallBatteryTimer <= 0f)
        {
            // Collect small battery
            AddBattery("small", 1);
            Debug.Log("Small battery collected!");
            // Reset timer
            smallBatteryTimer = smallBatteryInterval;
        }

        // Medium Battery Timer
        mediumBatteryTimer -= Time.deltaTime;
        if (mediumBatteryTimer <= 0f)
        {
            // Collect medium battery
            AddBattery("medium", 1);
            Debug.Log("Medium battery collected!");
            // Reset timer
            mediumBatteryTimer = mediumBatteryInterval;
        }

        // Large Battery Timer
        largeBatteryTimer -= Time.deltaTime;
        if (largeBatteryTimer <= 0f)
        {
            // Collect large battery
            AddBattery("large", 1);
            Debug.Log("Large battery collected!");
            // Reset timer
            largeBatteryTimer = largeBatteryInterval;
        }

        // Update timer display
        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        // Display countdown timers in MM:SS format
        if (smallBatteryTimerText != null)
        {
            int minutes = Mathf.FloorToInt(smallBatteryTimer / 60f);
            int seconds = Mathf.FloorToInt(smallBatteryTimer % 60f);
            smallBatteryTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (mediumBatteryTimerText != null)
        {
            int minutes = Mathf.FloorToInt(mediumBatteryTimer / 60f);
            int seconds = Mathf.FloorToInt(mediumBatteryTimer % 60f);
            mediumBatteryTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (largeBatteryTimerText != null)
        {
            int minutes = Mathf.FloorToInt(largeBatteryTimer / 60f);
            int seconds = Mathf.FloorToInt(largeBatteryTimer % 60f);
            largeBatteryTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void UpdatePassiveXP()
    {
        // Only run passive XP timer if sleep mode is active
        if (!isSleepModeActive)
            return;

        // Passive XP Timer
        passiveXPTimer -= Time.deltaTime;
        if (passiveXPTimer <= 0f)
        {
            // Gain passive XP
            AddXP(passiveXPAmount);
            Debug.Log($"Passive XP gained from sleep! +{passiveXPAmount} XP");
            // Reset timer
            passiveXPTimer = passiveXPInterval;
        }
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
    GameObject prefabToSpawn = null;

    if (petType == "robofox")
        prefabToSpawn = robofoxPrefab;
    else if (petType == "robocat")
        prefabToSpawn = robocatPrefab;

    if (prefabToSpawn == null)
    {
        Debug.LogError($"No prefab assigned for pet type: {petType}");
        return;
    }

    bool needsRespawn =
        currentPet == null ||
        !currentPet.name.Contains(petType);

    if (needsRespawn)
    {
        if (currentPet != null)
            Destroy(currentPet);

        currentPet = Instantiate(
            prefabToSpawn,
            petSpawnPoint.position,
            petSpawnPoint.rotation
        );
    }

    // ALWAYS update stage
    var switcher = currentPet.GetComponent<EvolutionModelSwitcher>();
    var animator = currentPet.GetComponent<PetAnimatorController>();

    if (switcher == null || animator == null)
    {
        Debug.LogError("Pet prefab missing required components!");
        return;
    }

    switcher.SetStage(stage);
    animator.RefreshAnimator();

    Debug.Log($"Pet {petType} set to Stage {stage}");
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
        CloseMenu(); // Close menu when action is triggered
        // TODO: Open feeding UI/menu
        // This will be implemented in the eating script
    }

    public void OnSleepButtonPress()
    {
        // Toggle sleep mode
        isSleepModeActive = !isSleepModeActive;

        if (isSleepModeActive)
        {
            Debug.Log("Sleep mode activated - Passive XP timer started");
            // Start the timer from the beginning
            passiveXPTimer = passiveXPInterval;
        }
        else
        {
            Debug.Log("Sleep mode deactivated - Passive XP timer reset (XP gained is saved)");
            // Reset timer when exiting sleep mode
            passiveXPTimer = passiveXPInterval;
        }

        CloseMenu(); // Close menu when action is triggered
    }

    // ================= MENU TOGGLE =================

    public void OnMenuToggleButtonPress()
    {
        if (isMenuOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    void OpenMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            isMenuOpen = true;
            UpdateMenuToggleButtonText();
            Debug.Log("Menu opened");
        }
    }

    void CloseMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            isMenuOpen = false;
            UpdateMenuToggleButtonText();
            Debug.Log("Menu closed");
        }
    }

    void UpdateMenuToggleButtonText()
    {
        if (menuToggleButtonText != null)
        {
            menuToggleButtonText.text = isMenuOpen ? "Close" : "Menu";
        }
    }

    // ================= LOGOUT SYSTEM =================

    public void OnLogoutButtonPress()
    {
        Debug.Log("Logout button pressed - Showing confirmation");
        CloseMenu(); // Close menu when showing logout confirmation
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

    // Call this when pet gains XP (from feeding, passive gain, etc.)
    // Updates in real-time and auto-saves to Firebase
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

    // Update battery counts in real-time (auto-saves to Firebase)
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
