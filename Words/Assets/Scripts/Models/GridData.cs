using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridData
{
	public List<string> crosswordTable;
    public List<string> words;
    public long generateTime;

    public static GridData FromJson(string json)
    {
        return JsonUtility.FromJson<GridData>(json);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public Dictionary<string, object> ToDictionary()
    {
        // Use JsonUtility to convert ApiResponse to a dictionary
        var json = ToJson();
        var dict = JsonUtility.FromJson<Dictionary<string, object>>(json);
        return dict;
    }
}

