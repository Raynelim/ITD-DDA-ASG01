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

    [Header("Size Scaling Settings")]
    public int xpPerScaleIncrease = 50; // XP needed for each size increase
    public float scaleIncreaseAmount = 0.1f; // How much to increase scale each time (10%)
    public float baseScale = 1f; // Starting scale at 0 XP
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

    [Header("Sleep Mode UI")]
    public GameObject sleepModePanel; // Panel shown during sleep mode
    public TMP_Text sleepPassiveXPText; // Display passive XP earned during sleep
    public TMP_Text sleepTimePassedText; // Display time passed during sleep
    public GameObject exitSleepButton; // Button to exit sleep mode

    private DatabaseReference dbReference;
    private GameObject currentPet;

    // Battery collection timers
    private float smallBatteryTimer;
    private float mediumBatteryTimer;
    private float largeBatteryTimer;

    // Passive XP timer
    private float passiveXPTimer;
    private bool isSleepModeActive = false;
    private int sleepXPEarned = 0; // Track XP earned during current sleep session
    private float sleepTimeElapsed = 0f; // Track time passed during current sleep session

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

        // Hide sleep mode panel initially
        if (sleepModePanel != null)
            sleepModePanel.SetActive(false);

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

    void UpdateSleepUI()
    {
        if (!isSleepModeActive || sleepModePanel == null || !sleepModePanel.activeSelf)
            return;

        // Update passive XP earned display
        if (sleepPassiveXPText != null)
        {
            sleepPassiveXPText.text = $"Passive XP Earned: {sleepXPEarned}";
        }

        // Update time passed display in HH:MM:SS format
        if (sleepTimePassedText != null)
        {
            int hours = Mathf.FloorToInt(sleepTimeElapsed / 3600f);
            int minutes = Mathf.FloorToInt((sleepTimeElapsed % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(sleepTimeElapsed % 60f);
            sleepTimePassedText.text = $"Time Passed: {hours:00}:{minutes:00}:{seconds:00}";
        }
    }

    void UpdatePassiveXP()
    {
        // Only run passive XP timer if sleep mode is active
        if (!isSleepModeActive)
            return;

        // Track time elapsed during sleep
        sleepTimeElapsed += Time.deltaTime;

        // Passive XP Timer
        passiveXPTimer -= Time.deltaTime;
        if (passiveXPTimer <= 0f)
        {
            // Gain passive XP
            AddXP(passiveXPAmount);
            sleepXPEarned += passiveXPAmount;
            Debug.Log($"Passive XP gained from sleep! +{passiveXPAmount} XP (Total this session: {sleepXPEarned})");
            // Reset timer
            passiveXPTimer = passiveXPInterval;
        }

        // Update sleep UI
        UpdateSleepUI();

        if (DevSettings.Instance != null && DevSettings.Instance.devModeEnabled)
        {
            Debug.Log("DEV MODE ENABLED → Overriding battery counts");

            UserDataManager.currentSmallBattery = DevSettings.Instance.smallBattery;
            UserDataManager.currentMediumBattery = DevSettings.Instance.mediumBattery;
            UserDataManager.currentLargeBattery = DevSettings.Instance.largeBattery;
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

        // Spawn the correct pet model (always stage 1, no evolution)
        SpawnPet(UserDataManager.currentPetType, 1);

        // Apply size scaling based on current XP
        UpdatePetScale();

        // Update UI
        UpdateUI();
    }

    // Update pet scale based on current XP
    public void UpdatePetScale()
    {
        if (currentPet == null)
            return;

        // Calculate how many 50 XP increments the pet has reached
        int scaleLevel = UserDataManager.currentXP / xpPerScaleIncrease;
        
        // Calculate the new scale
        float newScale = baseScale + (scaleLevel * scaleIncreaseAmount);
        
        // Apply the scale to the pet
        currentPet.transform.localScale = Vector3.one * newScale;
        
        Debug.Log($"Pet scaled to {newScale:F2}x based on {UserDataManager.currentXP} XP (Scale Level: {scaleLevel})");
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
            
            // Apply current scale based on XP
            UpdatePetScale();
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
        CloseMenu(); // Close main menu when action is triggered
        
        // Open the consumable/food menu
        ConsumableMenu foodMenu = FindObjectOfType<ConsumableMenu>();
        if (foodMenu != null)
        {
            foodMenu.ToggleMenu();
        }
        else
        {
            Debug.LogWarning("ConsumableMenu not found in scene!");
        }
    }

    public void OnSleepButtonPress()
    {
        if (!isSleepModeActive)
        {
            // Activate sleep mode
            isSleepModeActive = true;
            Debug.Log("Sleep mode activated - Passive XP timer started");
            
            // Reset tracking variables
            sleepXPEarned = 0;
            sleepTimeElapsed = 0f;
            passiveXPTimer = passiveXPInterval;
            
            // Show sleep mode panel
            if (sleepModePanel != null)
            {
                sleepModePanel.SetActive(true);
                UpdateSleepUI();
            }
        }

        CloseMenu(); // Close menu when action is triggered
    }

    public void OnExitSleepButtonPress()
    {
        if (isSleepModeActive)
        {
            Debug.Log($"Sleep mode deactivated - Earned {sleepXPEarned} XP over {sleepTimeElapsed:F0} seconds");
            
            // Deactivate sleep mode
            isSleepModeActive = false;
            
            // Hide sleep panel
            if (sleepModePanel != null)
                sleepModePanel.SetActive(false);
            
            // Reset timer
            passiveXPTimer = passiveXPInterval;
            
            // Update UI and save to database in real-time
            RefreshUI();
            AutoSave();
            
            Debug.Log("Game state updated and saved to database");
        }
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
        }
        
        // Update pet size based on new XP
        UpdatePetScale();
        
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
        if (DevSettings.Instance != null && DevSettings.Instance.devModeEnabled)
        {
            Debug.Log($"[DEV MODE] Ignoring battery usage: {batteryType}");
            return true;
        }

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
            RefreshUI();
            AutoSave();
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
