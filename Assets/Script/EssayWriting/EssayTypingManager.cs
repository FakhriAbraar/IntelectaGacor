using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
// Tambahkan namespace ini agar support Input System baru jika terinstall
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EssayTypingManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Masukkan pg1, pg2, pg3, pg4 ke sini secara berurutan")]
    public TMP_Text[] essaySlots;
    public TMP_Text timerTextDisplay;

    [Header("Data Soal")]
    public TextAsset jsonFile;

    [Header("Game Settings")]
    public float gameDuration = 105.0f; // Waktu 105 Detik
    public bool autoSkipSpaces = true;

    [Header("Visual Feedback")]
    public string colorCorrect = "#000000"; // Hitam
    public string colorDefault = "#808080"; // Abu-abu
    public string colorError = "#FF0000";   // Merah

    // --- Struktur Data JSON ---
    [System.Serializable]
    public class EssayTopicData
    {
        public string topicID;
        public string[] sentences;
    }

    [System.Serializable]
    public class EssayData
    {
        public EssayTopicData[] topics;
    }
    // --------------------------

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
        Debug.Log("Script Dimulai (Versi Sanitasi Input).."); // DEBUG

        if (jsonFile == null)
        {
            Debug.LogError("ERROR FATAL: Slot 'Json File' di Inspector masih KOSONG (None). Masukkan file .json!");
            return;
        }

        if (essaySlots == null || essaySlots.Length == 0)
        {
            Debug.LogError("ERROR FATAL: Slot 'Essay Slots' belum diisi (Size 0)!");
            return;
        }

        for (int i = 0; i < essaySlots.Length; i++)
        {
            if (essaySlots[i] == null)
            {
                Debug.LogError($"ERROR: Essay Slot Element {i} masih kosong (None). Harap isi pg{i + 1}!");
                return;
            }
        }

        PrepareGameContent();
    }

    // Fungsi Sanitasi Teks: Hapus spasi ganda, spasi di awal/akhir.
    string SanitizeText(string input)
    {
        // 1. Hapus spasi di awal dan akhir
        string clean = input.Trim();
        // 2. Ganti semua spasi ganda menjadi spasi tunggal
        while (clean.Contains("  "))
        {
            clean = clean.Replace("  ", " ");
        }
        return clean;
    }

    void PrepareGameContent()
    {
        try
        {
            EssayData data = JsonUtility.FromJson<EssayData>(jsonFile.text);
            if (data == null || data.topics == null || data.topics.Length == 0)
            {
                Debug.LogError("JSON Gagal dibaca atau Format Salah!");
                return;
            }

            int randomTopicIdx = Random.Range(0, data.topics.Length);
            EssayTopicData selectedTopic = data.topics[randomTopicIdx];

            List<string> pool = new List<string>(selectedTopic.sentences);

            // Lakukan Shuffle (Fisher-Yates)
            for (int i = 0; i < pool.Count; i++)
            {
                string temp = pool[i];
                int r = Random.Range(i, pool.Count);
                pool[i] = pool[r];
                pool[r] = temp;
            }

            // Ambil dan Sanitasi 4 Kalimat Acak
            int linesNeeded = Mathf.Min(essaySlots.Length, pool.Count);
            targetLines = new string[linesNeeded];
            for (int i = 0; i < linesNeeded; i++)
            {
                targetLines[i] = SanitizeText(pool[i]); // <--- SANITASI DI SINI
                Debug.Log($"Baris {i}: '{targetLines[i]}'");
            }

            Debug.Log($"Game Siap! Total Baris Soal: {targetLines.Length}");
            StartGame();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EXCEPTION saat load JSON: {e.Message}");
        }
    }

    void StartGame()
    {
        currentTimer = gameDuration;
        currentLineIndex = 0;
        currentCharIndex = 0;
        isGameActive = true;

        // Reset Visual
        for (int i = 0; i < essaySlots.Length; i++)
        {
            if (essaySlots[i] == null) continue;

            if (i < targetLines.Length)
                essaySlots[i].text = $"<color={colorDefault}>{targetLines[i]}</color>";
            else
                essaySlots[i].text = "";
        }

        if (autoSkipSpaces) CheckForAutoSkip();
        UpdateVisuals();

        Debug.Log("Game Loop Aktif. SILAKAN KLIK LAYAR GAME DAN KETIK!");
    }

    void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) Keyboard.current.onTextInput += OnNewSystemTextInput;
#endif
    }

    void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) Keyboard.current.onTextInput -= OnNewSystemTextInput;
#endif
    }

    // New Input System
    void OnNewSystemTextInput(char c)
    {
        if (!isGameActive) return;
        if (c == '\b' || c == '\n' || c == '\r') return;

        Debug.Log($"[New Input System] DETEKSI INPUT: '{c}'");
        ValidateCharacter(c);
    }


    void Update()
    {
        if (!isGameActive) return;

        HandleTimer();

        // --- FALLBACK INPUT LAMA ---
        if (Input.anyKeyDown)
        {
            if (!string.IsNullOrEmpty(Input.inputString))
            {
                foreach (char c in Input.inputString)
                {
                    if (c == '\b' || c == '\n' || c == '\r') continue;
                    // Debug.Log($"[Old Input System] DETEKSI INPUT: '{c}'"); // Jika mau verbose
                    ValidateCharacter(c);
                }
            }
        }
    }

    void HandleTimer()
    {
        currentTimer -= Time.deltaTime;
        if (timerTextDisplay != null)
        {
            int minutes = Mathf.FloorToInt(currentTimer / 60F);
            int seconds = Mathf.FloorToInt(currentTimer % 60F);
            timerTextDisplay.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
        if (currentTimer <= 0) GameOver();
    }

    void ValidateCharacter(char typedChar)
    {
        if (currentLineIndex >= targetLines.Length) return;

        string currentLineData = targetLines[currentLineIndex];
        if (currentCharIndex >= currentLineData.Length) return;

        char targetChar = currentLineData[currentCharIndex];

        if (typedChar == targetChar)
        {
            currentCharIndex++;
            if (autoSkipSpaces) CheckForAutoSkip();
            UpdateVisuals();

            if (currentCharIndex >= currentLineData.Length)
            {
                currentLineIndex++;
                currentCharIndex = 0;

                if (currentLineIndex < targetLines.Length)
                {
                    if (autoSkipSpaces) CheckForAutoSkip();
                    UpdateVisuals();
                }
                else
                {
                    GameWin();
                }
            }
        }
        else
        {
            if (!isFlashingError) StartCoroutine(FlashErrorEffect());
        }
    }

    void CheckForAutoSkip()
    {
        if (currentLineIndex >= targetLines.Length) return;
        string line = targetLines[currentLineIndex];
        while (currentCharIndex < line.Length && char.IsWhiteSpace(line[currentCharIndex]))
        {
            currentCharIndex++;
        }
    }

    void UpdateVisuals()
    {
        if (currentLineIndex < targetLines.Length && !isFlashingError)
        {
            string line = targetLines[currentLineIndex];
            string finished = line.Substring(0, currentCharIndex);
            string remaining = line.Substring(currentCharIndex);
            essaySlots[currentLineIndex].text = $"<color={colorCorrect}>{finished}</color><color={colorDefault}>{remaining}</color>";
        }

        for (int i = 0; i < currentLineIndex; i++)
        {
            if (essaySlots[i] != null)
                essaySlots[i].text = $"<color={colorCorrect}>{targetLines[i]}</color>";
        }
    }

    IEnumerator FlashErrorEffect()
    {
        isFlashingError = true;
        string line = targetLines[currentLineIndex];
        string finished = line.Substring(0, currentCharIndex);
        char errorChar = line[currentCharIndex];
        string remaining = "";
        if (currentCharIndex + 1 < line.Length) remaining = line.Substring(currentCharIndex + 1);

        essaySlots[currentLineIndex].text = $"<color={colorCorrect}>{finished}</color><color={colorError}>{errorChar}</color><color={colorDefault}>{remaining}</color>";
        yield return new WaitForSeconds(0.15f);
        isFlashingError = false;
        UpdateVisuals();
    }

    void GameWin()
    {
        isGameActive = false;
        Debug.Log("GAME WON!");
        OnGameWin?.Invoke();
    }

    void GameOver()
    {
        isGameActive = false;
        currentTimer = 0;
        if (timerTextDisplay != null) timerTextDisplay.text = "00:00";
        Debug.Log("GAME OVER!");
        OnGameLose?.Invoke();
    }
}