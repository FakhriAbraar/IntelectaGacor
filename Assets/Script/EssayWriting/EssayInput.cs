using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EssayInput : MonoBehaviour
{
    [Header("Hubungkan ke Otak Game")]
    public EssayGameManager gameManager; // Referensi ke script manager

    void Update()
    {
        // CARA 1: Input String (Paling Stabil untuk Typing Game)
        if (Input.anyKeyDown && !string.IsNullOrEmpty(Input.inputString))
        {
            foreach (char letter in Input.inputString)
            {
                gameManager.ReceiveInput(letter); // Kirim huruf ke Manager
            }
        }

        // CARA 2: Backup untuk New Input System (Jika Cara 1 tidak jalan di setting project tertentu)
#if ENABLE_INPUT_SYSTEM
        else if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.wasPressedThisFrame && !string.IsNullOrEmpty(key.displayName))
                {
                    string charStr = key.displayName;
                    if (charStr.Length == 1) gameManager.ReceiveInput(charStr[0]);
                    else if (charStr == "Space") gameManager.ReceiveInput(' ');
                }
            }
        }
#endif
    }
}