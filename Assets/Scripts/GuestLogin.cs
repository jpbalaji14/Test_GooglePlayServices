using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;
public class GuestLogin : MonoBehaviour
{
    FirebaseAuth mAuth;
    public Firebase.Auth.FirebaseUser newUser;
    public FirebaseUser _SignInuser;
    public FirebaseUser _existingUser;
    
    //public Text _guestPlayerName;
    public GameObject LoginButton;
    private string _getGuestName;
    bool CanSignIn = false;

   //Firebase Database References
   DatabaseReference mReference;
    Player playerDetails = new Player();

    //SignIn Guest Gplay
    string mAuthCode;
    void Awake()
    {     
        GooglePlayGamesInitialize();
        FirebaseUserDetails();
        AuthStateChange();
    }
    private void GooglePlayGamesInitialize()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().RequestServerAuthCode(false /*forceRefresh*/).Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
    }
    private void FirebaseUserDetails()
    {
        mAuth = FirebaseAuth.DefaultInstance;
        mReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerDetails._playerName = "Guest";
        playerDetails._playerCurrentLevel = 50;
        playerDetails._coins = 20;
        playerDetails._energy = 50;
    }
    void AuthStateChange()
    {
        mAuth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (mAuth.CurrentUser.IsAnonymous != null)
        {
            bool signedIn = mAuth.CurrentUser != null;
            if (!signedIn)
            {
                Debug.Log("Signed out " + _existingUser.UserId);
                //loginPanel.SetActive(true);
            }
            _existingUser = mAuth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + _existingUser.UserId);
                Debug.Log("Now u can Upgrade the Guest Account");
                CanSignIn = true;
            }
        }
        else
        {
            Debug.Log("Not Signed In bro");
        }
    }
    public void OnClickGuestLogin()
    {
        LoginButton.SetActive(false);        
        mAuth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            newUser = task.Result;
            _getGuestName = playerDetails._playerName;
            playerDetails._playerID = newUser.UserId;
            Debug.Log("UserName : " + playerDetails._playerName);
            Debug.Log("UserID : " + newUser.UserId);
            
            SaveNewUser(newUser.UserId);
        });
    }
    public void SaveNewUser(string userId)
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        string json = JsonUtility.ToJson(playerDetails);
        mReference.Child("Guest Users").Child(currentUser.UserId).SetRawJsonValueAsync(json);
    }
    public void ClickToLogin()
    {
        if (CanSignIn==true)
        {
            Social.Active.localUser.Authenticate(success =>
            {
                if (success)
                {
                    Debug.Log("Logged in successfully");
                    mAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                    AuthWithFirebase();
                }
                else
                {
                    Debug.Log("Login Failed");
                }
            });  
        }
    }
    void AuthWithFirebase()
    {
        Credential credential = PlayGamesAuthProvider.GetCredential(mAuthCode);
        mAuth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(task=> 
        {
            if (task.IsFaulted)
            {
                Debug.Log("signin encountered error" + task.Exception);
            }          
            _SignInuser = task.Result;

            //Database Details Update
            playerDetails._playerName = _SignInuser.DisplayName;
            playerDetails._playerID = _SignInuser.UserId;
            SaveGuestAsUser(_SignInuser.UserId);
        });     
    } 
    public void SaveGuestAsUser(string userId)
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        string json = JsonUtility.ToJson(playerDetails);
        mReference.Child("Google Play Users").Child(currentUser.UserId).SetRawJsonValueAsync(json);
        mReference.Child("Guest Users").Child(newUser.UserId).RemoveValueAsync();
    }
}