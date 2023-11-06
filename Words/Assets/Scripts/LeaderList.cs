using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
public class LeaderList : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GameEvents.GetLeaderBoard(CreateScoreList);
    }

    public void CreateScoreList(List<(DataSnapshot, ScoreList.Score)> scores)
    {
        VerticalLayoutGroup group = GetComponentInChildren<VerticalLayoutGroup>();
        // clear
        foreach (Transform child in group.transform)
            Destroy(child.gameObject);
        for (int i = 0; i < scores.Count; i++)
        {
            GameObject textObj = new();
            textObj.transform.SetParent(group.transform);
            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500, 50);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = (i + 1) + ". " + (scores[i].Item2);
            text.fontSize = 48;
            text.color = new Color(101, 67, 33);

            text.fontStyle = TMPro.FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            textObj.transform.localScale = new Vector3(1, 1, 1);

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

    // Update is called once per frame
    void Update()
    {

    }
}
