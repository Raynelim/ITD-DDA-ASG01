using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using TMPro;
using Firebase.Auth;

public class DatabaseController : MonoBehaviour
{
    private Player myPlayer;

    public string Name;

    public TMP_InputField Email;
    public TMP_InputField Password;

    public void Signup()
    {
        var createUserTask = FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(Email.text, Password.text);

        createUserTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Can't create user!");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log($"User created, user ID is: {task.Result.User.UserId}");

                // SAVE THE USER'S PROFILE
            }

        });
    }

    public void SignIn()
    {
        var signInTask = FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(Email.text, Password.text);

        signInTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Can't sign in!");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log($"User signed in, user ID is: {task.Result.User.UserId}");

                // LOAD THE USER'S PROFILE
            }

        });
    }
}