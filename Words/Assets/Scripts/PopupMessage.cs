using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class PopupMessage : MonoBehaviour
{

    public void ShowPopup(string message)
    {
        GetComponentInChildren<TMP_Text>().text = message;
        gameObject.SetActive(true);
        Task.Delay(3000).ContinueWithOnMainThread((task) =>
        {
            Hide();
        });
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Start()
    {
        Hide();
    }
}
