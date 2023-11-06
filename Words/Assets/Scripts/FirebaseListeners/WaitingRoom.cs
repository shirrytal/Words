using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using OnRoomChangeDelegate = System.EventHandler<Firebase.Database.ValueChangedEventArgs>;



public class CollectionReferences
{
    public static DatabaseReference WaitingRoomReference(string roomId)
    {
        return FirebaseDatabase.DefaultInstance.RootReference.Child("waitingRooms").Child(roomId);
    }
    public static Query LeaderBoard()
    {
        return FirebaseDatabase.DefaultInstance.RootReference.Child("leaderBoard").LimitToFirst(10);
    }
}

public class GameEvents
{
    public delegate void OnRoomChanged(WaitingRoomModel readyRoom);
    public delegate void RoomScoresConsumer(List<ScoreList.Score> scores);


    public static async Task AddLeaderBoardScore(ScoreList.Score score)
    {
        var snapshot = await CollectionReferences.LeaderBoard().OrderByChild("username").EqualTo(score.PlayerName).GetValueAsync();
        if(snapshot.ChildrenCount > 0)
        {
            var first = new List<DataSnapshot>(snapshot.Children)[0];
            ScoreList.Score lastScore = ScoreList.Score.FromSnapshot(first);
            if(score.IsBetter(lastScore))
                await first.Reference.SetRawJsonValueAsync(score.ToSnapshotString());
            return;
        }
        await CollectionReferences.LeaderBoard().Reference.Push().SetRawJsonValueAsync(score.ToSnapshotString());
    }
    public static void GetRoomScores(string roomId, RoomScoresConsumer consumer)
    {
        CollectionReferences.WaitingRoomReference(roomId).Child("users").GetValueAsync()
             .ContinueWithOnMainThread(task =>
             {
                 if (task.IsFaulted)
                 {
                     // Handle the error here
                     Debug.LogError(task.Exception.ToString());
                     return;
                 }
                 if (task.IsCompletedSuccessfully)
                 {
                     List<ScoreList.Score> scores;
                     DataSnapshot snapshot = task.Result;
                     scores = new(snapshot.Children.Select(scoreSnapshot => ScoreList.Score.FromSnapshot(scoreSnapshot)));
                     consumer.Invoke(scores);
                 }
             });
    }
    public static void RemoveUserFromGame(string roomId)
    {
            CollectionReferences.WaitingRoomReference(roomId)
                .Child("users")
                .OrderByChild("email")
                .EqualTo(Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email)
                .LimitToFirst(1)
                .Reference
                .RemoveValueAsync();
    }
    public static OnRoomChangeDelegate WaitingRoomListener(
        OnRoomChanged onRoomChanged)
    {
        return (_, snapshot) =>
         {
             if (snapshot.DatabaseError != null)
             {
                 Debug.Log(snapshot.DatabaseError.Message);
                 return;
             }
             var room = WaitingRoomModel.FromJson(snapshot.Snapshot.GetRawJsonValue());
             if (room == null) return;
             onRoomChanged(room);
         };
    }


    public delegate void ConsumeScoreList(List<(DataSnapshot, ScoreList.Score)> scores);
    public static void GetLeaderBoard(ConsumeScoreList consumer)
    {
        CollectionReferences.LeaderBoard().GetValueAsync()
          .ContinueWithOnMainThread(task =>
          {
          if (task.IsFaulted)
          {
              // Handle the error here
              Debug.LogError(task.Exception.ToString());
              return;
          }
            
          if (task.IsCompletedSuccessfully)
          {
              List<(DataSnapshot, ScoreList.Score)> scores = new();
              DataSnapshot snapshot = task.Result;
              foreach (DataSnapshot childSnapshot in snapshot.Children)
                   scores.Add((childSnapshot, ScoreList.Score.FromSnapshot(childSnapshot)));
              consumer.Invoke(scores);
          } else { consumer.Invoke(new List<(DataSnapshot, ScoreList.Score)>()); }
        });
    }


    public delegate void ConsumeMinScore((DataSnapshot, ScoreList.Score)? minScore, int scoreSize);
    public static void GetMinScoreLeaderBoard(ConsumeMinScore consumer)
    {
        GetLeaderBoard((scores) =>
        {
            scores.Sort((x, y) =>
            {
                int result = x.Item2.GuessedWords.CompareTo(y.Item2.GuessedWords);

                // For minimum score but highest time (if scores are equal)
                if (result == 0)
                    return y.Item2.Time.CompareTo(x.Item2.Time); // Order matters here
                return result;
            });
            if (scores.Count > 0)
                consumer.Invoke(scores[0],scores.Count);
            else
                consumer.Invoke(null, 0); ;
        });
    }

    public static void UpdateUserScore(string roomId, int score, long time)
    {
        var email = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email;
        var query = CollectionReferences.WaitingRoomReference(roomId)
                        .Child("users")
                        .OrderByChild("email")
                        .EqualTo(email)
                        .LimitToFirst(1);

        ScoreList.Score s = new(score, email.Split("@")[0])
        {
            Time = time
        };

     
        query.GetValueAsync().ContinueWith(task => {
            DataSnapshot snapshot = task.Result;
            if (snapshot.HasChildren)
            {
                DataSnapshot userSnapshot = null;
                foreach (var child in snapshot.Children)
                {
                    userSnapshot = child;
                    break;
                }

                if (userSnapshot != null)
                {
                    var userScoreRef = userSnapshot.Reference.Child("score");
                    var userTimeRef = userSnapshot.Reference.Child("time");
                    userTimeRef.SetValueAsync(time);
                    userScoreRef.SetValueAsync(score);
                }
            }
        });

        GetMinScoreLeaderBoard(async (minScore, size) =>
        {
            if(size < 10)
            {
                await AddLeaderBoardScore(s);
                return;
            }
            if (s.IsBetter(minScore?.Item2))
            {
                var snapshot = await CollectionReferences.LeaderBoard().GetValueAsync();

                if (snapshot.Exists && snapshot.ChildrenCount > 0)
                {
                    DataSnapshot targetSnap = null;
                    foreach (var snap in snapshot.Children)
                    {
                        var name = (string)snap.Child("username").Value;
                        if (s.PlayerName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            targetSnap = snap;
                            break;
                        }
                    }

                    if (targetSnap != null)
                    {
                        var scoreEarlier = ScoreList.Score.FromSnapshot(targetSnap);
                        if (s.IsBetter(scoreEarlier))
                        {
                            await targetSnap.Reference.SetRawJsonValueAsync(s.ToSnapshotString());
                            return;  // Return early after updating score
                        }
                    }
                }

                // If no match is found, or the snapshot doesn't exist, then set the minimum score
                   await minScore?.Item1.Reference.RemoveValueAsync();
                   await minScore?.Item1.Reference.SetRawJsonValueAsync(s.ToSnapshotString());
            }

        });


    }

}

public class WaitingRoom : MonoBehaviour
{

    [SerializeField]
    private PlayerList playerList;


    [SerializeField]
    private TextMeshProUGUI statusText;


    [SerializeField]
    private TextMeshProUGUI timerText;


    private OnRoomChangeDelegate onRoomChangeDelegate;

    private WaitingRoomModel room;

    private Coroutine updateTimeCoroutine;

    private bool isReady = false;
    void Start()
    {
        onRoomChangeDelegate = GameEvents.WaitingRoomListener((room) =>
        {
            this.room = room;
            if (playerList != null)
                playerList.CreatePlayerList(room.GetUsers());
            else
            {
                RemoveListeners();
                return;
            }

            if (room.IsClosed())
            {
                statusText.text = "Prepare, the game will start in 30 seconds";
                return;
            }
            if (room.IsReady())
            {
                statusText.text = "Good luck!";
                Task.Delay(1000).ContinueWith((task) =>
                {
                    TaskUtils.RunOnMainThread(() =>
                    {
                        SceneManager.LoadSceneAsync(3);
                    });
                });
                isReady = true;
            }
        });
        updateTimeCoroutine = StartCoroutine(UpdateTime());
        string roomId = PlayerPrefs.GetString("roomId");
        // No room id was provided in transition to waiting room
        if (roomId == null) { Back(); return; }
        CollectionReferences.WaitingRoomReference(roomId).ValueChanged += onRoomChangeDelegate;
    }

    void RemoveListeners()
    {
        if (room != null && room.GetRoomId() != null)
            CollectionReferences.WaitingRoomReference(room.GetRoomId()).ValueChanged -= onRoomChangeDelegate;
    }

    void OnDestroy()
    {
        if (!isReady && room != null && room.GetUsers().Count > 1)
            GameEvents.RemoveUserFromGame(room.GetRoomId());
        RemoveListeners();
        StopCoroutine(updateTimeCoroutine);
    }


    void Back()
    {
        SceneManager.LoadScene(1);
       
        Debug.Log("Something went wrong upon joining wait room");
    }


    private IEnumerator UpdateTime()
    {
        while (true)
        {
            if (room != null)
            {
                var currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var startTime = room.GetTime(); // Assuming this is the time the event started, in Unix milliseconds

                // Calculate the time passed since the start time, in seconds
                var timePassed = (currentTime - startTime) / 1000.0;

                // Create a TimeSpan that represents 2 minutes
                TimeSpan twoMinutes = "closed".Equals(room.status) ? TimeSpan.FromMinutes(0.5) : TimeSpan.FromMinutes(2);

                // Subtract the time passed from 2 minutes to get the remaining time
                TimeSpan timeLeft = twoMinutes - TimeSpan.FromSeconds(timePassed);

                // Make sure timeLeft isn't negative
                if (timeLeft.TotalSeconds < 0 || room.status.Equals("started"))
                    timeLeft = TimeSpan.Zero;
                // Display the remaining time
                timerText.text = $"{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";

            }
            yield return new WaitForSeconds(1); // wait for 1 second before the next update
        }
    }

}
