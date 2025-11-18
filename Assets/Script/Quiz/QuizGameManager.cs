using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuizGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public TextAsset jsonFile;
    public float globalTime = 45f;
    public int maxQuestions = 10;

    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI timerText;
    public GameObject[] crossIcons;

    [Header("Buttons")]
    public AnswerButton[] answerButtons;

    // Data Soal
    private List<Question> allQuestions = new List<Question>();
    private int currentQuestionIndex = 0;

    // Status Game
    private int currentLives = 3;
    private float timer;
    private bool isGameActive = true;

    // --- LIST BARU UNTUK MENYIMPAN URUTAN ACAK ---
    // List ini akan menyimpan pemetaan tombol ke jawaban asli
    // Contoh: Tombol 0 (Kiri Atas) ternyata menyimpan jawaban index 2 (C)
    private List<int> currentShuffledIndices = new List<int>();

    void Start()
    {
        // 1. Load Soal
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

        // 2. Reset Status
        timer = globalTime;
        currentLives = 3;
        currentQuestionIndex = 0;

        foreach (var cross in crossIcons) if (cross != null) cross.SetActive(false);

        // 3. Setup Tombol
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].Setup(this, i);

                Button btn = answerButtons[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(answerButtons[i].OnClick);
                }
            }
        }

        ShowQuestion();
    }

    void Update()
    {
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

    void ShowQuestion()
    {
        if (currentQuestionIndex < allQuestions.Count && currentQuestionIndex < maxQuestions)
        {
            Question q = allQuestions[currentQuestionIndex];

            if (questionText != null) questionText.text = q.questionText;


            currentShuffledIndices.Clear();
            for (int k = 0; k < q.answers.Length; k++)
            {
                currentShuffledIndices.Add(k);
            }

            for (int i = 0; i < currentShuffledIndices.Count; i++)
            {
                int temp = currentShuffledIndices[i];
                int randomIndex = Random.Range(i, currentShuffledIndices.Count);
                currentShuffledIndices[i] = currentShuffledIndices[randomIndex];
                currentShuffledIndices[randomIndex] = temp;
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < q.answers.Length)
                {
                    answerButtons[i].gameObject.SetActive(true);


                    int originalIndex = currentShuffledIndices[i];

                    answerButtons[i].SetAnswerText(q.answers[originalIndex]);
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            GameOver("Selamat! Semua soal selesai.");
        }
    }

    public void AnswerClicked(int buttonIndex)
    {
        if (!isGameActive) return;

        Question currentQ = allQuestions[currentQuestionIndex];

        int originalAnswerIndex = currentShuffledIndices[buttonIndex];

        if (originalAnswerIndex == currentQ.correctAnswerIndex)
        {
            Debug.Log("Jawaban Benar!");
            currentQuestionIndex++;
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

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer - minutes * 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

            if (timer <= 10f) timerText.color = Color.red;
        }
    }

    void UpdateLivesUI()
    {
        int crossIndexToShow = 2 - currentLives;
        if (crossIndexToShow >= 0 && crossIndexToShow < crossIcons.Length)
        {
            if (crossIcons[crossIndexToShow] != null)
                crossIcons[crossIndexToShow].SetActive(true);
        }
    }

    void GameOver(string message)
    {
        isGameActive = false;
        if (questionText != null) questionText.text = message;

        foreach (var btn in answerButtons)
        {
            btn.GetComponent<Button>().interactable = false;
        }
    }
}