using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Firebase.Database;
using TMPro;

public class AuthManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private DataManager dataManager;
    public DependencyStatus dependencyStatus;
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public DatabaseReference DBreference;
    public GameObject scoreElement;
    public Transform scoreboardContent;

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
        DBreference = FirebaseDatabase.GetInstance("https://auth-3913d-default-rtdb.firebaseio.com/").RootReference;
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
            StartCoroutine(LoadUserData());  
            uiManager.ShowPlayerScore();
            uiManager.GameScreen();
            StartCoroutine(UpdateUsernameAuth(emailLoginField.text));
            StartCoroutine(UpdateUsernameDatabase(emailLoginField.text));
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
                    StartCoroutine(LoadUserData());
                    uiManager.GameScreen();
                    StartCoroutine(UpdateUsernameAuth(emailLoginField.text));
                    StartCoroutine(UpdateUsernameDatabase(emailLoginField.text));
                    Debug.Log("Registration Sucessful Welcome " + user.DisplayName);
                }
            }
        }
    }
    public void UpdateScore()
    {
        StartCoroutine(UpdateScore(dataManager.GetScore()));
    }
    private IEnumerator UpdateUsernameAuth(string _username)
    {
        
        UserProfile profile = new UserProfile { DisplayName = _username };
        
        Task ProfileTask = user.UpdateUserProfileAsync(profile);
       
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
       
    }

    public void UpdateLeaderboard()
    {
        StartCoroutine(LoadScoreboardData());
    }

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("username").SetValueAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    
    }
    private IEnumerator UpdateScore(int _score)
    {
        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("score").SetValueAsync(_score);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }

    }
     private IEnumerator LoadUserData()
    {
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(user.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            dataManager.SetScore(0);
            uiManager.ShowPlayerScore();
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            if(snapshot.Child("score").Value.ToString() != null)
                dataManager.SetScore(int.Parse(snapshot.Child("score").Value.ToString()));
            uiManager.ShowPlayerScore();
        }
    }

    private IEnumerator LoadScoreboardData()
    {
        Task<DataSnapshot> DBTask = DBreference.Child("users").OrderByChild("score").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                if (!childSnapshot.HasChild("username") || !childSnapshot.HasChild("score"))
                {
                    Debug.LogWarning($"User {childSnapshot.Key} missing username or score field");
                    continue;
                }
            
                string username = childSnapshot.Child("username")?.Value?.ToString();
                if (string.IsNullOrEmpty(username))
                {
                    Debug.LogWarning($"Username is null or empty for user {childSnapshot.Key}");
                    continue;
                }
            
                // Проверка корректности значения score
                object scoreValue = childSnapshot.Child("score")?.Value;
                if (scoreValue == null)
                {
                    Debug.LogWarning($"Score is null for user {username}");
                    continue;
                }
            
                int score;
                if (!int.TryParse(scoreValue.ToString(), out score))
                {
                    Debug.LogWarning($"Failed to parse score for user {username}: {scoreValue}");
                    continue;
                }
            
                // Создание элемента таблицы лидеров
                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
            
                // Проверка компонента LeadrBoardEntry
                LeadrBoardEntry entry = scoreboardElement.GetComponent<LeadrBoardEntry>();
                if (entry == null)
                {
                    Debug.LogError("LeadrBoardEntry component not found on instantiated prefab!");
                    Destroy(scoreboardElement);
                    continue;
                }
            
                entry.SetName(username, score);
            }
            
        }
    }

}