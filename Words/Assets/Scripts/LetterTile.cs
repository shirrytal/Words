using UnityEngine;
using TMPro;

public class LetterTile : MonoBehaviour
{
    [SerializeField]
    private TMP_Text l_text;

    public void SetLetter(char letter) {l_text.text = letter.ToString(); }

    public char GetLetter()  {return l_text.text[0];}

    private void Awake()   {  l_text = GetComponentInChildren<TMP_Text>();  }
}
