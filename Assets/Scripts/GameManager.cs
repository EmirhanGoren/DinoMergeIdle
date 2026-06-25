using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using System.Collections.Generic; 
using System.IO; 
using UnityEngine.UI; // YENİ: UI Image için gerekli

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dinozor Şablonları Listesi")]
    public GameObject[] dinoPrefabs;

    [Header("Yumurta Ayarları")]
    public GameObject eggPrefab; 

    [Header("Ekonomi Ayarları")]
    public int currentGold = 0;
    public int currentAreaEggPrice = 50; 
    
    [Header("UI Ayarları")]
    public TextMeshProUGUI goldText; 

    [Header("Keşif (Discovery) Ekranı")]
    public GameObject discoveryPanel; // Tüm ekranı kaplayan panel
    public Image discoveryImage; // İçindeki dinozor resmi
    public Sprite[] discoverySprites; // Çizdirdiğin resimler (Sırayla 1., 2., 3. seviye...)
    public List<int> unlockedDinoLevels = new List<int>(); // Oyuncunun daha önce açtığı dinozorlar

    private string saveFilePath; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        saveFilePath = Application.persistentDataPath + "/dino_save.json";
    }

    void Start()
    {
        LoadGame();
    }

    // --- KEŞİF EKRANI (DISCOVERY) SİSTEMİ ---

    public void CheckNewDinoDiscovery(int dinoLevel)
    {
        // Eğer bu seviyeyi daha önce LİSTEDE YOKSA (Yani ilk defa görüyorsak)
        if (!unlockedDinoLevels.Contains(dinoLevel))
        {
            unlockedDinoLevels.Add(dinoLevel); // Listeye ekle
            SaveGame(); // Hemen kaydet ki bir daha oyunu aç kapa yapsa bile çıkmasın

            // Resmi ayarla (Seviye 1 ise index 0 olur, Seviye 2 ise index 1 olur)
            int index = dinoLevel - 1;
            if (index >= 0 && index < discoverySprites.Length && discoverySprites[index] != null)
            {
                discoveryImage.sprite = discoverySprites[index]; // Resmi değiştir
                discoveryPanel.SetActive(true); // Paneli görünür yap
                Time.timeScale = 0f; // Arkada oyunu (dinozor hareketlerini) dondur
            }
        }
    }

    // Ekrana tıklandığında (veya Tap the Screen yazısına basıldığında) çalışacak
    public void CloseDiscoveryScreen()
    {
        discoveryPanel.SetActive(false); // Paneli gizle
        Time.timeScale = 1f; // Oyunu normal hızına döndür
    }

    // --- JSON KAYDETME VE YÜKLEME ---

    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.savedGold = currentGold;
        data.unlockedDinoLevels = new List<int>(unlockedDinoLevels); // Keşfedilenleri kopyala

        DinoController[] allDinos = FindObjectsByType<DinoController>(FindObjectsSortMode.None);
        foreach (DinoController dino in allDinos)
        {
            data.savedDinoLevels.Add(dino.dinoLevel);
        }

        GameObject[] activeEggs = GameObject.FindGameObjectsWithTag("Egg");
        data.savedEggCount = activeEggs.Length;

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            currentGold = data.savedGold;
            UpdateGoldUI();

            // Keşfedilenleri geri yükle
            if (data.unlockedDinoLevels != null)
            {
                unlockedDinoLevels = data.unlockedDinoLevels;
            }

            foreach (int level in data.savedDinoLevels)
            {
                Vector3 randomPos = new Vector3(Random.Range(-2f, 2f), Random.Range(-3f, 3f), 0f);
                SpawnNextLevelDino(level, randomPos); 
            }

            for (int i = 0; i < data.savedEggCount; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-2f, 2f), Random.Range(-3f, 3f), 0f);
                Instantiate(eggPrefab, randomPos, Quaternion.identity);
            }
        }
        else
        {
            UpdateGoldUI();
            Instantiate(eggPrefab, Vector3.zero, Quaternion.identity); 
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Kayıt Dosyasını Sıfırla (JSON Sil)")]
    public void ResetSaveData()
    {
        // Dosya yolunu burada tekrar belirtiyoruz ki oyun kapalıyken de bulabilsin
        string path = Application.persistentDataPath + "/dino_save.json";
        
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("JSON Dosyası SİLİNDİ!");
        }
        else
        {
            Debug.Log("Silinecek bir kayıt dosyası zaten yok.");
        }
        
        PlayerPrefs.DeleteAll(); 

        // EN ÖNEMLİ KISIM: Eğer oyun çalışıyorken bu butona basarsan, 
        // hafızadaki her şeyi sıfırla ki kapanırken eski verileri tekrar kaydetmesin!
        currentGold = 0;
        if (unlockedDinoLevels != null) unlockedDinoLevels.Clear();
        
        // Ekranda açık olan sahnede objeler varsa (Play modundayken) onları yok et
        if (Application.isPlaying)
        {
            DinoController[] dinos = FindObjectsByType<DinoController>(FindObjectsSortMode.None);
            foreach (var d in dinos) Destroy(d.gameObject);
            
            GameObject[] eggs = GameObject.FindGameObjectsWithTag("Egg");
            foreach (var e in eggs) Destroy(e);
            
            UpdateGoldUI();
        }
    }
#endif

    private void OnApplicationQuit() { SaveGame(); }
    private void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveGame(); }

    public void AddGold(int amount) { currentGold += amount; UpdateGoldUI(); SaveGame(); }
    private void UpdateGoldUI() { if (goldText != null) goldText.text = currentGold.ToString(); }

    public void BuyDino()
    {
        if (currentGold >= currentAreaEggPrice)
        {
            currentGold -= currentAreaEggPrice; UpdateGoldUI(); SaveGame(); 
            Vector3 randomPos = new Vector3(Random.Range(-2f, 2f), Random.Range(-3f, 3f), 0f);
            Instantiate(eggPrefab, randomPos, Quaternion.identity);
        }
    }

    public void SpawnNextLevelDino(int nextLevel, Vector3 spawnPosition)
    {
        int targetIndex = nextLevel - 1;
        if (targetIndex < dinoPrefabs.Length && dinoPrefabs[targetIndex] != null)
        {
            GameObject yeniDino = Instantiate(dinoPrefabs[targetIndex], spawnPosition, Quaternion.identity);
            DinoController controllerScript = yeniDino.GetComponent<DinoController>();
            if (controllerScript != null) controllerScript.dinoLevel = nextLevel;
        }
    }

    public void LoadMainMenu() { SaveGame(); SceneManager.LoadScene(0); }
}

[System.Serializable]
public class SaveData
{
    public int savedGold;
    public int savedEggCount;
    public List<int> savedDinoLevels = new List<int>();
    public List<int> unlockedDinoLevels = new List<int>(); 
}