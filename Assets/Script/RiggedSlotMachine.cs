using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RiggedSlotMachine : MonoBehaviour
{
    [Header("Assets & UI Reference")]
    public Sprite[] slotIcons;
    public Image[] reels;      
    public Button spinButton;

    [Header("Settings")]
    public float spinDuration = 2.0f; 
    public float spinSpeed = 0.05f;   


    [System.Serializable]
    public struct RiggedResult
    {
        public int reel1Index; 
        public int reel2Index; 
        public int reel3Index; 
    }

    [Header("The 'Gimmick' Config")]
    public List<RiggedResult> preDeterminedResults; 
    private int currentSpinCount = 0; 

    private bool isSpinning = false;

    void Start()
    {

        if (spinButton != null)
        {
            spinButton.onClick.AddListener(StartSpin);
        }
    }

    void StartSpin()
    {
        if (isSpinning) return; 
        StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        isSpinning = true;
        spinButton.interactable = false;

        float timer = 0;


        while (timer < spinDuration)
        {
            foreach (var reel in reels)
            {
                reel.sprite = slotIcons[Random.Range(0, slotIcons.Length)];
            }

            yield return new WaitForSeconds(spinSpeed);
            timer += spinSpeed;
        }

        int resultIndex1, resultIndex2, resultIndex3;

        if (currentSpinCount < preDeterminedResults.Count)
        {

            resultIndex1 = preDeterminedResults[currentSpinCount].reel1Index;
            resultIndex2 = preDeterminedResults[currentSpinCount].reel2Index;
            resultIndex3 = preDeterminedResults[currentSpinCount].reel3Index;
        }
        else
        {
    
            resultIndex1 = Random.Range(0, slotIcons.Length);
            resultIndex2 = Random.Range(0, slotIcons.Length);
            resultIndex3 = Random.Range(0, slotIcons.Length);
        }


        reels[0].sprite = slotIcons[resultIndex1];
        reels[1].sprite = slotIcons[resultIndex2];
        reels[2].sprite = slotIcons[resultIndex3];

        currentSpinCount++;
        
        isSpinning = false;
        spinButton.interactable = true;
    }
}