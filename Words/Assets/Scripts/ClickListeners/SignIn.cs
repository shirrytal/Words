using System.Threading.Tasks;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignIn : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI emailText;

    [SerializeField]
    private TextMeshProUGUI passwordText;

    [SerializeField]
    private PopupMessage popupMessage;
    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(SignInAction);
    }

    void OnDestroy()
    {
        Button button = GetComponent<Button>();
        button.onClick.RemoveListener(SignInAction);
    }
    void SignInAction()
    {
        string email = emailText.text;
        string password = passwordText.text;
        if (email.Length == 0 || password.Length == 0)
        {
            popupMessage.ShowPopup("Email or password is empty");
            return;
        }
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password)
        .ContinueWithOnMainThread((task) =>
        {
            if (task.IsCompletedSuccessfully)
            {
                popupMessage.ShowPopup("Signing in user " + task.Result.User.Email);
                Task.Delay(1000).ContinueWithOnMainThread((task) =>
                {
                    SceneManager.LoadScene(1);
                });
            }
            else
            {
                popupMessage.ShowPopup(task.Exception.Message);
            }
        });


    }

}
