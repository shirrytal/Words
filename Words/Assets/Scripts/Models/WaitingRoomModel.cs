using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaitingRoomModel
{
    public List<User> users;

    public GridData gameData;

    public long time;
    public string roomId;
    public string status;

    public static WaitingRoomModel FromJson(string json)
    {
        return JsonUtility.FromJson<WaitingRoomModel>(json);
    }

    public List<User> GetUsers()
    {
        users ??= new List<User>();
        return users;
    }

    public string GetRoomId() { return roomId; }
    public void SetRoomId(string id) { this.roomId = id; }

    public void SetTime(long time)
    {
        this.time = time;
    }

    public long GetTime()
    {
        return time;
    }

    public void SetGameData(GridData gameData)
    {
        this.gameData = gameData;
    }
    public GridData GetGameData()
    {
        return gameData;
    }
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void SetUsers(List<User> users)
    {
        this.users = users;
    }

    public bool IsReady()
    {
        return status == "started";
    }

    public bool IsOpen()
    {
        return status == "open";
    }

    public bool IsClosed()
    {
        return status == "closed";
    }

    public bool isEnded()
    {
        return status == "ended";
    }

    public void SetStatus(string status)
    {
        this.status = status;
    }

    public string GetStatus()
    {
        return status;
    }


}
