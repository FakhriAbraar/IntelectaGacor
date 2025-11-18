using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour
{
    public TextMeshProUGUI answerText;
    private GameManager gameManager;
    private int answerIndex;

    public void Setup(GameManager manager, int index)
    {
        gameManager = manager;
        answerIndex = index;
    }

    public void OnClick()
    {
        if (gameManager != null)
        {
            gameManager.AnswerClicked(answerIndex);
        }
    }
}