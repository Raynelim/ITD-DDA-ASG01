using System.Collections;
using UnityEngine;
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
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject petSelectionPanel; // Choose between Pet 1 or Pet 2
    public GameObject pet1NameCreationPanel; // Pet 1 (Skeleton Warrior) name creation panel
    public GameObject pet2NameCreationPanel; // Pet 2 (Wolf Mage) name creation panel
    public GameObject gamePanel; // The game menu shown after successful login

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

        // Start with register panel visible
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
    }

    // ================= REGISTER LOGIC =================

    public void OnRegisterButtonPress()
    {
        string email = registerEmailInput.text.Trim();
        string password = registerPasswordInput.text;

        // Validate email format
        if (string.IsNullOrEmpty(email) || !email.Contains("@") || !email.Contains("."))
        {
            ShowWrongFormatNotification();
            return;
        }

        // Validate password length
        if (password.Length < 6)
        {
            ShowWrongFormatNotification();
            return;
        }

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

    public void SelectSkeletonWarrior()
    {
        selectedPetType = "SkeletonWarrior";
        Debug.Log("=== SelectSkeletonWarrior called - Going to Pet 1 Name Panel ===");
        Debug.Log("Pet Selection Panel: " + (petSelectionPanel != null ? "EXISTS" : "NULL"));
        Debug.Log("Pet 1 Name Creation Panel: " + (pet1NameCreationPanel != null ? "EXISTS" : "NULL"));
        
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
        
        if (pet1NameCreationPanel != null)
        {
            pet1NameCreationPanel.SetActive(true);
            Debug.Log("Pet 1 (Skeleton Warrior) Name Panel set to ACTIVE");
            Debug.Log("Pet1NameCreationPanel activeSelf: " + pet1NameCreationPanel.activeSelf);
            Debug.Log("Pet1NameCreationPanel activeInHierarchy: " + pet1NameCreationPanel.activeInHierarchy);
        }
        else
        {
            Debug.LogError("pet1NameCreationPanel is NULL! Assign the Pet 1 name panel in Inspector!");
        }
    }

    public void SelectWolfMage()
    {
        selectedPetType = "WolfMage";
        Debug.Log("=== SelectWolfMage called - Going to Pet 2 Name Panel ===");
        Debug.Log("Pet Selection Panel: " + (petSelectionPanel != null ? "EXISTS" : "NULL"));
        Debug.Log("Pet 2 Name Creation Panel: " + (pet2NameCreationPanel != null ? "EXISTS" : "NULL"));
        
        if (petSelectionPanel != null) petSelectionPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
        
        if (pet2NameCreationPanel != null)
        {
            pet2NameCreationPanel.SetActive(true);
            Debug.Log("Pet 2 (Wolf Mage) Name Panel set to ACTIVE");
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
        // Create user object with pet data
        UserProgress userProgress = new UserProgress(email, 1, 0, petType, petName);
        string json = JsonUtility.ToJson(userProgress);

        // Save to Database: users/USER_ID/
        dbReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("User profile saved with Pet: " + petType + ", Pet Name: " + petName);
                
                // Go to game menu
                if (pet1NameCreationPanel != null) pet1NameCreationPanel.SetActive(false);
                if (pet2NameCreationPanel != null) pet2NameCreationPanel.SetActive(false);
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
        pet1NameCreationPanel.SetActive(false);
        pet2NameCreationPanel.SetActive(false);
        petSelectionPanel.SetActive(true);
    }

    public void GoBackRegister()
    {
        Debug.Log("Going back to Register");
        petSelectionPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    private void CreateNewUserProfile(string userId, string email)
    {
        // This is only used for healing missing data during login
        UserProgress initialProgress = new UserProgress(email, 1, 0, "Unknown", "Pet");
        string json = JsonUtility.ToJson(initialProgress);

        dbReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("User data healed in Database.");
            }
        });
    }

    // ================= LOGIN LOGIC =================

    public void OnLoginButtonPress()
    {
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        // Validate email format
        if (string.IsNullOrEmpty(email) || !email.Contains("@") || !email.Contains("."))
        {
            ShowWrongFormatNotification();
            return;
        }

        // Validate password length
        if (password.Length < 6)
        {
            ShowWrongFormatNotification();
            return;
        }

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
        var dataTask = dbReference.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => dataTask.IsCompleted);

        if (dataTask.Exception != null)
        {
            Debug.LogError("Failed to load data.");
        }
        else if (dataTask.Result.Value == null)
        {
            // Auth exists, but Database data is missing (rare edge case)
            Debug.LogWarning("No data found for this user.");
            CreateNewUserProfile(userId, auth.CurrentUser.Email); // Heal the data
        }
        else
        {
            // Data retrieved successfully!
            DataSnapshot snapshot = dataTask.Result;
            string json = snapshot.GetRawJsonValue();
            
            // Convert JSON back to Object
            UserProgress loadedProgress = JsonUtility.FromJson<UserProgress>(json);
            
            Debug.Log("Loaded Level: " + loadedProgress.level);
            
            // Show "Login Successful" and go to game menu
            ShowLoginSuccessNotification();
        }
    }

    // ================= UI HELPERS =================

    public void SwitchToRegister()
    {
        Debug.Log("Switching to Register Panel");
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        HideAllNotifications();
        
        // Clear login input fields
        if (loginEmailInput != null) loginEmailInput.text = "";
        if (loginPasswordInput != null) loginPasswordInput.text = "";
    }

    public void SwitchToLogin()
    {
        Debug.Log("Switching to Login Panel");
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        HideAllNotifications();
        
        // Clear register input fields
        if (registerEmailInput != null) registerEmailInput.text = "";
        if (registerPasswordInput != null) registerPasswordInput.text = "";
    }

    void LoadGameScene()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    // ================= NOTIFICATION HANDLERS =================

    private void HideAllNotifications()
    {
        if (invalidLoginPanel != null) invalidLoginPanel.SetActive(false);
        if (loginSuccessPanel != null) loginSuccessPanel.SetActive(false);
        if (accountExistsPanel != null) accountExistsPanel.SetActive(false);
        if (accountCreatedPanel != null) accountCreatedPanel.SetActive(false);
        if (wrongFormatPanel != null) wrongFormatPanel.SetActive(false);
    }

    private void ShowInvalidLoginNotification()
    {
        Debug.Log("Notification: Invalid Login Details");
        HideAllNotifications();
        if (invalidLoginPanel != null)
        {
            invalidLoginPanel.SetActive(true);
            StartCoroutine(HideNotificationAfterDelay(invalidLoginPanel, 2f));
        }
    }

    private void ShowLoginSuccessNotification()
    {
        Debug.Log("Notification: Login Successful - Loading game menu");
        HideAllNotifications();
        if (loginSuccessPanel != null)
        {
            loginSuccessPanel.SetActive(true);
            StartCoroutine(HideNotificationAndLoadGame(loginSuccessPanel, 2f));
        }
        else
        {
            LoadGameScene();
        }
    }

    private void ShowAccountExistsNotification()
    {
        Debug.Log("Notification: Account already exists - Redirecting to login");
        HideAllNotifications();
        if (accountExistsPanel != null)
        {
            accountExistsPanel.SetActive(true);
            StartCoroutine(HideNotificationAndSwitchToLogin(accountExistsPanel, 2f));
        }
        else
        {
            SwitchToLogin();
        }
    }

    private void ShowAccountCreatedNotification()
    {
        Debug.Log("Notification: Account created successfully - Moving to pet selection");
        HideAllNotifications();
        if (accountCreatedPanel != null)
        {
            accountCreatedPanel.SetActive(true);
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
            wrongFormatPanel.SetActive(true);
            StartCoroutine(HideNotificationAfterDelay(wrongFormatPanel, 2f));
        }
    }

    private void ShowPetSelectionPanel()
    {
        Debug.Log("Showing Pet Selection Panel");
        registerPanel.SetActive(false);
        loginPanel.SetActive(false);
        gamePanel.SetActive(false);
        pet1NameCreationPanel.SetActive(false);
        pet2NameCreationPanel.SetActive(false);
        petSelectionPanel.SetActive(true);
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

    private IEnumerator HideNotificationAndSwitchToLogin(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null) panel.SetActive(false);
        SwitchToLogin();
    }
}

// ================= DATA CLASS =================

[System.Serializable]
public class UserProgress
{
    public string email;
    public int level;
    public int xp;
    public string pet; // Pet type (e.g., "Dragon", "Turtle")
    public string petName; // Custom pet name

    // Constructor
    public UserProgress(string email, int level, int xp, string pet, string petName)
    {
        this.email = email;
        this.level = level;
        this.xp = xp;
        this.pet = pet;
        this.petName = petName;
    }
}