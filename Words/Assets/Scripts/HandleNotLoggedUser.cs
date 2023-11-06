using UnityEngine;

public class HandleNotLoggedUser : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

}
