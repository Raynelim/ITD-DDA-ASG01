using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using TMPro; // Use this if you are using TextMeshPro for UI

public class LoginRegisterManager : MonoBehaviour
{
    // --- UI ELEMENTS (Drag these from the Inspector) ---

    [Header("UI Sections")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject loggedInPanel;

    [Header("Login Inputs")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;

    [Header("Register Inputs")]
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;

    [Header("Status & Info")]
    public TMP_Text statusText;
    public TMP_Text loggedInUserText;

    // --- FIREBASE VARIABLES ---

    private FirebaseAuth auth;
    private FirebaseUser user;

    // =========================================================
    // 1. INITIALIZATION
    // =========================================================

    void Start()
    {
        InitializeFirebase();
        // Start by showing the login screen
        SwitchUI(loginPanel); 
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Set up the authentication instance
                auth = FirebaseAuth.DefaultInstance;
                auth.StateChanged += AuthStateChanged;
                AuthStateChanged(this, null);
                Debug.Log("Firebase is ready.");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    // =========================================================
    // 2. UI SWITCHING LOGIC (Called by Button OnClick() events)
    // =========================================================

    public void ShowLogin()
    {
        SwitchUI(loginPanel);
    }

    public void ShowRegister()
    {
        SwitchUI(registerPanel);
    }

    private void SwitchUI(GameObject activePanel)
    {
        // Hide all panels first
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        loggedInPanel.SetActive(false);
        
        // Show the desired panel
        activePanel.SetActive(true);
        statusText.text = ""; // Clear status when switching
    }

    // =========================================================
    // 3. FIREBASE AUTHENTICATION FUNCTIONS
    // =========================================================

    public void RegisterButton()
    {
        RegisterUser(registerEmailInput.text, registerPasswordInput.text);
    }

    public void LoginButton()
    {
        LoginUser(loginEmailInput.text, loginPasswordInput.text);
    }

    public void LogoutButton()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
    }

    private void RegisterUser(string email, string password)
    {
        statusText.text = "Registering...";
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Registration was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"Registration encountered an error: {task.Exception}");
                // Display user-friendly error
                UnityMainThreadDispatcher.Instance().Enqueue(() => { 
                    statusText.text = $"Registration Failed: {task.Exception.InnerExceptions[0].Message}";
                });
                return;
            }

            // Firebase user has been created. AuthStateChanged will handle UI update.
            user = task.Result.User;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", user.DisplayName, user.UserId);
        });
    }

    private void LoginUser(string email, string password)
    {
        statusText.text = "Logging in...";
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Login was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"Login encountered an error: {task.Exception}");
                // Display user-friendly error
                 UnityMainThreadDispatcher.Instance().Enqueue(() => { 
                    statusText.text = $"Login Failed: {task.Exception.InnerExceptions[0].Message}";
                });
                return;
            }

            // User is signed in. AuthStateChanged will handle UI update.
            user = task.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.UserId);
        });
    }

    // =========================================================
    // 4. AUTH STATE CHANGE LISTENER
    // =========================================================

    // This handles the UI update regardless of how the user signed in (login or register)
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = auth.CurrentUser != null;
            if (signedIn)
            {
                user = auth.CurrentUser;
                Debug.Log($"Signed in as {user.Email}");

                // Update UI on the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    loggedInUserText.text = $"Welcome, {user.Email}!";
                    SwitchUI(loggedInPanel);
                    loginEmailInput.text = "";
                    loginPasswordInput.text = "";
                });
            }
            else
            {
                Debug.Log("Signed out.");
                user = null;

                // Update UI on the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    SwitchUI(loginPanel); // Go back to login screen
                });
            }
        }
    }

    // Clean up the listener when the object is destroyed
    void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
        }
    }
}

// NOTE: You will need a UnityMainThreadDispatcher script to safely update the UI 
// from the asynchronous Firebase tasks. A common implementation can be found online.