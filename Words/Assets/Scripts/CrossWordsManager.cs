using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using OnRoomChangeDelegate = System.EventHandler<Firebase.Database.ValueChangedEventArgs>;

public class CrossWordsManager : MonoBehaviour
{

    [SerializeField]
    private LetterGrid gridManager;


    [SerializeField]
    private TextMeshProUGUI clock;

    public delegate void TableProcessorDelegate(GridData data);

    private OnRoomChangeDelegate onRoomChangeDelegate;


    private WaitingRoomModel room;

    private Coroutine updateTimeCoroutine;

    void Start()
    {
        ConnectGameRoom();
        updateTimeCoroutine = StartCoroutine(UpdateTime());
    }

    private void ConnectGameRoom()
    {
        var roomId = PlayerPrefs.GetString("roomId");
        onRoomChangeDelegate = GameEvents.WaitingRoomListener((room) =>
        {
            if (room == null) return;
            this.room = room;
            var users = room.GetUsers();
            var hasUser = false;
            foreach (var u in users)
                if (u.email.Equals(FirebaseAuth.DefaultInstance.CurrentUser.Email))
                    hasUser = true;
            if (room.IsReady())
                gridManager.SetGridData(room.GetGameData());
            else if(hasUser)
            {
                CollectionReferences.WaitingRoomReference(roomId).ValueChanged -= onRoomChangeDelegate;
                if(PlayerPrefs.HasKey("roomId"))
                    SceneManager.LoadScene(4); // Go to score board
            }else
            {
                CollectionReferences.WaitingRoomReference(roomId).ValueChanged -= onRoomChangeDelegate;
            }
        });
        CollectionReferences.WaitingRoomReference(roomId).ValueChanged += onRoomChangeDelegate;
    }

    void OnDestroy()
    {
        try
        {
            var roomId = PlayerPrefs.GetString("roomId");
            CollectionReferences.WaitingRoomReference(roomId).ValueChanged -= onRoomChangeDelegate;
            if (room != null && !room.isEnded())
            {
                CollectionReferences.WaitingRoomReference(room.GetRoomId())
                .Child("users")
                .OrderByChild("email")
                .EqualTo(Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email)
                .LimitToFirst(1)
                .Reference
                .RemoveValueAsync();
                PlayerPrefs.DeleteKey("roomId");
            }
            else if (room != null && room.isEnded() && !gridManager.FinishedBoard())
            {
                var score = gridManager.GetScore();
                GameEvents.UpdateUserScore(roomId, score, gridManager.GetGameTime());
            }
        }
        catch { }
        StopCoroutine(updateTimeCoroutine);
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
                TimeSpan twoMinutes = TimeSpan.FromMinutes(2);
                // Subtract the time passed from 2 minutes to get the remaining time
                TimeSpan timeLeft = twoMinutes - TimeSpan.FromSeconds(timePassed);

                // Make sure timeLeft isn't negative
                if (timeLeft.TotalSeconds < 0)
                    timeLeft = TimeSpan.Zero;
                // Display the remaining time
                clock.text = $"{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";

            }
            yield return new WaitForSeconds(1); // wait for 1 second before the next update
        }
    }


}
