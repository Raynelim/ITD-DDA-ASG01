using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro; // Assuming you use TextMeshPro

public class FirebaseController : MonoBehaviour
{
    [Header("Login Input Fields")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;

    [Header("Register Input Fields")]
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;

    [Header("Pet Selection & Naming")]
    public TMP_InputField pet1NameInput; // Input field for Pet 1 (Skeleton Warrior) name
    public TMP_InputField pet2NameInput; // Input field for Pet 2 (Wolf Mage) name

    [Header("Main Panels")]
    public GameObject startPage; // The initial start page
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject petSelectionPanel; // Choose between Pet 1 or Pet 2
    public GameObject pet1NameCreationPanel; // Pet 1 (Skeleton Warrior) name creation panel
    public GameObject pet2NameCreationPanel; // Pet 2 (Wolf Mage) name creation panel

    [Header("Scene Settings")]
    public string gameSceneName = "DemoScene"; // Name of the game scene to load

    [Header("Notification Panels")]
    public GameObject invalidLoginPanel; // "Invalid Login Details"
    public GameObject loginSuccessPanel; // "Login Successful"
    public GameObject accountExistsPanel; // "Account created, go login"
    public GameObject accountCreatedPanel; // "Account created successfully"
    public GameObject wrongFormatPanel; // "Wrong email or password format"

    private FirebaseAuth auth;
    private DatabaseReference dbReference;

    // Temporary storage for registration flow
    private string tempUserId;
    private string tempUserEmail;
    private string selectedPetType;

    void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Hide all notification panels initially
        if (invalidLoginPanel != null) invalidLoginPanel.SetActive(false);
        if (loginSuccessPanel != null) loginSuccessPanel.SetActive(false);
        if (accountExistsPanel != null) accountExistsPanel.SetActive(false);
        if (accountCreatedPanel != null) accountCreatedPanel.SetActive(false);
        if (wrongFormatPanel != null) wrongFormatPanel.SetActive(false);

        // Hide pet selection and name creation panels
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);

        // Start with start page visible
        if (startPage != null) startPage.SetActive(true);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
    }

    // ================= REGISTER LOGIC =================

    public void OnRegisterButtonPress()
    {
        Debug.Log("=== OnRegisterButtonPress called ===");
        Debug.Log("registerEmailInput is null? " + (registerEmailInput == null));
        Debug.Log("registerPasswordInput is null? " + (registerPasswordInput == null));

        if (registerEmailInput == null || registerPasswordInput == null)
        {
            Debug.LogError("Input fields are not assigned in Inspector!");
            return;
        }

        string email = registerEmailInput.text.Trim();
        string password = registerPasswordInput.text;

        Debug.Log("Email entered: '" + email + "' (length: " + email.Length + ")");
        Debug.Log("Password entered: '" + password + "' (length: " + password.Length + ")");

        // Validate email format (basic check)
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogWarning("Email is empty");
            ShowWrongFormatNotification();
            return;
        }

        if (!email.Contains("@"))
        {
            Debug.LogWarning("Email missing @");
            ShowWrongFormatNotification();
            return;
        }

        string[] emailParts = email.Split('@');
        if (emailParts.Length != 2 || !emailParts[1].Contains("."))
        {
            Debug.LogWarning("Email format invalid - After @: '" + (emailParts.Length > 1 ? emailParts[1] : "nothing") + "'");
            ShowWrongFormatNotification();
            return;
        }

        // Validate password length
        if (password.Length < 6)
        {
            Debug.LogWarning("Password too short: " + password.Length + " characters");
            ShowWrongFormatNotification();
            return;
        }

        Debug.Log("✓ Email and password validation passed - Starting registration");
        StartCoroutine(RegisterUser(email, password));
    }

    private IEnumerator RegisterUser(string email, string password)
    {
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            // Handle Errors (e.g., Account already exists)
            FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            if (errorCode == AuthError.EmailAlreadyInUse)
            {
                // Show "Account created, go login" panel
                ShowAccountExistsNotification();
            }
            else if (errorCode == AuthError.InvalidEmail || errorCode == AuthError.WeakPassword)
            {
                // Show "Wrong format" panel for invalid email or weak password
                ShowWrongFormatNotification();
            }
            else
            {
                // Other errors - log them
                Debug.LogError("Register Error: " + firebaseEx.Message);
            }
        }
        else
        {
            // User created successfully in Auth. Now go to pet selection.
            FirebaseUser newUser = registerTask.Result.User;
            tempUserId = newUser.UserId;
            tempUserEmail = email;
            
            // Show success notification, then go to pet selection
            ShowAccountCreatedNotification();
        }
    }

    // ================= PET SELECTION & NAMING =================

    public void SelectRoboFox()
    {
        selectedPetType = "robofox";
        Debug.Log("=== SelectRoboFox called - Going to Pet 1 Name Panel ===");
        Debug.Log("Pet Selection Panel: " + (petSelectionPanel != null ? "EXISTS" : "NULL"));
        Debug.Log("Pet 1 Name Creation Panel: " + (pet1NameCreationPanel != null ? "EXISTS" : "NULL"));
        
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
        
        if (pet1NameCreationPanel != null)
        {
            pet1NameCreationPanel.SetActive(true);
            Debug.Log("Pet 1 (RoboFox) Name Panel set to ACTIVE");
            Debug.Log("Pet1NameCreationPanel activeSelf: " + pet1NameCreationPanel.activeSelf);
            Debug.Log("Pet1NameCreationPanel activeInHierarchy: " + pet1NameCreationPanel.activeInHierarchy);
        }
        else
        {
            Debug.LogError("pet1NameCreationPanel is NULL! Assign the Pet 1 name panel in Inspector!");
        }
    }

    public void SelectRoboCat()
    {
        selectedPetType = "robocat";
        Debug.Log("=== SelectRoboCat called - Going to Pet 2 Name Panel ===");
        Debug.Log("Pet Selection Panel: " + (petSelectionPanel != null ? "EXISTS" : "NULL"));
        Debug.Log("Pet 2 Name Creation Panel: " + (pet2NameCreationPanel != null ? "EXISTS" : "NULL"));
        
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        
        if (pet2NameCreationPanel != null)
        {
            pet2NameCreationPanel.SetActive(true);
            Debug.Log("Pet 2 (Robocat) Name Panel set to ACTIVE");
            Debug.Log("Pet2NameCreationPanel activeSelf: " + pet2NameCreationPanel.activeSelf);
            Debug.Log("Pet2NameCreationPanel activeInHierarchy: " + pet2NameCreationPanel.activeInHierarchy);
        }
        else
        {
            Debug.LogError("pet2NameCreationPanel is NULL! Assign the Pet 2 name panel in Inspector!");
        }
    }

    public void ConfirmSkeletonWarriorName()
    {
        string customPetName = pet1NameInput.text.Trim();
        
        if (string.IsNullOrEmpty(customPetName))
        {
            Debug.LogWarning("Pet name cannot be empty!");
            return;
        }

        Debug.Log("Confirming Skeleton Warrior Name: " + customPetName);
        SaveUserProfileWithPet(tempUserId, tempUserEmail, selectedPetType, customPetName);
    }

    public void ConfirmWolfMageName()
    {
        string customPetName = pet2NameInput.text.Trim();
        
        if (string.IsNullOrEmpty(customPetName))
        {
            Debug.LogWarning("Pet name cannot be empty!");
            return;
        }

        Debug.Log("Confirming Wolf Mage Name: " + customPetName);
        SaveUserProfileWithPet(tempUserId, tempUserEmail, selectedPetType, customPetName);
    }

    private void SaveUserProfileWithPet(string userId, string email, string petType, string petName)
    {
        // First, count existing players to determine the new player number
        StartCoroutine(CreatePlayerAccount(userId, email, petType, petName));
    }

    private IEnumerator CreatePlayerAccount(string userId, string email, string petType, string petName)
    {
        // Get all players to count them
        var playersTask = dbReference.Child("players").GetValueAsync();
        yield return new WaitUntil(() => playersTask.IsCompleted);

        int playerCount = 0;
        if (playersTask.Exception == null && playersTask.Result.Exists)
        {
            playerCount = (int)playersTask.Result.ChildrenCount;
        }

        // Create new player key (player1, player2, etc.)
        string newPlayerKey = "player" + (playerCount + 1);
        Debug.Log($"Creating new player: {newPlayerKey}");

        // Create user object with pet data (level 1, xp 0, batteries all 0 for new users)
        UserProgress userProgress = new UserProgress(userId, email, petType, petName, 1, 0);
        string json = JsonUtility.ToJson(userProgress);

        // Save to Database: players/player{N}/
        dbReference.Child("players").Child(newPlayerKey).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log($"Player account created: {newPlayerKey} with Pet: {petType}, Pet Name: {petName}");
                
                // Hide name creation panels
                if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
                if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
                
                // Load user data into static manager
                UserDataManager.LoadUserData(newPlayerKey, userProgress);
                
                // Go to game scene
                LoadGameScene();
            }
            else
            {
                Debug.LogError("Failed to save user profile: " + task.Exception);
            }
        });
    }

    // ================= BACK BUTTON NAVIGATION =================

    public void GoBackPetSelection()
    {
        Debug.Log("Going back to Pet Selection");
        if (startPage != null) startPage.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
        if (petSelectionPanel != null) petSelectionPanel.SetActive(true);
    }

    public void GoBackRegister()
    {
        Debug.Log("Going back to Register");
        if (startPage != null) startPage.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
    }

    // ================= LOGIN LOGIC =================

    public void OnLoginButtonPress()
    {
        Debug.Log("=== OnLoginButtonPress called ===");
        
        if (loginEmailInput == null)
        {
            Debug.LogError("loginEmailInput is NULL! Not assigned in Inspector!");
            return;
        }
        
        if (loginPasswordInput == null)
        {
            Debug.LogError("loginPasswordInput is NULL! Not assigned in Inspector!");
            return;
        }

        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        Debug.Log("Login - Email entered: '" + email + "' (length: " + email.Length + ")");
        Debug.Log("Login - Password entered: (length: " + password.Length + ")");

        // Basic validation - just check if fields are not empty
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogWarning("Login failed: Email is empty!");
            ShowWrongFormatNotification();
            return;
        }
        
        if (string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Login failed: Password is empty!");
            ShowWrongFormatNotification();
            return;
        }

        Debug.Log("✓ Proceeding with login - Firebase will validate credentials");
        StartCoroutine(LoginUser(email, password));
    }

    private IEnumerator LoginUser(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            // Handle Errors - Show "Invalid Login Details" panel
            Debug.LogError("Login Error: " + loginTask.Exception.GetBaseException().Message);
            ShowInvalidLoginNotification();
        }
        else
        {
            // Auth successful. Now retrieve data from Database.
            FirebaseUser user = loginTask.Result.User;
            StartCoroutine(RetrieveUserData(user.UserId));
        }
    }

    private IEnumerator RetrieveUserData(string userId)
    {
        // Search all players for matching firebaseId
        var playersTask = dbReference.Child("players").GetValueAsync();
        yield return new WaitUntil(predicate: () => playersTask.IsCompleted);

        if (playersTask.Exception != null)
        {
            Debug.LogError("Failed to load players data: " + playersTask.Exception);
            ShowInvalidLoginNotification();
        }
        else if (!playersTask.Result.Exists)
        {
            Debug.LogWarning("No players found in database.");
            ShowInvalidLoginNotification();
        }
        else
        {
            // Search through all players for matching firebaseId
            bool playerFound = false;
            foreach (DataSnapshot playerSnapshot in playersTask.Result.Children)
            {
                string json = playerSnapshot.GetRawJsonValue();
                UserProgress playerData = JsonUtility.FromJson<UserProgress>(json);
                
                if (playerData.firebaseId == userId)
                {
                    // Found the player!
                    playerFound = true;
                    string playerKey = playerSnapshot.Key; // e.g., "player1"
                    
                    Debug.Log($"Player found: {playerKey} - {playerData.petName} (Level {playerData.level})");
                    
                    // Load data into static manager
                    UserDataManager.LoadUserData(playerKey, playerData);
                    
                    // Show success and go to game
                    ShowLoginSuccessNotification();
                    break;
                }
            }
            
            if (!playerFound)
            {
                Debug.LogWarning("No player account found for this Firebase ID.");
                ShowInvalidLoginNotification();
            }
        }
    }

    // ================= UI HELPERS =================

    public void GoToLoginFromStart()
    {
        Debug.Log("Going to Login from Start Page");
        
        // Clear login input fields
        if (loginEmailInput != null) loginEmailInput.text = "";
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        
        if (startPage != null) startPage.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(true);
        
        // Hide notifications AFTER switching panels to handle child notification panels
        HideAllNotifications();
    }

    public void GoToRegisterFromStart()
    {
        Debug.Log("Going to Register from Start Page");
        
        // Clear register input fields
        if (registerEmailInput != null) registerEmailInput.text = "";
        if (registerPasswordInput != null) registerPasswordInput.text = "";
        
        if (startPage != null) startPage.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        
        // Hide notifications AFTER switching panels to handle child notification panels
        HideAllNotifications();
    }

    public void SwitchToRegister()
    {
        Debug.Log("Switching to Register Panel");
        
        // Clear all input fields
        if (loginEmailInput != null) loginEmailInput.text = "";
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        if (registerEmailInput != null) registerEmailInput.text = "";
        if (registerPasswordInput != null) registerPasswordInput.text = "";
        
        if (startPage != null) startPage.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        
        // Hide notifications AFTER switching panels to handle child notification panels
        HideAllNotifications();
    }

    public void SwitchToLogin()
    {
        Debug.Log("Switching to Login Panel");
        
        // Clear all input fields
        if (loginEmailInput != null) loginEmailInput.text = "";
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        if (registerEmailInput != null) registerEmailInput.text = "";
        if (registerPasswordInput != null) registerPasswordInput.text = "";
        
        if (startPage != null) startPage.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(true);
        
        // Hide notifications AFTER switching panels to handle child notification panels
        HideAllNotifications();
    }

    void LoadGameScene()
    {
        Debug.Log("Loading game scene: " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }

    // ================= NOTIFICATION HANDLERS =================

    private void HideAllNotifications()
    {
        Debug.Log("Hiding all notifications");
        if (invalidLoginPanel != null)
        {
            invalidLoginPanel.SetActive(false);
            Debug.Log("Hid invalidLoginPanel");
        }
        if (loginSuccessPanel != null)
        {
            loginSuccessPanel.SetActive(false);
            Debug.Log("Hid loginSuccessPanel");
        }
        if (accountExistsPanel != null)
        {
            accountExistsPanel.SetActive(false);
            Debug.Log("Hid accountExistsPanel");
        }
        if (accountCreatedPanel != null)
        {
            accountCreatedPanel.SetActive(false);
            Debug.Log("Hid accountCreatedPanel");
        }
        if (wrongFormatPanel != null)
        {
            wrongFormatPanel.SetActive(false);
            Debug.Log("Hid wrongFormatPanel");
        }
        else
        {
            Debug.LogWarning("wrongFormatPanel is NULL in HideAllNotifications!");
        }
    }

    private void ShowInvalidLoginNotification()
    {
        Debug.Log("Notification: Invalid Login Details");
        HideAllNotifications();
        if (invalidLoginPanel != null)
        {
            invalidLoginPanel.transform.SetAsLastSibling();
            invalidLoginPanel.SetActive(true);
            
            // Ensure visibility
            CanvasGroup cg = invalidLoginPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = invalidLoginPanel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            
            StartCoroutine(HideNotificationAfterDelay(invalidLoginPanel, 2f));
        }
    }

    private void ShowLoginSuccessNotification()
    {
        Debug.Log("Notification: Login Successful - Loading game menu");
        HideAllNotifications();
        if (loginSuccessPanel != null)
        {
            loginSuccessPanel.transform.SetAsLastSibling();
            loginSuccessPanel.SetActive(true);
            
            // Ensure visibility
            CanvasGroup cg = loginSuccessPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = loginSuccessPanel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            
            StartCoroutine(HideNotificationAndLoadGame(loginSuccessPanel, 2f));
        }
        else
        {
            LoadGameScene();
        }
    }

    private void ShowAccountExistsNotification()
    {
        Debug.Log("Notification: Account already exists");
        HideAllNotifications();
        if (accountExistsPanel != null)
        {
            accountExistsPanel.transform.SetAsLastSibling();
            accountExistsPanel.SetActive(true);
            
            // Ensure visibility
            CanvasGroup cg = accountExistsPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = accountExistsPanel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            
            StartCoroutine(HideNotificationAfterDelay(accountExistsPanel, 2f));
        }
    }

    private void ShowAccountCreatedNotification()
    {
        Debug.Log("Notification: Account created successfully - Moving to pet selection");
        HideAllNotifications();
        if (accountCreatedPanel != null)
        {
            accountCreatedPanel.transform.SetAsLastSibling();
            accountCreatedPanel.SetActive(true);
            
            // Ensure visibility
            CanvasGroup cg = accountCreatedPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = accountCreatedPanel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            
            StartCoroutine(HideNotificationAndShowPetSelection(accountCreatedPanel, 2f));
        }
        else
        {
            ShowPetSelectionPanel();
        }
    }

    private void ShowWrongFormatNotification()
    {
        Debug.Log("Notification: Wrong email or password format");
        HideAllNotifications();
        
        if (wrongFormatPanel != null)
        {
            wrongFormatPanel.transform.SetAsLastSibling();
            wrongFormatPanel.SetActive(true);
            
            // Ensure visibility
            CanvasGroup cg = wrongFormatPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = wrongFormatPanel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            
            StartCoroutine(HideNotificationAfterDelay(wrongFormatPanel, 2f));
        }
        else
        {
            Debug.LogError("wrongFormatPanel is NULL!");
        }
    }

    private void ShowPetSelectionPanel()
    {
        Debug.Log("Showing Pet Selection Panel");
        if (startPage != null) startPage.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
        if (petSelectionPanel != null) petSelectionPanel.SetActive(true);
    }

    private IEnumerator HideNotificationAndShowPetSelection(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null) panel.SetActive(false);
        ShowPetSelectionPanel();
    }

    private IEnumerator HideNotificationAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null) panel.SetActive(false);
    }

    private IEnumerator HideNotificationAndLoadGame(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null) panel.SetActive(false);
        LoadGameScene();
    }
}

// ================= DATA CLASS =================

[System.Serializable]
public class Inventory
{
    public int smallBattery;
    public int mediumBattery;
    public int largeBattery;

    public Inventory(int small = 0, int medium = 0, int large = 0)
    {
        smallBattery = small;
        mediumBattery = medium;
        largeBattery = large;
    }
}

[System.Serializable]
public class UserProgress
{
    public string firebaseId; // Firebase Auth UID
    public string email;
    public string pet; // Pet type (robofox, robocat)
    public string petName; // Custom pet name
    public int petStage; // Evolution stage (1, 2, 3)
    public int level;
    public int xp;
    public Inventory inventory;

    // Constructor for new users
    public UserProgress(string firebaseId, string email, string pet, string petName, int level, int xp)
    {
        this.firebaseId = firebaseId;
        this.email = email;
        this.pet = pet;
        this.petName = petName;
        this.petStage = 1; // New pets start at stage 1
        this.level = level;
        this.xp = xp;
        this.inventory = new Inventory(0, 0, 0);
    }

    // Full constructor with inventory and stage
    public UserProgress(string firebaseId, string email, string pet, string petName, int petStage, int level, int xp, int smallBattery, int mediumBattery, int largeBattery)
    {
        this.firebaseId = firebaseId;
        this.email = email;
        this.pet = pet;
        this.petName = petName;
        this.petStage = petStage;
        this.level = level;
        this.xp = xp;
        this.inventory = new Inventory(smallBattery, mediumBattery, largeBattery);
    }
}

// ================= STATIC DATA MANAGER =================
// This class holds user data across scenes
public static class UserDataManager
{
    public static string currentPlayerKey; // e.g., "player1", "player2"
    public static string currentFirebaseId;
    public static string currentUserEmail;
    public static int currentLevel;
    public static int currentXP;
    public static string currentPetType; // robofox or robocat
    public static string currentPetName;
    public static int currentPetStage; // Evolution stage (1, 2, 3)
    public static int currentSmallBattery;
    public static int currentMediumBattery;
    public static int currentLargeBattery;

    // Load data from UserProgress object
    public static void LoadUserData(string playerKey, UserProgress data)
    {
        currentPlayerKey = playerKey;
        currentFirebaseId = data.firebaseId;
        currentUserEmail = data.email;
        currentLevel = data.level;
        currentXP = data.xp;
        currentPetType = data.pet;
        currentPetName = data.petName;
        currentPetStage = data.petStage;
        currentSmallBattery = data.inventory.smallBattery;
        currentMediumBattery = data.inventory.mediumBattery;
        currentLargeBattery = data.inventory.largeBattery;

        Debug.Log($"UserDataManager loaded: {playerKey} - {currentPetName} (Level {currentLevel}, XP {currentXP}, Stage {currentPetStage})");
        Debug.Log($"Inventory: Small={currentSmallBattery}, Medium={currentMediumBattery}, Large={currentLargeBattery}");
    }

    // Clear all data (for logout)
    public static void ClearUserData()
    {
        currentPlayerKey = null;
        currentFirebaseId = null;
        currentUserEmail = null;
        currentLevel = 0;
        currentXP = 0;
        currentPetType = null;
        currentPetName = null;
        currentPetStage = 1;
        currentSmallBattery = 0;
        currentMediumBattery = 0;
        currentLargeBattery = 0;
    }
}