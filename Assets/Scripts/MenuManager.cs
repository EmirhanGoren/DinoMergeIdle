using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panel Ayarları")]
    public GameObject settingsPanel;
    public GameObject collectionPanel;
    
    [Header("Ses Görsel Ayarları")]
    public Image sfxToggleImage;   // SFX (Efekt) butonunun üzerindeki ikon resmi
    public Image musicToggleImage; // Müzik butonunun üzerindeki ikon resmi
    public Sprite onSprite;   
    public Sprite offSprite;  

    [Header("Koleksiyon Ayarları")]
    public GameObject dinoCardPrefab;
    public Transform collectionContent;
    public Sprite[] allDinoSprites;
    public string[] allDinoNames;

    private bool isSFXOn = true;
    private bool isMusicOn = true;

    void Start()
    {
        // Hafızadan ses durumlarını yükle (Kayıt yoksa varsayılan olarak 1 yani Açık gelir)
        isSFXOn = PlayerPrefs.GetInt("SFXState", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("MusicState", 1) == 1;

        // Butonların üzerindeki On/Off ikonlarını güncelle
        UpdateUIStyles();

        // Ana menü sahnesindeki BackgroundMusic objesini bul ve durumuna göre sustur/aç
        ApplyMusicState();
    }

    public void PlayGame() { SceneManager.LoadScene(1); }

    public void ToggleSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    // Sadece ses efektlerini (SFX) açıp kapatan buton fonksiyonu
    public void ToggleSFX()
    {
        isSFXOn = !isSFXOn;
        PlayerPrefs.SetInt("SFXState", isSFXOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateUIStyles();
    }

    // Sadece müziği açıp kapatan buton fonksiyonu
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MusicState", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateUIStyles();
        ApplyMusicState(); // Değişikliği anında bu sahnedeki müziğe uygula
    }

    // Tüm sesleri tek tıkla kapatan/açan master buton fonksiyonu
    public void ToggleAllSounds()
    {
        if (isSFXOn || isMusicOn)
        {
            isSFXOn = false;
            isMusicOn = false;
        }
        else
        {
            isSFXOn = true;
            isMusicOn = true;
        }

        PlayerPrefs.SetInt("SFXState", isSFXOn ? 1 : 0);
        PlayerPrefs.SetInt("MusicState", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateUIStyles();
        ApplyMusicState();
    }

    private void UpdateUIStyles()
    {
        if (sfxToggleImage != null) sfxToggleImage.sprite = isSFXOn ? onSprite : offSprite;
        if (musicToggleImage != null) musicToggleImage.sprite = isMusicOn ? onSprite : offSprite;
    }

    private void ApplyMusicState()
    {
        GameObject bgMusicObj = GameObject.Find("BackgroundMusic");
        if (bgMusicObj != null)
        {
            AudioSource audio = bgMusicObj.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.mute = !isMusicOn;
            }
        }
    }

    public void OpenCollection()
    {
        collectionPanel.SetActive(true);
        LoadCollectionData();
    }

    public void CloseCollection()
    {
        collectionPanel.SetActive(false);
    }

    private void LoadCollectionData()
    {
        foreach (Transform child in collectionContent) Destroy(child.gameObject);

        List<int> unlockedLevels = new List<int>();
        string saveFilePath = Application.persistentDataPath + "/dino_save.json";
        
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data.unlockedDinoLevels != null) unlockedLevels = data.unlockedDinoLevels;
        }

        for (int i = 0; i < allDinoSprites.Length; i++)
        {
            int currentLevel = i + 1;
            GameObject newCard = Instantiate(dinoCardPrefab, collectionContent);
            
            Image cardImage = newCard.transform.Find("DinoImage").GetComponent<Image>();
            TextMeshProUGUI cardText = newCard.transform.Find("DinoNameText").GetComponent<TextMeshProUGUI>();

            cardImage.sprite = allDinoSprites[i];

            if (unlockedLevels.Contains(currentLevel))
            {
                cardImage.color = Color.white;
                cardText.text = allDinoNames[i];
            }
            else
            {
                cardImage.color = Color.black;
                cardText.text = "???";
            }
        }
    }
}