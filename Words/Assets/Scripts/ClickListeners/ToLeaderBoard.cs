using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ToLeaderBoard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(ToLeaderBoardAction);
    }

    void OnDestroy()
    {
        Button button = GetComponent<Button>();
        button.onClick.RemoveListener(ToLeaderBoardAction);
    }

    void ToLeaderBoardAction()
    {
        SceneManager.LoadSceneAsync(5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
