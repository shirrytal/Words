using System.Collections.Generic;
using System.Linq;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;


public class ScoreList : MonoBehaviour
{
    public class Score
    {

        public bool IsBetter(Score other)
        {
            return GuessedWords > other.GuessedWords || (GuessedWords == other.GuessedWords && Time < other.Time);
        }
        private int _guessedWords;
        private long _time;
        private string _playerName;

        public int GuessedWords
        {
            get { return _guessedWords; }
            set { _guessedWords = value; }
        }

        public long Time
        {
            get { return _time; }
            set { _time = value; }
        }

        public string PlayerName
        {
            get {
                return _playerName; }
            set { _playerName = value; }
        }
        public Score(int guessed_words, string player_name)
        {
            this.GuessedWords = guessed_words;
            this.PlayerName = player_name;
        }

        public override string ToString()
        {
            return PlayerName +  " : " + GuessedWords  + " words" + " in " + Time + "s";
        }


        public static Score FromSnapshot(DataSnapshot snapshot)
        {
            int.TryParse(snapshot.Child("score").Value?.ToString(), out int score);
            int.TryParse(snapshot.Child("time").Value?.ToString(), out int time);
            var username = snapshot.Child("username").Value?.ToString();
            username += (FirebaseAuth.DefaultInstance.CurrentUser.Email.Split("@")[0].Equals(username) ? " (me)" : "");
            var s = new Score(score, username) { Time = time };
            return s;
        }
      
        public string ToSnapshotString()
        {
            return $"{{\"score\": {GuessedWords}, \"time\": {Time}, \"username\": \"{PlayerName}\"}}";
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var roomId = PlayerPrefs.GetString("roomId");
        GameEvents.GetRoomScores(roomId, CreateScoreList);
    }

    public void CreateScoreList(List<Score> scores)
    {
        VerticalLayoutGroup group = GetComponentInChildren<VerticalLayoutGroup>();
        // clear
        foreach (Transform child in group.transform)
            Destroy(child.gameObject);
        // add 4 players
        for (int i = 0; i < scores.Count; i++)
        {
            GameObject textObj = new GameObject();
            textObj.transform.SetParent(group.transform);
            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500, 50);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = scores[i].ToString();
            text.fontSize = 48;
            text.color = new Color(101,67,33);

            text.fontStyle = FontStyles.Bold;
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
