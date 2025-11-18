using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Penting: Kita pakai namespace TextMeshPro

public class RiggedSlotMachine : MonoBehaviour
{
    [Header("Assets & UI Reference")]
    public Sprite[] slotIcons; // Urutan gambar (0: Banana, 1: 7, dst...)
    public Image[] reels;      // 3 Image UI
    public Button spinButton;
    public TMP_Text resultText; // Referensi ke TextMeshPro untuk Win/Lose

    [Header("Settings")]
    public float spinDuration = 2.0f; 
    public float spinSpeed = 0.05f;
    
    // Menyimpan harga hadiah per Index Gambar
    // Misal element 0 (Banana) harganya 5000
    public int[] prizeValues; 

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

        // Reset text saat awal
        if(resultText != null) resultText.text = "";
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
        
        // Ubah teks jadi "Spinning..." atau kosongkan saat muter
        if(resultText != null) resultText.text = "";

        float timer = 0;

        // --- FASE 1: ANIMASI RANDOM ---
        while (timer < spinDuration)
        {
            foreach (var reel in reels)
            {
                reel.sprite = slotIcons[Random.Range(0, slotIcons.Length)];
            }
            yield return new WaitForSeconds(spinSpeed);
            timer += spinSpeed;
        }

        // --- FASE 2: MENENTUKAN HASIL ---
        int resultIndex1, resultIndex2, resultIndex3;

        if (currentSpinCount < preDeterminedResults.Count)
        {
            // Pakai hasil settingan
            resultIndex1 = preDeterminedResults[currentSpinCount].reel1Index;
            resultIndex2 = preDeterminedResults[currentSpinCount].reel2Index;
            resultIndex3 = preDeterminedResults[currentSpinCount].reel3Index;
        }
        else
        {
            // Random murni kalau settingan habis
            resultIndex1 = Random.Range(0, slotIcons.Length);
            resultIndex2 = Random.Range(0, slotIcons.Length);
            resultIndex3 = Random.Range(0, slotIcons.Length);
        }

        // Tampilkan gambar akhir
        reels[0].sprite = slotIcons[resultIndex1];
        reels[1].sprite = slotIcons[resultIndex2];
        reels[2].sprite = slotIcons[resultIndex3];

        // --- FASE 3: CEK MENANG ATAU KALAH ---
        CheckWinCondition(resultIndex1, resultIndex2, resultIndex3);

        currentSpinCount++;
        isSpinning = false;
        spinButton.interactable = true;
    }

    void CheckWinCondition(int r1, int r2, int r3)
    {
        if (resultText == null) return;

        // Logika Menang: Ketiga reel harus sama index-nya
        if (r1 == r2 && r2 == r3)
        {
            // Ambil nominal hadiah dari array prizeValues berdasarkan index gambar yang muncul
            int winAmount = 0;
            
            // Cek apakah indexnya valid di dalam array prizeValues
            if (r1 < prizeValues.Length)
            {
                winAmount = prizeValues[r1];
            }

            resultText.text = $"YOU WIN!\nRp. {winAmount}";
            resultText.color = Color.yellow; // Ubah warna jadi kuning/emas
        }
        else
        {
            // Jika tidak sama semua
            resultText.text = "YOU LOSE\nTry Again";
            resultText.color = Color.red; // Ubah warna jadi merah
        }
    }
}