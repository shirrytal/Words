using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Ext;
public class LetterGrid : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField]
    private GameObject letterTilePrefab;

    [SerializeField]
    private Transform _camTransform;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private int gridWidth;

    [SerializeField]
    private int gridHeight;

    [SerializeField]
    private float spacing;

    private LetterTile[,] gridTiles;

    [SerializeField]
    private PopupMessage popupMessage;

    [SerializeField]
    private WordLenghts wordLengths;

    private long time;

    private Timer updateTimeTimer;
    private GridData gridData;
    private bool isPointerDown = false;
    private StringBuilder markingWord;
    private Dictionary<Vector2,bool> lastTilePositions;
    private Dictionary<Vector2, Color> tileColors;
    private Dictionary<string, bool> wordsCaptured;

    void Start()
    {
        gridTiles = new LetterTile[gridWidth, gridHeight];
        tileColors = new Dictionary<Vector2, Color>();
        wordsCaptured = new Dictionary<string, bool>();
        GenerateGrid();
        updateTimeTimer = new Timer(UpdateTimeCallback,null,0,1000);
    }

    public void UpdateTimeCallback(object state)
    {
        time += 1;
    }


    public void SetGridData(GridData data)
    {
        this.gridData = data;
        List<string> table = data.crosswordTable;
        for  (int i = 0; i < table.Count && i < 5; i++)
        {
            char[] rowLetters = table[i].ToCharArray();
            for (int j = 0; j < rowLetters.Length && j < 5; j++)
            {
                gridTiles[j, i].SetLetter(rowLetters[j]);
            }
        }
    }

    private void GenerateGrid()
    {
       
        float fullWidth = spacing * (gridWidth - 1);  // total width of all gaps between tiles
        float fullHeight = spacing * (gridHeight - 1); // total height of all gaps between tiles

        float startGridX = -fullWidth / 2;  // the x-coordinate of the left-most tile's center
        float startGridY = fullHeight / 2;  // the y-coordinate of the top-most tile's center
        ColorUtility.TryParseHtmlString("#601E1E", out Color color);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = new(startGridX + x * spacing, startGridY - y * spacing + 0.5f, -1);
                GameObject instantiatedTile = Instantiate(letterTilePrefab, position, Quaternion.identity, this.transform);
                LetterTile letterTile = instantiatedTile.GetComponent<LetterTile>();
                letterTile.SetLetter('-');
                gridTiles[x, y] = letterTile;
                tileColors[new(x, y)] = color;
            }
        }
    }

    public long GetGameTime()
    {
        return time;
    }

    public void OnPointerUp(PointerEventData eventData) {
        isPointerDown = false;
        if(IsGameWord(markingWord.ToString(), out string capturedWord)) {
            MarkWordCaptured( ref capturedWord );
        } else {
            ResetSelection();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPointerDown)
        {
            markingWord = new StringBuilder();
            lastTilePositions = new Dictionary<Vector2, bool>();
        }
        isPointerDown = true;
        // query a tile based on tap position
        KeyValuePair <LetterTile, Vector2> TileData = GetTileFromScreenPosition(eventData.position);
        // The click was outside the grid || the selected tile is already selected
        if (TileData.Key == null || lastTilePositions.ContainsKey(TileData.Value)) return;
        // Mark as selected
        lastTilePositions[TileData.Value] = true;
        // Access the clicked tile
        Image image = TileData.Key.GetComponent<Image>();
        // Change the color of the material to green
        image.color = new Color(255, 127, 80).Normalize();
        // Append the tile's letter to the marking word
        AppendToMarkingWord(TileData.Key.GetLetter());
    }

    private void AppendToMarkingWord(char v) { markingWord?.Append(v); }
       

    public void OnPointerMove(PointerEventData eventData) {
        if(isPointerDown) OnPointerDown(eventData);
    }


    public int GetScore() {
        return wordsCaptured.Keys.Count;
    }

    private void MarkWordCaptured(ref string capturedWord)
    {
        foreach (Vector2 letter_pos in lastTilePositions.Keys)
        {
            int x = Mathf.FloorToInt(letter_pos.x);
            int y = Mathf.FloorToInt(letter_pos.y);
            var Red = new Color(60, 179, 113).Normalize();
            gridTiles[x, y].GetComponent<Image>().color = Red;
            tileColors[letter_pos] = Red;
        }
        if (wordsCaptured.ContainsKey(capturedWord) && wordsCaptured[capturedWord]) return;
        wordsCaptured[capturedWord] = true;
        wordLengths.SetWordCaptured(capturedWord);
        if (FinishedBoard())
        {
            popupMessage.ShowPopup("You have found all the words in " + time + " seconds");
            var roomId = PlayerPrefs.GetString("roomId");
            GameEvents.UpdateUserScore(roomId, GetScore(), time);
        }
    }

    public bool FinishedBoard()
    {
        return wordsCaptured.Keys.Count == gridData.words.Count;
    }

    private void ResetSelection()
    {
        foreach (Vector2 letter_pos in lastTilePositions.Keys)
        {
            int x = Mathf.FloorToInt(letter_pos.x);
            int y = Mathf.FloorToInt(letter_pos.y);
            gridTiles[x, y].GetComponent<Image>().color = tileColors[letter_pos];
        }
    }



    private KeyValuePair<LetterTile, Vector2> GetTileFromScreenPosition(Vector2 ScreenPosition)
    {
        // Get world position
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(ScreenPosition);
        LetterTile targetTile = null;
        int x = 0, y = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if(gridTiles[j, i] == null) continue;
                if (Vector2.Distance(
                    gridTiles[j, i].GetComponent<Transform>().position,
                    new Vector2(worldPosition.x, worldPosition.y)) < spacing / 2)
                {
                    targetTile = gridTiles[j, i];
                    x = j; y = i;
                    break;
                }
            }
        }
        return new KeyValuePair<LetterTile, Vector2>(targetTile, new Vector2(x, y));
    }

    private bool IsGameWord(string word, out string wordOut)
    {
        foreach (string GameWord in gridData.words)
        {
            if (GameWord.Equals(word)
                || ReversedWord(GameWord).Equals(word)
                || IsPermutation(GameWord, word))
            {
                wordOut = GameWord;
                return true;
            }
        }
        wordOut = null;
        return false;
    }


    bool IsPermutation(string word, string other)
    {
        if (word.Length != other.Length)
            return false;

        Dictionary<char, int> wordCharCounts = new Dictionary<char, int>();

        // Count the characters in the word
        foreach (char wordChar in word)
        {
            if (wordCharCounts.ContainsKey(wordChar))
                wordCharCounts[wordChar]++;
            else
                wordCharCounts[wordChar] = 1;
        }

        // Check the characters in the other string
        foreach (char otherChar in other)
        {
            if (!wordCharCounts.ContainsKey(otherChar) || wordCharCounts[otherChar] == 0)
                return false;
            wordCharCounts[otherChar]--;
        }
        return true;
    }

    private string ReversedWord(string gameWord)
    {
        StringBuilder sb = new();
        int i = gameWord.Length - 1;
        while (i >= 0)
            sb.Append(gameWord[i--]);
        return sb.ToString();
    }


    public void OnDestroy()
    {
        updateTimeTimer.Dispose();
    }

}