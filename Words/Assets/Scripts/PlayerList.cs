using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Auth;

public class PlayerList : MonoBehaviour
{

    List<TextMeshProUGUI> playerNames = new List<TextMeshProUGUI>();

    // Start is called before the first frame update
    void Start()
    {

    }


    public void CreatePlayerList(List<User> players)
    {
        VerticalLayoutGroup group = GetComponentInChildren<VerticalLayoutGroup>();
        // clear
        foreach (Transform child in group.transform)
            Destroy(child.gameObject);
        // add 4 players
        for (int i = 0; i < players.Count; i++)
        {
            GameObject textObj = new GameObject();
            textObj.transform.SetParent(group.transform);
            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500, 50);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = players[i].username + (players[i].email.Equals(FirebaseAuth.DefaultInstance.CurrentUser.Email) ?  " (me)" : "");
            text.fontSize = 48;
            text.color = Color.white;

            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            textObj.transform.localScale = new Vector3(1, 1, 1);

            playerNames.Add(text);

            // Add Line Break
            GameObject lineBreak = new GameObject();
            lineBreak.transform.SetParent(group.transform);
            RectTransform lineBreakTransform = lineBreak.AddComponent<RectTransform>();
            lineBreakTransform.sizeDelta = new Vector2(500, 1);
            Image lineBreakImage = lineBreak.AddComponent<Image>();
            lineBreakImage.color = Color.white;
            lineBreakTransform.localScale = new Vector3(1, 1, 1);

        }
    }
}
