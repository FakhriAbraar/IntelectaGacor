using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
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

    // Private Variables
    private List<Question> allQuestions = new List<Question>();
    private List<Question> currentQuestions = new List<Question>();
    private int currentQuestionIndex = 0;
    private int currentLives = 3;
    private float timer;
    private bool isGameActive = true;

    void Start()
    {
        LoadQuestionsFromJson();

        timer = globalTime;
        currentLives = 3;

        foreach (var cross in crossIcons)
        {
            if (cross != null) cross.SetActive(false);
        }

        SelectQuestions();

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

        if (currentQuestions.Count > 0)
        {
            ShowQuestion();
        }
        else
        {
            Debug.LogError("Gagal memulai: Soal tidak ditemukan atau JSON belum dipasang!");
            isGameActive = false;
        }
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
                GameOver();
            }
        }
    }

    void LoadQuestionsFromJson()
    {
        if (jsonFile != null)
        {
            QuestionList data = JsonUtility.FromJson<QuestionList>(jsonFile.text);
            if (data != null && data.questions != null)
            {
                allQuestions = new List<Question>(data.questions);
            }
        }
        else
        {
            Debug.LogError("File JSON belum dimasukkan ke Inspector GameManager!");
        }
    }

    void SelectQuestions()
    {
        currentQuestions.Clear();
        if (allQuestions.Count > 0)
        {
            List<Question> temp = new List<Question>(allQuestions);
            int loopCount = Mathf.Min(maxQuestions, temp.Count);

            for (int i = 0; i < loopCount; i++)
            {
                int rand = Random.Range(0, temp.Count);
                currentQuestions.Add(temp[rand]);
                temp.RemoveAt(rand);
            }
        }
    }

    void ShowQuestion()
    {
        if (currentQuestionIndex < currentQuestions.Count)
        {
            Question q = currentQuestions[currentQuestionIndex];

            if (questionText != null)
                questionText.text = q.questionText;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < q.answers.Length)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerButtons[i].answerText.text = q.answers[i];
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.Log("Menang! Semua soal selesai.");
            isGameActive = false;
        }
    }

    public void AnswerClicked(int selectedIndex)
    {
        if (!isGameActive) return;

        Question currentQ = currentQuestions[currentQuestionIndex];

        if (selectedIndex == currentQ.correctAnswerIndex)
        {
            Debug.Log("Benar!");
            currentQuestionIndex++;
            ShowQuestion();
        }
        else
        {
            Debug.Log("Salah!");
            currentLives--;
            UpdateLivesUI();

            if (currentLives <= 0)
            {
                GameOver();
            }
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

    void GameOver()
    {
        isGameActive = false;
        Debug.Log("Game Over!");
    }
}