using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Day1StoryController : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI dayText;
    public Image timeIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image blackScreen;
    public TextMeshProUGUI transitionText;

    [Header("Backgrounds")]
    public Image backgroundImage;
    public Sprite bgKelas;
    public Sprite bgOsis;

    [Header("Time Icons")]
    public Sprite iconPagi;
    public Sprite iconSiang;
    // public Sprite iconSore; // Bisa ditambahkan nanti
    // public Sprite iconMalam;

    [Header("Character Slots (UI Images)")]
    public Image charLeft;
    public Image charCenter;
    public Image charRight;

    [Header("Character Sprites")]
    // Masukkan sprite dari folder Assets/Arts/Characters disini via Inspector
    public Sprite ruliNormal, ruliSenyum, ruliCanggung;
    public Sprite mayaNormal, mayaSenyum, mayaKhawatir;
    public Sprite jojiNormal;
    public Sprite raniNormal;
    public Sprite pakHeriNormal;

    private bool isWaitingForInput = false;

    void Start()
    {
        // Memulai cerita saat game dimulai
        StartCoroutine(StartStoryDay1());
    }

    void Update()
    {
        // Deteksi klik spasi atau klik mouse untuk lanjut dialog
        if (isWaitingForInput && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            isWaitingForInput = false;
        }
    }

    // --- LOGIKA UTAMA CERITA ---
    IEnumerator StartStoryDay1()
    {
        // SETUP AWAL: PAGI
        dayText.text = "DAY 1";
        timeIcon.sprite = iconPagi;
        backgroundImage.sprite = bgKelas;
        transitionText.text = "";
        HideAllChars();

        // [Visual: Pagi hari, Fokus ke Ruli di Tengah]
        ShowChar(charCenter, ruliNormal);
        
        yield return Dialogue("RULI", "Namaku Ruli. Kalau dilihat dari luar, hidupku mungkin terlihat sempurna. Aku murid teladan, karismatik, dan kalau kata Rani, aku bendahara OSIS paling bisa diandalkan di angkatan ini.");
        
        yield return Dialogue("RULI", "Mereka nggak tahu aja, di balik seragam rapi dan senyum ini, aku nggak punya apa-apa. Di sekolah elit ini, citra adalah segalanya. Dan aku jago banget pura-pura.");

        // [Visual: Maya Muncul dari Kanan, Ruli Pindah ke Kiri]
        HideChar(charCenter);
        ShowChar(charLeft, ruliNormal);
        ShowChar(charRight, mayaSenyum); // Maya lari kecil tersenyum

        yield return Dialogue("MAYA", "Ruli! Pagi!");

        yield return Dialogue("RULI (Dalam Hati)", "Ah. Kecuali dia. Maya. Dia satu-satunya orang yang nggak perlu kuberikan ‘citra’.");

        // Ruli senyum tulus
        ShowChar(charLeft, ruliSenyum);
        yield return Dialogue("RULI", "Hei, May. Pagi. Kebiasaan, lari-lari.");

        // Maya senyum lebar
        // (Asumsi sprite mayaSenyum sudah dipakai, bisa ganti variasi lain jika ada)
        yield return Dialogue("MAYA", "Hehe, biar nggak telat. Kamu udah sarapan? Aku bawa bekal roti isi cokelat hari ini. Nanti pas istirahat kamu cobain, ya?");

        yield return Dialogue("RULI", "Pasti. Roti buatanmu selalu paling oke.");

        yield return Dialogue("RULI (Dalam Hati)", "Dia selalu tulus. Dia nggak pernah peduli soal Joji, soal uang, atau soal apapun. Kadang... justru itu yang bikin aku merasa... malu.");

        // Layar Hitam Sebentar (Tabrakan)
        yield return FadeOutBlackScreen(0.2f);
        // SFX: Duk (Play sound here if available)
        yield return new WaitForSeconds(0.5f);
        yield return FadeInBlackScreen(0.2f);

        yield return Dialogue("MAYA", "A-aduh!");

        // [Visual: Joji Muncul menggantikan Ruli (Ruli sementara hide atau geser)]
        // Sesuai request: Sprite Joji muncul menggantikan Sprite Ruli
        HideChar(charLeft); 
        ShowChar(charLeft, jojiNormal); // Joji di posisi Ruli tadi (Kiri)

        yield return Dialogue("JOJI", "Waduh, sori, lik. Hampir kena, ya? Haha.");

        // Sprite Maya berganti jadi Ruli (Ini instruksi unik, berarti di Kanan jadi Ruli, di Kiri Joji?)
        // Mengikuti instruksi: "sprite maya berganti menjadi sprite ruli"
        HideChar(charRight);
        ShowChar(charRight, ruliNormal); 

        yield return Dialogue("RULI (Dalam Hati)", "Joji. Si ‘anak emas’. Dia punya semua yang aku nggak punya, dan dia bahkan nggak sadar kalau dia sombong. Baginya, ini normal.");

        // Ruli pasang senyum citra
        ShowChar(charRight, ruliSenyum);
        yield return Dialogue("RULI", "Santai, Ji. Ngomong-ngomong itu sepatu baru?");

        yield return Dialogue("JOJI", "Oh, ini? Matamu jeli juga. Baru dateng kemarin keluaran terbaru limited edition. Kalo sepatunya itu-itu aja bosen juga. Eh, duluan ya. Kayaknya Rani nyariin gue tadi.");

        yield return Dialogue("RULI (Dalam Hati)", "Lihat? Dia bahkan nggak perlu berusaha untuk terlihat sempurna.");

        // Sprite Joji digantikan Maya
        HideChar(charLeft); // Joji hilang
        ShowChar(charLeft, mayaKhawatir); // Maya muncul di kiri

        yield return Dialogue("MAYA", "Rul? Kamu nggak apa-apa?");

        // Ruli tersentak
        ShowChar(charRight, ruliCanggung);
        yield return Dialogue("RULI", "Hah? Oh. Nggak apa-apa. Aman. Yuk, masuk.");

        // SFX: Bell Sekolah
        yield return Dialogue("RULI (Dalam Hati)", "Bel sudah bunyi. Waktunya pakai topeng lagi.");

        // --- TRANSISI KE SIANG ---
        yield return StartCoroutine(TransitionSequence("Ruang OSIS"));

        // SETUP SIANG: OSIS
        timeIcon.sprite = iconSiang; // Ganti icon matahari full
        backgroundImage.sprite = bgOsis; // Ganti background
        
        // [Visual: Ruli Muncul di Tengah]
        HideAllChars();
        ShowChar(charCenter, ruliNormal);

        yield return Dialogue("RULI (Dalam Hati)", "Jam istirahat. Aku memilih sembunyi di Ruang OSIS daripada melihat jajanan kantin yang nggak terbeli.");

        // Ruli geser kiri, Rani muncul (di kanan/tengah)
        HideChar(charCenter);
        ShowChar(charLeft, ruliNormal);
        ShowChar(charRight, raniNormal);

        yield return Dialogue("RANI", "Ruli Fokus, jangan sampai salah hitung.");

        yield return Dialogue("RULI (Dalam Hati)", "Dia Rani, Ketua OSIS yang sangat berbakat di bidang akademis, walaupun terkadang dia agak sulit ditebak tapi dia ketua yang baik.");

        // Pak Heri Muncul di Tengah
        ShowChar(charCenter, pakHeriNormal);

        yield return Dialogue("PAK HERI", "Nah, ini dia dua andalan saya.");

        // Rani hilang geser (Instruksi: Sprite Rani hilang bergeser, Pak Heri geser ke kanan)
        HideChar(charRight); // Rani hilang
        HideChar(charCenter);
        ShowChar(charRight, pakHeriNormal); // Pak Heri pindah ke kanan

        yield return Dialogue("RULI (Dalam Hati)", "Pak Heri. Guru Pembina OSIS. Dia guru yang tegas dan sibuk, tapi dia sangat mendukung OSIS.");

        // Sprite Ruli hilang digantikan Rani (Berarti di Kiri jadi Rani)
        HideChar(charLeft);
        ShowChar(charLeft, raniNormal);

        yield return Dialogue("RANI", "Siang, Pak.");

        yield return Dialogue("PAK HERI", "Rani, Ruli. Saya suka lihat kalian berdua ini. Kombinasi sempurna. Yang satu Ketua OSIS paling bertalenta, yang satu Bendahara paling jujur dan cerdas.");

        // Sprite Rani hilang digantikan Ruli (Kiri jadi Ruli)
        HideChar(charLeft);
        ShowChar(charLeft, ruliCanggung);

        yield return Dialogue("RULI", "Bisa aja Bapak. Hehe.");

        yield return Dialogue("PAK HERI", "Serius, Rul. Pertahankan reputasimu itu. Bapak bangga punya murid teladan kayak kamu. Ya sudah, saya duluan, ada rapat.");

        // Pak Heri hilang, digantikan Rani (Kanan jadi Rani)
        HideChar(charRight);
        ShowChar(charRight, raniNormal);

        yield return Dialogue("RANI", "Dengerin tuh. Jangan kecewain Pak Heri. Oke, Besok di jam yang sama ya, jangan telat Rul. Gue duluan.");

        // Rani Keluar (Hide Kanan)
        HideChar(charRight);

        // Ruli ke Tengah
        HideChar(charLeft);
        ShowChar(charCenter, ruliNormal);

        yield return Dialogue("RULI (Dalam Hati)", "Murid teladan. Bendahara jujur. Kepercayaan.");
        yield return Dialogue("RULI (Dalam Hati)", "Mereka semua menaruh beban itu di pundakku. Mereka nggak tahu betapa beratnya pura-pura jadi orang yang mereka inginkan.");

        // SFX: Bel Masuk
        yield return FadeOutBlackScreen(1.0f);
        
        Debug.Log("Scene End");
    }

    // --- FUNGSI BANTUAN (HELPER) ---

    IEnumerator Dialogue(string name, string text)
    {
        nameText.text = name;
        dialogueText.text = ""; // Reset text
        
        // Efek mengetik (Typewriter)
        foreach (char c in text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f); // Kecepatan ketik
        }

        isWaitingForInput = true;
        yield return new WaitUntil(() => isWaitingForInput == false);
    }

    IEnumerator TransitionSequence(string title)
    {
        // Layar gelap
        transitionText.text = title;
        yield return FadeOutBlackScreen(1f); // Munculkan layar hitam
        yield return new WaitForSeconds(3f); // Tahan 3 detik
        transitionText.text = "";
        yield return FadeInBlackScreen(1f); // Hilangkan layar hitam
    }

    IEnumerator FadeOutBlackScreen(float duration)
    {
        float t = 0;
        Color c = blackScreen.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / duration);
            blackScreen.color = c;
            yield return null;
        }
        c.a = 1;
        blackScreen.color = c;
    }

    IEnumerator FadeInBlackScreen(float duration)
    {
        float t = 0;
        Color c = blackScreen.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / duration);
            blackScreen.color = c;
            yield return null;
        }
        c.a = 0;
        blackScreen.color = c;
    }

    void ShowChar(Image slot, Sprite sprite)
    {
        slot.sprite = sprite;
        slot.color = Color.white; // Pastikan terlihat
        slot.preserveAspect = true;
        slot.gameObject.SetActive(true);
    }

    void HideChar(Image slot)
    {
        slot.gameObject.SetActive(false);
    }
    
    void HideAllChars()
    {
        charLeft.gameObject.SetActive(false);
        charCenter.gameObject.SetActive(false);
        charRight.gameObject.SetActive(false);
    }
}