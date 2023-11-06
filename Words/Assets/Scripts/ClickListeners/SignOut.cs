using UnityEngine;
using UnityEngine.UI;

public class SignOut : MonoBehaviour
{
    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(SignOutAction);
    }

    void OnDestroy()
    {
        Button button = GetComponent<Button>();
        button.onClick.RemoveListener(SignOutAction);
    }
    void SignOutAction()
    {
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
    }

}
