using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordLenghts : MonoBehaviour
{
    [SerializeField]
    private GameObject tile;
    private TMP_Text letterText;

    private GameObject[] FiveLetterWord;
    private GameObject[] FourLetterWord_1;
    private GameObject[] FourLetterWord_2;

    private GameObject[] ThreeLetterWord_1;
    private GameObject[] ThreeLetterWord_2;

    private Dictionary<string, bool> captured;

    // Start is called before the first frame update
    void Start()
    {
        captured = new Dictionary<string, bool>();
        string[] fiveLetterWordTags = { "w_1_l1", "w_1_l2", "w_1_l3", "w_1_l4", "w_1_l5"};
        string[] fourLetterWordTags_1 = { "w_2_l1", "w_2_l2", "w_2_l3", "w_2_l4"};
        string[] fourLetterWordTags_2 = { "w_3_l1", "w_3_l2", "w_3_l3", "w_3_l4" };

        string[] threeLetterWordTags_1 = { "w_4_l1", "w_4_l2", "w_4_l3"};
        string[] threeLetterWordTags_2 = { "w_5_l1", "w_5_l2", "w_5_l3"};

        FiveLetterWord = new GameObject[fiveLetterWordTags.Length];
        FourLetterWord_1 = new GameObject[fourLetterWordTags_1.Length];
        FourLetterWord_2 = new GameObject[fourLetterWordTags_2.Length];
        ThreeLetterWord_1 = new GameObject[threeLetterWordTags_1.Length];
        ThreeLetterWord_2 = new GameObject[threeLetterWordTags_2.Length];

        for (int i = 0; i < fiveLetterWordTags.Length; i++)
            FiveLetterWord[i] = GameObject.FindWithTag(fiveLetterWordTags[i]);

        for (int i = 0; i < fourLetterWordTags_1.Length; i++)
            FourLetterWord_1[i] = GameObject.FindWithTag(fourLetterWordTags_1[i]);
        for (int i = 0; i < fourLetterWordTags_2.Length; i++)
            FourLetterWord_2[i] = GameObject.FindWithTag(fourLetterWordTags_2[i]);

        for (int i = 0; i < threeLetterWordTags_1.Length; i++)
            ThreeLetterWord_1[i] = GameObject.FindWithTag(threeLetterWordTags_1[i]);
        for (int i = 0; i < threeLetterWordTags_2.Length; i++)
            ThreeLetterWord_2[i] = GameObject.FindWithTag(threeLetterWordTags_2[i]);
    }

    public void SetWordCaptured(string word)
    {
        int length = word.Length;
        if(length == 5)
        {
            captured["FiveLetterWord"] = true;
            Mark(FiveLetterWord, word);
        }
        else if(length == 4) {
            if (captured.ContainsKey("FourLetterWord_1")  && captured["FourLetterWord_1"])
            {
                Mark(FourLetterWord_2, word);
                captured["FourLetterWord_2"] = true;
            }
            else
            {
                Mark(FourLetterWord_1, word);
                captured["FourLetterWord_1"] = true;
            }
        } else if(length == 3)
        {
            if (captured.ContainsKey("ThreeLetterWord_1") && captured["ThreeLetterWord_1"])
            {
                Mark(ThreeLetterWord_2, word);
                captured["ThreeLetterWord_2"] = true;
            }
            else
            {
                Mark(ThreeLetterWord_1, word);
                captured["ThreeLetterWord_1"] = true;
            }
        }
    }

    private void Mark(GameObject[] tiles, string word)
    {
        for(int i = 0; i < tiles.Length; i ++) {
            var TextComponent = tiles[i].GetComponentInChildren<TMP_Text>();
            TextComponent.text = word[i].ToString();
            TextComponent.color = Color.black;
            tiles[i].GetComponent<Image>().color = Color.green;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
