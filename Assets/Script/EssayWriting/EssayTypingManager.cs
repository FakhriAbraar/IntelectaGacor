using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class EssayTypingManager : MonoBehaviour
{
    [Header("Komponen UI")]
    [Tooltip("Masukkan pg1, pg2, pg3, pg4 ke dalam array ini secara berurutan")]
    public TMP_Text[] essaySlots; // UBAHAN: Menggunakan Array TMP untuk pg1-pg4

    public TMP_Text timerTextDisplay;

    [Header("Sumber Soal (JSON)")]
    [Tooltip("Masukkan file EssayTopics.json di sini")]
    public TextAsset jsonFile;

    // Kita tidak perlu memilih topik manual lagi, karena akan di-randomize
    // public string topicToPlay = "reformasi_1998"; 

    [Header("Pengaturan Game")]
    public float gameDuration = 105.0f; // UBAHAN: Waktu diset ke 105 detik
    public bool autoSkipSpaces = true;

    [Header("Warna")]
    public string colorCorrect = "#000000"; // Hitam
    public string colorDefault = "#808080"; // Abu-abu
    public string colorError = "#FF0000";   // Merah

    // --- Data Classes untuk JSON ---
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
    // -------------------------------

    // State Variables
    private string[] targetLines; // Menyimpan kalimat target per baris
    private int currentLineIndex = 0; // Baris mana yang sedang diketik (0-3)
    private int currentCharIndex = 0; // Huruf ke berapa di baris tersebut

    private float currentTimer;
    private bool isGameActive = false;
    private bool isFlashingError = false;

    // Events
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;

    void Start()
    {
        if (jsonFile != null)
        {
            LoadContentFromJsonRandomly();
        }
        else
        {
            Debug.LogError("File JSON belum dimasukkan!");
        }
    }

    void LoadContentFromJsonRandomly()
    {
        try
        {
            EssayData data = JsonUtility.FromJson<EssayData>(jsonFile.text);

            if (data.topics == null || data.topics.Length == 0) return;

            // 1. Pilih Topik Secara Acak
            int randomTopicIndex = Random.Range(0, data.topics.Length);
            EssayTopicData selectedTopic = data.topics[randomTopicIndex];
            Debug.Log($"Topik terpilih: {selectedTopic.topicID}");

            if (selectedTopic.sentences.Length > 0)
            {
                // 2. Acak Urutan Kalimat (Shuffle) agar unik dan tidak sama
                List<string> shuffledList = new List<string>(selectedTopic.sentences);
                for (int i = 0; i < shuffledList.Count; i++)
                {
                    string temp = shuffledList[i];
                    int randomIndex = Random.Range(i, shuffledList.Count);
                    shuffledList[i] = shuffledList[randomIndex];
                    shuffledList[randomIndex] = temp;
                }

                // 3. Ambil kalimat sejumlah slot UI yang tersedia (misal 4 slot)
                int linesCount = Mathf.Min(essaySlots.Length, shuffledList.Count);
                targetLines = new string[linesCount];

                for (int i = 0; i < linesCount; i++)
                {
                    targetLines[i] = shuffledList[i];
                }

                StartGame();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON Error: " + e.Message);
        }
    }

    void Update()
    {
        if (!isGameActive) return;
        HandleTimer();
        HandleInput();
    }

    void StartGame()
    {
        if (targetLines == null || targetLines.Length == 0) return;

        // Reset State
        currentTimer = gameDuration;
        currentLineIndex = 0;
        currentCharIndex = 0;
        isGameActive = true;

        // Reset Visual Awal (Semua Text jadi Abu-abu)
        for (int i = 0; i < essaySlots.Length; i++)
        {
            if (i < targetLines.Length)
                essaySlots[i].text = $"<color={colorDefault}>{targetLines[i]}</color>";
            else
                essaySlots[i].text = ""; // Kosongkan jika slot berlebih
        }

        // Cek auto-skip untuk karakter pertama di baris pertama
        if (autoSkipSpaces) CheckForAutoSkip();

        UpdateActiveLineVisuals(); // Update tampilan baris pertama
    }

    void HandleTimer()
    {
        currentTimer -= Time.deltaTime;

        // Format waktu mm:ss
        int minutes = Mathf.FloorToInt(currentTimer / 60F);
        int seconds = Mathf.FloorToInt(currentTimer % 60F);

        if (timerTextDisplay != null)
            timerTextDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (currentTimer <= 0)
        {
            GameOver();
        }
    }

    void HandleInput()
    {
        if (!string.IsNullOrEmpty(Input.inputString))
        {
            foreach (char c in Input.inputString)
            {
                // Abaikan backspace atau enter
                if (c == '\b' || c == '\n' || c == '\r') continue;
                ValidateCharacter(c);
            }
        }
    }

    void ValidateCharacter(char typedChar)
    {
        // Cek apakah sudah selesai semua baris
        if (currentLineIndex >= targetLines.Length) return;

        string currentLine = targetLines[currentLineIndex];

        // Safety check
        if (currentCharIndex >= currentLine.Length) return;

        char targetChar = currentLine[currentCharIndex];

        // --- LOGIKA BENAR ---
        // Kita gunakan char.ToLower untuk toleransi kapital jika diinginkan, 
        // tapi biasanya typing game case-sensitive. Di sini kita buat Case-Sensitive sesuai contoh.
        if (typedChar == targetChar)
        {
            currentCharIndex++;

            // Cek Auto Skip (Spasi)
            if (autoSkipSpaces) CheckForAutoSkip();

            UpdateActiveLineVisuals();

            // Cek apakah baris ini sudah selesai?
            if (currentCharIndex >= currentLine.Length)
            {
                // Pindah ke baris berikutnya
                currentLineIndex++;
                currentCharIndex = 0; // Reset index huruf untuk baris baru

                // Jika masih ada baris berikutnya, cek auto skip untuk huruf pertamanya
                if (currentLineIndex < targetLines.Length)
                {
                    if (autoSkipSpaces) CheckForAutoSkip();
                    UpdateActiveLineVisuals();
                }
                else
                {
                    // Sudah tidak ada baris lagi -> MENANG
                    GameWin();
                }
            }
        }
        // --- LOGIKA SALAH ---
        else
        {
            if (!isFlashingError)
            {
                StartCoroutine(FlashErrorEffect());
            }
        }
    }

    // Melewati spasi secara otomatis di baris yang sedang aktif
    void CheckForAutoSkip()
    {
        if (currentLineIndex >= targetLines.Length) return;

        string currentLine = targetLines[currentLineIndex];

        while (currentCharIndex < currentLine.Length)
        {
            char nextChar = currentLine[currentCharIndex];
            if (char.IsWhiteSpace(nextChar))
            {
                currentCharIndex++;
            }
            else
            {
                break;
            }
        }
    }

    // Memperbarui warna teks HANYA pada baris yang sedang aktif
    void UpdateActiveLineVisuals()
    {
        if (isFlashingError || currentLineIndex >= targetLines.Length) return;

        string currentLine = targetLines[currentLineIndex];

        // Potong teks berdasarkan posisi kursor saat ini
        string correctPart = currentLine.Substring(0, currentCharIndex);
        string remainingPart = currentLine.Substring(currentCharIndex);

        // Format: Hitam (sudah diketik) + Abu-abu (belum)
        essaySlots[currentLineIndex].text = $"<color={colorCorrect}>{correctPart}</color><color={colorDefault}>{remainingPart}</color>";

        // Pastikan baris yang SUDAH LEWAT tetap hitam penuh (opsional, untuk keamanan visual)
        for (int i = 0; i < currentLineIndex; i++)
        {
            essaySlots[i].text = $"<color={colorCorrect}>{targetLines[i]}</color>";
        }
    }

    System.Collections.IEnumerator FlashErrorEffect()
    {
        isFlashingError = true;

        string currentLine = targetLines[currentLineIndex];

        // Ambil bagian benar
        string correctPart = currentLine.Substring(0, currentCharIndex);

        // Ambil huruf yang salah (targetnya)
        char errorChar = currentLine[currentCharIndex];

        // Sisa string
        string remainingPart = "";
        if (currentCharIndex + 1 < currentLine.Length)
            remainingPart = currentLine.Substring(currentCharIndex + 1);

        // Tampilkan efek MERAH pada huruf target
        essaySlots[currentLineIndex].text = $"<color={colorCorrect}>{correctPart}</color><color={colorError}>{errorChar}</color><color={colorDefault}>{remainingPart}</color>";

        yield return new WaitForSeconds(0.2f);

        isFlashingError = false;
        UpdateActiveLineVisuals(); // Kembalikan ke normal
    }

    void GameWin()
    {
        isGameActive = false;
        Debug.Log("Menang"); // Log sesuai permintaan
        OnGameWin?.Invoke();
    }

    void GameOver()
    {
        isGameActive = false;
        currentTimer = 0;
        if (timerTextDisplay != null) timerTextDisplay.text = "00:00";
        Debug.Log("Kalah"); // Log sesuai permintaan
        OnGameLose?.Invoke();
    }
}