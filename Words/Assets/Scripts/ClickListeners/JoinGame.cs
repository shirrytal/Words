using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JoinGame : MonoBehaviour
{
    private delegate void JoinRoom(WaitingRoomModel room);
    private bool onGoingRoomJoinRequest = false;
    [SerializeField]
    private PopupMessage popupMessage;

    // Start is called before the first frame update
    void Awake()
    {
        PlayerPrefs.SetString("roomId", null);
        Button button = GetComponent<Button>();
        button.onClick.AddListener(JoinRoomAction);
    }

    void onDestroy()
    {
        Button button = GetComponent<Button>();
        button.onClick.RemoveListener(JoinRoomAction);
    }

    void JoinRoomAction()
    {
        if (onGoingRoomJoinRequest) return;
        if(Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            popupMessage.ShowPopup("You must be logged in to join a room");
            return;
        }
        RequestJoinWaitingRoom((room) =>
        {
            if (room != null)
            {
                PlayerPrefs.SetString("roomId", room?.GetRoomId());
                SceneManager.LoadSceneAsync(2); 
                return;
            }
            onGoingRoomJoinRequest = false;
            popupMessage.ShowPopup("Was unable to join a waiting room..");
        });
    }

    private async void RequestJoinWaitingRoom(JoinRoom joinRoomCallback)
    {
        SetOnGoingRoomJoinRequest(true);
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        var roomRefQuery = reference.Child("waitingRooms");
        // Filter for status==open 
        Query query = roomRefQuery.OrderByChild("status").EqualTo("open");
        var roomRef = await query.GetValueAsync();

        if (!roomRef.Exists)
        {  // No rooms available
            popupMessage.ShowPopup("No rooms available.. please try again later");
            TaskUtils.RunOnMainThread(() => { joinRoomCallback.Invoke(null); });
            return;
        }

        WaitingRoomModel waitingRoom = null;
        var randomIndex = UnityEngine.Random.Range(0, roomRef.ChildrenCount);
        var randomRoom = roomRef.Children.GetEnumerator();
        for (int i = 0; i < randomIndex; i++)
            randomRoom.MoveNext();
        if (randomRoom.Current != null)
        {
            waitingRoom = WaitingRoomModel.FromJson(randomRoom.Current.GetRawJsonValue());
            var userId = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            var email = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email;
            var userName = email.Split('@')[0];
            waitingRoom.GetUsers().Add(new User(userName, email, userId));
            await randomRoom.Current.Reference.SetRawJsonValueAsync(waitingRoom.ToJson());
            TaskUtils.RunOnMainThread(() => { joinRoomCallback.Invoke(waitingRoom); });
            return;
        }
        else
            popupMessage.ShowPopup("No rooms available.. please try again later");
    }


    void SetOnGoingRoomJoinRequest(bool onGoing) { this.onGoingRoomJoinRequest = onGoing; }
}
