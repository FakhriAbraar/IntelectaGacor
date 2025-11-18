using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class EssayTypingManager : MonoBehaviour
{
    [Header("Komponen UI")]
    [Tooltip("Masukkan komponen TextMeshProUGUI di sini")]
    public TMP_Text essayTextDisplay;

    [Tooltip("Text untuk menampilkan sisa waktu")]
    public TMP_Text timerTextDisplay;

    [Header("Pengaturan Soal")]
    [Tooltip("Masukkan kalimat per baris di sini. Klik tombol '+' untuk menambah baris.")]
    public string[] essayLines; // UBAHAN: Menggunakan Array agar bisa input per baris

    [Header("Pengaturan Game")]
    public float gameDuration = 75.0f;
    public bool autoSkipSpaces = true; // Set true agar spasi dan enter terlewati otomatis

    [Header("Warna")]
    public string colorCorrect = "#000000"; // Hitam
    public string colorDefault = "#808080"; // Abu-abu (Transparan/Pudar)
    public string colorError = "#FF0000";   // Merah

    // State Variables
    private string contentToType; // Variable internal hasil gabungan baris
    private int currentIndex = 0;
    private float currentTimer;
    private bool isGameActive = false;
    private bool isFlashingError = false;

    // Events
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (!isGameActive) return;

        HandleTimer();
        HandleInput();
    }

    void StartGame()
    {
        // Menggabungkan semua baris di array menjadi satu string panjang dengan pemisah baris baru (\n)
        // Ini memastikan format baris sesuai keinginan Anda di Inspector
        contentToType = string.Join("\n", essayLines);

        essayTextDisplay.text = contentToType;
        currentTimer = gameDuration;
        currentIndex = 0;
        isGameActive = true;

        // Cek jika karakter pertama adalah spasi/newline
        if (autoSkipSpaces) CheckForAutoSkip();

        UpdateTextVisuals();
    }

    void HandleTimer()
    {
        currentTimer -= Time.deltaTime;

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
                if (c == '\b' || c == '\n' || c == '\r') continue;
                ValidateCharacter(c);
            }
        }
    }

    void ValidateCharacter(char typedChar)
    {
        if (currentIndex >= contentToType.Length) return;

        char targetChar = contentToType[currentIndex];

        // Logika Benar
        if (typedChar == targetChar)
        {
            currentIndex++;

            // Jika AutoSkip nyala, dia akan melewati spasi DAN baris baru (\n)
            if (autoSkipSpaces) CheckForAutoSkip();

            UpdateTextVisuals();

            if (currentIndex >= contentToType.Length)
            {
                GameWin();
            }
        }
        // Logika Salah
        else
        {
            if (!isFlashingError)
            {
                StartCoroutine(FlashErrorEffect());
            }
        }
    }

    // Fungsi ini sekarang akan melewati spasi (' ') dan Enter ('\n')
    // Jadi ketika player selesai mengetik baris 1, kursor otomatis pindah ke huruf pertama baris 2
    void CheckForAutoSkip()
    {
        while (currentIndex < contentToType.Length)
        {
            char nextChar = contentToType[currentIndex];
            // char.IsWhiteSpace mendeteksi spasi, tab, dan enter/newline
            if (char.IsWhiteSpace(nextChar))
            {
                currentIndex++;
            }
            else
            {
                break;
            }
        }
    }

    void UpdateTextVisuals()
    {
        if (isFlashingError) return;

        string correctPart = contentToType.Substring(0, currentIndex);
        string remainingPart = contentToType.Substring(currentIndex);

        // Tampilkan teks dengan warna yang sesuai
        essayTextDisplay.text = $"<color={colorCorrect}>{correctPart}</color><color={colorDefault}>{remainingPart}</color>";
    }

    IEnumerator FlashErrorEffect()
    {
        isFlashingError = true;

        string correctPart = contentToType.Substring(0, currentIndex);
        char errorChar = contentToType[currentIndex];

        string remainingPart = "";
        if (currentIndex + 1 < contentToType.Length)
            remainingPart = contentToType.Substring(currentIndex + 1);

        essayTextDisplay.text = $"<color={colorCorrect}>{correctPart}</color><color={colorError}>{errorChar}</color><color={colorDefault}>{remainingPart}</color>";

        yield return new WaitForSeconds(0.2f);

        isFlashingError = false;
        UpdateTextVisuals();
    }

    void GameWin()
    {
        isGameActive = false;
        Debug.Log("Menang!");
        OnGameWin?.Invoke();
    }

    void GameOver()
    {
        isGameActive = false;
        currentTimer = 0;
        if (timerTextDisplay != null) timerTextDisplay.text = "00:00";
        Debug.Log("Waktu Habis!");
        OnGameLose?.Invoke();
    }
}