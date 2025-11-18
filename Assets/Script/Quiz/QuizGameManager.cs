using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuizGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public TextAsset jsonFile;      // Masukkan file Quiz.json ke sini
    public float globalTime = 45f;  // Waktu total permainan
    public int maxQuestions = 10;   // Batas jumlah soal

    [Header("UI References")]
    public TextMeshProUGUI questionText; // Masukkan Text Pertanyaan (Question)
    public TextMeshProUGUI timerText;    // Masukkan Text Timer
    public GameObject[] crossIcons;      // Masukkan 3 Gambar Silang (Lives)

    [Header("Buttons")]
    public AnswerButton[] answerButtons; // Masukkan 4 Object Tombol (A, B, C, D)

    // Variabel Internal
    private List<Question> allQuestions = new List<Question>();
    private int currentQuestionIndex = 0;
    private int currentLives = 3;
    private float timer;
    private bool isGameActive = true;

    void Start()
    {
        // 1. Load Soal dari File JSON
        if (jsonFile != null)
        {
            QuestionList data = JsonUtility.FromJson<QuestionList>(jsonFile.text);
            allQuestions = new List<Question>(data.questions);
        }
        else
        {
            Debug.LogError("File JSON belum dimasukkan ke Inspector GameManager!");
            isGameActive = false;
            return;
        }

        // 2. Reset Status Game
        timer = globalTime;
        currentLives = 3;
        currentQuestionIndex = 0;

        // Sembunyikan semua tanda silang di awal
        foreach (var cross in crossIcons)
        {
            if (cross != null) cross.SetActive(false);
        }

        // 3. Setup Tombol Jawaban
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].Setup(this, i);

                // Tambahkan fungsi klik secara otomatis via kode
                Button btn = answerButtons[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(answerButtons[i].OnClick);
                }
            }
        }

        // 4. Mulai Tampilkan Soal Pertama
        ShowQuestion();
    }

    void Update()
    {
        // Logika Timer Berjalan Mundur
        if (isGameActive && timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();

            if (timer <= 0)
            {
                timer = 0;
                UpdateTimerUI();
                GameOver("Waktu Habis!");
            }
        }
    }

    // Fungsi untuk menampilkan soal ke layar
    void ShowQuestion()
    {
        // Cek apakah index masih dalam batas jumlah soal yang tersedia
        if (currentQuestionIndex < allQuestions.Count && currentQuestionIndex < maxQuestions)
        {
            Question q = allQuestions[currentQuestionIndex];

            // Tampilkan teks pertanyaan
            if (questionText != null) questionText.text = q.questionText;

            // Tampilkan pilihan jawaban di tombol
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < q.answers.Length)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerButtons[i].SetAnswerText(q.answers[i]);
                }
                else
                {
                    // Matikan tombol jika jawaban kurang dari 4
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // Menang jika soal habis
            GameOver("Selamat! Semua soal selesai.");
        }
    }

    // Fungsi yang dipanggil saat tombol jawaban diklik
    public void AnswerClicked(int selectedIndex)
    {
        if (!isGameActive) return;

        Question currentQ = allQuestions[currentQuestionIndex];

        // Cek Jawaban
        if (selectedIndex == currentQ.correctAnswerIndex)
        {
            Debug.Log("Jawaban Benar!");
            currentQuestionIndex++; // Lanjut ke soal berikutnya
            ShowQuestion();
        }
        else
        {
            Debug.Log("Jawaban Salah!");
            currentLives--;
            UpdateLivesUI();

            if (currentLives <= 0)
            {
                GameOver("Game Over! Nyawa Habis.");
            }
        }
    }

    // Update tampilan timer (Format 0:45)
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer - minutes * 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

            // Ubah warna jadi merah jika waktu tinggal 10 detik
            if (timer <= 10f) timerText.color = Color.red;
        }
    }

    // Update tampilan nyawa (Tanda Silang)
    void UpdateLivesUI()
    {
        // Logic: 3 nyawa = 0 silang, 2 nyawa = 1 silang, dst.
        int crossIndexToShow = 2 - currentLives;

        if (crossIndexToShow >= 0 && crossIndexToShow < crossIcons.Length)
        {
            if (crossIcons[crossIndexToShow] != null)
                crossIcons[crossIndexToShow].SetActive(true);
        }
    }

    // Fungsi Game Over
    void GameOver(string message)
    {
        isGameActive = false;
        if (questionText != null) questionText.text = message;
        Debug.Log(message);

        // Nonaktifkan interaksi tombol
        foreach (var btn in answerButtons)
        {
            btn.GetComponent<Button>().interactable = false;
        }
    }
}