using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    [SerializeField]
    private PopupMessage popupMessage;


    private bool SecondClick = false;

    // Start is called before the first frame update
    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(ExitAction);
    }

    public void OnDestroy()
    {
        Button button = GetComponent<Button>();
        button.onClick.RemoveListener(ExitAction);

    }

    void ExitAction()
    {
        if(SecondClick)
        {
            GameEvents.RemoveUserFromGame(PlayerPrefs.GetString("roomId"));
            SceneManager.LoadScene(1);
            PlayerPrefs.DeleteKey("roomId");
        }
        else
        {
            SecondClick = true;
            popupMessage.ShowPopup("Press twice to exit");
            Task.Delay(2000).ContinueWith((task) =>
            {
                SecondClick = false;
            });
        }

    }

}
