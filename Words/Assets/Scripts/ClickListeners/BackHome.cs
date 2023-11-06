using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class BackHome : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(BackHomeAction);
    }

    void OnDestroy()
    {
        Button button = GetComponent<Button>();
        button.onClick.RemoveListener(BackHomeAction);
    }

    void BackHomeAction()
    {
        SceneManager.LoadSceneAsync(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
