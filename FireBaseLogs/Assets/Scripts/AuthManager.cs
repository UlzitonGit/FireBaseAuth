using System;
using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Text.RegularExpressions;
using TMPro;

public class AuthManager : MonoBehaviour
{
    public DependencyStatus dependencyStatus;
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    


    private FirebaseAuth auth;
    private FirebaseUser user;
    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChange;
        AuthStateChange(this, null);
    }

    private void AuthStateChange(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed Out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);
            
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseEx.ErrorCode;
            
            string failedMassage = "Login Failed! Because ";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMassage += "Invalid Email";
                    break;
                case AuthError.WrongPassword :
                    failedMassage += "Wrong Password";
                    break;
                case AuthError.MissingEmail :
                    failedMassage += "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    failedMassage += "Missing Password";
                    break;
                default:
                    failedMassage += "Unknown Error";
                    break;
            }
        }
        else
        {
            user = loginTask.Result.User;
            
            Debug.Log("Login Success " + user.DisplayName);
        }
        
    }
    public void Register()
    {
        StartCoroutine(RegisterAsync(emailLoginField.text, passwordLoginField.text));
    }
    private IEnumerator RegisterAsync(string email, string password)
    {
        if (email == "")
        {
            Debug.LogError("email field is empty");
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogError(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Registration Failed! Becuase ";
                switch (authError)
                {
                    case AuthError.InvalidEmail:
                        failedMessage += "Email is invalid";
                        break;
                    case AuthError.WrongPassword:
                        failedMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failedMessage += "Email is missing";
                        break;
                    case AuthError.MissingPassword:
                        failedMessage += "Password is missing";
                        break;
                    default:
                        failedMessage = "Registration Failed";
                        break;
                }

                Debug.Log(failedMessage);
            }
            else
            {
                user = registerTask.Result.User;

                UserProfile userProfile = new UserProfile { DisplayName = name };

                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    user.DeleteAsync();

                    Debug.LogError(updateProfileTask.Exception);

                    FirebaseException firebaseException = updateProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;


                    string failedMessage = "Profile update Failed! Becuase ";
                    switch (authError)
                    {
                        case AuthError.InvalidEmail:
                            failedMessage += "Email is invalid";
                            break;
                        case AuthError.WrongPassword:
                            failedMessage += "Wrong Password";
                            break;
                        case AuthError.MissingEmail:
                            failedMessage += "Email is missing";
                            break;
                        case AuthError.MissingPassword:
                            failedMessage += "Password is missing";
                            break;
                        default:
                            failedMessage = "Profile update Failed";
                            break;
                    }

                    Debug.Log(failedMessage);
                }
                else
                {
                    Debug.Log("Registration Sucessful Welcome " + user.DisplayName);
                }
            }
        }
    }

}