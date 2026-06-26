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
    
    [Header("Ses Ayarları")]
    public Image toggleImage; 
    public Sprite onSprite;   
    public Sprite offSprite;  
    private bool isSoundOn = true;

    [Header("Koleksiyon Ayarları")]
    public GameObject dinoCardPrefab;
    public Transform collectionContent;
    public Sprite[] allDinoSprites;
    public string[] allDinoNames;

    void Start()
    {
        int savedSound = PlayerPrefs.GetInt("SoundState", 1);
        isSoundOn = (savedSound == 1);
        AudioListener.pause = !isSoundOn;
        if(toggleImage != null) toggleImage.sprite = isSoundOn ? onSprite : offSprite;
    }

    public void PlayGame() { SceneManager.LoadScene(1); }

    public void ToggleSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        AudioListener.pause = !isSoundOn;
        if(toggleImage != null) toggleImage.sprite = isSoundOn ? onSprite : offSprite;
        PlayerPrefs.SetInt("SoundState", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OpenCollection()
    {
        collectionPanel.SetActive(true);
        LoadCollectionData(); // Burayı ekledik, artık tetikleniyor!
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
