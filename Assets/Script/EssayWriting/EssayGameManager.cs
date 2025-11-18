using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class EssayGameManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text[] essaySlots; // pg1 - pg4
    public TMP_Text timerTextDisplay;

    [Header("Data")]
    public TextAsset jsonFile;

    [Header("Settings")]
    public float gameDuration = 75.0f;
    public bool autoSkipSpaces = true;

    [Header("Colors")]
    public string colorCorrect = "#000000";
    public string colorDefault = "#808080";
    public string colorError = "#FF0000";

    // --- Data ---
    [System.Serializable] public class EssayTopicData { public string topicID; public string[] sentences; }
    [System.Serializable] public class EssayData { public EssayTopicData[] topics; }

    private string[] targetLines;
    private int currentLineIndex = 0;
    private int currentCharIndex = 0;
    private float currentTimer;
    private bool isGameActive = false;
    private bool isFlashingError = false;

    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;

    void Start()
    {
        if (jsonFile != null && essaySlots.Length > 0) PrepareGameContent();
    }

    void PrepareGameContent()
    {
        var data = JsonUtility.FromJson<EssayData>(jsonFile.text);
        if (data == null) return;

        var topic = data.topics[Random.Range(0, data.topics.Length)];
        var pool = new List<string>(topic.sentences);

        // Shuffle
        for (int i = 0; i < pool.Count; i++)
        {
            string temp = pool[i]; int r = Random.Range(i, pool.Count);
            pool[i] = pool[r]; pool[r] = temp;
        }

        targetLines = new string[Mathf.Min(essaySlots.Length, pool.Count)];
        for (int i = 0; i < targetLines.Length; i++)
        {
            // Bersihkan spasi ganda
            targetLines[i] = System.Text.RegularExpressions.Regex.Replace(pool[i].Trim(), @"\s+", " ");
        }

        StartGame();
    }

    void StartGame()
    {
        currentTimer = gameDuration;
        currentLineIndex = 0;
        currentCharIndex = 0;
        isGameActive = true;

        for (int i = 0; i < essaySlots.Length; i++)
        {
            if (i < targetLines.Length) essaySlots[i].text = $"<color={colorDefault}>{targetLines[i]}</color>";
            else essaySlots[i].text = "";
        }

        if (autoSkipSpaces) SkipSpace();
        UpdateVisuals();
    }

    void Update()
    {
        if (!isGameActive) return;

        // HANYA UPDATE TIMER DI SINI
        currentTimer -= Time.deltaTime;
        if (timerTextDisplay)
        {
            int m = Mathf.FloorToInt(currentTimer / 60F);
            int s = Mathf.FloorToInt(currentTimer % 60F);
            timerTextDisplay.text = $"{m}:{s:00}";
        }
        if (currentTimer <= 0) GameOver();
    }

    // --- FUNGSI PUBLIK: Dipanggil oleh EssayInput.cs ---
    public void ReceiveInput(char typedChar)
    {
        if (!isGameActive) return;
        if (char.IsControl(typedChar)) return; // Abaikan Enter/Backspace

        string currentLine = targetLines[currentLineIndex];
        char targetChar = currentLine[currentCharIndex];

        // Rule Spasi
        if (autoSkipSpaces && char.IsWhiteSpace(typedChar))
        {
            if (!isFlashingError) StartCoroutine(FlashError());
            return;
        }

        // Cek Huruf (Case Insensitive)
        if (char.ToLower(typedChar) == char.ToLower(targetChar))
        {
            currentCharIndex++;

            if (autoSkipSpaces) SkipSpace();

            if (currentCharIndex >= currentLine.Length)
            {
                currentLineIndex++;
                currentCharIndex = 0;
                if (currentLineIndex >= targetLines.Length) { GameWin(); return; }
                if (autoSkipSpaces) SkipSpace();
            }
            UpdateVisuals();
        }
        else
        {
            if (!isFlashingError) StartCoroutine(FlashError());
        }
    }

    void SkipSpace()
    {
        string line = targetLines[currentLineIndex];
        while (currentCharIndex < line.Length && char.IsWhiteSpace(line[currentCharIndex]))
        {
            currentCharIndex++;
        }
    }

    void UpdateVisuals()
    {
        if (isFlashingError) return;
        if (currentLineIndex < targetLines.Length)
        {
            string line = targetLines[currentLineIndex];
            string done = line.Substring(0, currentCharIndex);
            string left = line.Substring(currentCharIndex);
            essaySlots[currentLineIndex].text = $"<color={colorCorrect}>{done}</color><color={colorDefault}>{left}</color>";
        }
        for (int i = 0; i < currentLineIndex; i++) essaySlots[i].text = $"<color={colorCorrect}>{targetLines[i]}</color>";
    }

    IEnumerator FlashError()
    {
        isFlashingError = true;
        string line = targetLines[currentLineIndex];
        string done = line.Substring(0, currentCharIndex);
        char errChar = line[currentCharIndex];
        string rest = (currentCharIndex + 1 < line.Length) ? line.Substring(currentCharIndex + 1) : "";

        essaySlots[currentLineIndex].text = $"<color={colorCorrect}>{done}</color><color={colorError}>{errChar}</color><color={colorDefault}>{rest}</color>";
        yield return new WaitForSeconds(0.2f);
        isFlashingError = false;
        UpdateVisuals();
    }

    void GameWin() { isGameActive = false; Debug.Log("MENANG!"); OnGameWin?.Invoke(); }
    void GameOver() { isGameActive = false; Debug.Log("KALAH!"); OnGameLose?.Invoke(); }
}