using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI answerText;

    private QuizGameManager gameManager;
    private int answerIndex;

    public void Setup(QuizGameManager manager, int index)
    {
        gameManager = manager;
        answerIndex = index;
    }
    public void SetAnswerText(string text)
    {
        if (answerText != null)
        {
            answerText.text = text;
        }
        else
        {
            Debug.LogError("Answer Text belum di-assign di Inspector tombol: " + gameObject.name);
        }
    }

    // Fungsi yang akan dipanggil saat tombol diklik
    public void OnClick()
    {
        if (gameManager != null)
        {
            gameManager.AnswerClicked(answerIndex);
        }
    }
}