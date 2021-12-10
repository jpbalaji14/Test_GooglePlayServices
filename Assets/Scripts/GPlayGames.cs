using UnityEngine;
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase;
using Firebase.Database;

public class GPlayGames : MonoBehaviour
{
    //public int playerScore;
    //string leaderboardID = "";
    //string achievementID = "";
    public static PlayGamesPlatform platform;
    string authCode;
    public Text DebugText;
    
    //Firebase Authenication
    private FirebaseAuth mAuth;
    public Firebase.Auth.FirebaseUser _newUser;
    //Firebase Database References
    private DatabaseReference mReference;
    Player playerDetails = new Player();

    void Start()
    {
        GooglePlayGamesInitialize();
        FirebaseUserDetails();
        // UnlockAchievement();
    }
    private void GooglePlayGamesInitialize()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().RequestServerAuthCode(false /*forceRefresh*/).Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
    }
    private void FirebaseUserDetails()
    {
        mReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerDetails._playerCurrentLevel = 50;
        playerDetails._coins = 20;
        playerDetails._energy = 50;
    }

    public void SignIn()
    {
        Social.Active.localUser.Authenticate(success =>
        {
            if (success)
            {
                Debug.Log("Logged in successfully");
                authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                AuthWithFirebase();
            }
            else
            {
                Debug.Log("Login Failed");
            }
        });
    }
    private void AuthWithFirebase()
    {
        mAuth = FirebaseAuth.DefaultInstance;
        Credential credential = PlayGamesAuthProvider.GetCredential(authCode);
        mAuth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                DebugText.text = "SignInWithCredentialAsync was canceled";
            }
            if (task.IsFaulted)
            {
                DebugText.text = "SignInWithCredentialAsync encountered an error: " + task.Exception;
            }
            _newUser = task.Result;
            playerDetails._playerName = _newUser.DisplayName;
            playerDetails._playerID = _newUser.UserId;
            SaveNewUser(this._newUser.UserId);
            DebugText.text = "User Signed in Firebase";
        });
    }
    public void SaveNewUser(string userId)
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        //string userNameId =userId;
        string json = JsonUtility.ToJson(playerDetails);
        mReference.Child("Google Play Users").Child(currentUser.UserId).SetRawJsonValueAsync(json);
    }
    //public void AddScoreToLeaderboard()
    //{
    //    if (Social.Active.localUser.authenticated)
    //    {
    //        Social.ReportScore(playerScore, leaderboardID, success => { });
    //    }
    //}

    //public void ShowLeaderboard()
    //{
    //    if (Social.Active.localUser.authenticated)
    //    {
    //        platform.ShowLeaderboardUI();
    //    }
    //}

    //public void ShowAchievements()
    //{
    //    if (Social.Active.localUser.authenticated)
    //    {
    //        platform.ShowAchievementsUI();
    //    }
    //}

    //public void UnlockAchievement()
    //{
    //    if (Social.Active.localUser.authenticated)
    //    {
    //        Social.ReportProgress(achievementID, 100f, success => { });
    //    }
    //}
}
