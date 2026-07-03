using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dinosaur Prefabs")]
    public GameObject[] dinoPrefabs;

    [Header("Egg Settings")]
    public GameObject eggPrefab; 

    [Header("Habitat Settings")]
    public int activeArea = 0; 
    public bool isCarnivoreUnlocked = false; 

    [Header("Area Switch & Background")]
    public GameObject switchAreaButton; 
    public SpriteRenderer backgroundRenderer; 
    public Sprite herbivoreBgSprite; 
    public Sprite carnivoreBgSprite; 

    [Header("Economy Settings")]
    public int currentGold = 0;
    public int herbivoreEggPrice = 50;  
    public int carnivoreEggPrice = 500; 
    
    [Header("UI Settings")]
    public TextMeshProUGUI goldText; 
    public TextMeshProUGUI buyEggText; 

    [Header("Discovery Screen")]
    public GameObject discoveryPanel; 
    public Image discoveryImage; 
    public Sprite[] discoverySprites; 
    public List<int> unlockedDinoLevels = new List<int>(); 

    [Header("New Era Screen")]
    public GameObject newEraPanel; 

    [Header("Audio Settings")]
    public AudioClip discoverySound; 
    public AudioClip newEraSound;    

    private string saveFilePath; 
    private int inactiveIncomePerTick = 0; 
    
    private bool pendingSpecialEgg = false;
    private Vector3 pendingEggPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        saveFilePath = Application.persistentDataPath + "/dino_save.json";
    }

    void Start()
    {
        LoadGame();
        StartCoroutine(InactiveAreaGoldRoutine());

        // YENİ: Sahne açıldığında bu sahnedeki BackgroundMusic objesini kontrol et
        ApplyMusicState();
    }

    public void ApplyMusicState()
    {
        GameObject bgMusicObj = GameObject.Find("BackgroundMusic");
        if (bgMusicObj != null)
        {
            AudioSource audio = bgMusicObj.GetComponent<AudioSource>();
            if (audio != null)
            {
                bool isMusicOn = PlayerPrefs.GetInt("MusicState", 1) == 1;
                audio.mute = !isMusicOn;
            }
        }
    }

    private void CalculateInactiveIncome()
    {
        inactiveIncomePerTick = 0;

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (activeArea == 0)
            {
                foreach (int level in data.carnivoreDinoLevels) inactiveIncomePerTick += (level * 5);
            }
            else 
            {
                foreach (int level in data.herbivoreDinoLevels) inactiveIncomePerTick += (level * 5);
            }
        }
    }

    IEnumerator InactiveAreaGoldRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (inactiveIncomePerTick > 0)
            {
                AddGold(inactiveIncomePerTick);
            }
        }
    }

    public void SwitchHabitat()
    {
        SaveGame();

        DinoController[] dinos = FindObjectsByType<DinoController>(FindObjectsSortMode.None);
        foreach (var d in dinos) 
        {
            d.gameObject.SetActive(false); 
            Destroy(d.gameObject);
        }
        
        GameObject[] eggs = GameObject.FindGameObjectsWithTag("Egg");
        foreach (var e in eggs) 
        {
            e.SetActive(false);
            Destroy(e);
        }

        activeArea = (activeArea == 0) ? 1 : 0;

        SaveGameWithoutDinos();
        LoadGameDinosOnly(); 
        UpdateAreaVisuals(); 
        CalculateInactiveIncome();
    }

    public void SaveGameWithoutDinos()
    {
        if (!File.Exists(saveFilePath)) return;
        
        string json = File.ReadAllText(saveFilePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        data.lastActiveArea = activeArea;
        data.isCarnivoreUnlocked = isCarnivoreUnlocked;
        data.savedGold = currentGold;

        File.WriteAllText(saveFilePath, JsonUtility.ToJson(data));
    }

    private void LoadGameDinosOnly()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (activeArea == 0)
            {
                foreach (int level in data.herbivoreDinoLevels) SpawnNextLevelDino(level, RandomSpawnPos());
                for (int i = 0; i < data.herbivoreEggCount; i++) Instantiate(eggPrefab, RandomSpawnPos(), Quaternion.identity);
            }
            else 
            {
                foreach (int level in data.carnivoreDinoLevels) SpawnNextLevelDino(level, RandomSpawnPos());
                for (int i = 0; i < data.carnivoreEggCount; i++) Instantiate(eggPrefab, RandomSpawnPos(), Quaternion.identity);
            }
        }
    }

    private Vector3 RandomSpawnPos()
    {
        return new Vector3(Random.Range(-2f, 2f), Random.Range(-3f, 3f), 0f);
    }

    private void UpdateAreaVisuals()
    {
        if (switchAreaButton != null) switchAreaButton.SetActive(isCarnivoreUnlocked);
        if (backgroundRenderer != null) backgroundRenderer.sprite = (activeArea == 0) ? herbivoreBgSprite : carnivoreBgSprite;
        UpdatePriceUI(); 
    }

    private void UpdatePriceUI()
    {
        if (buyEggText != null)
        {
            int currentPrice = (activeArea == 0) ? herbivoreEggPrice : carnivoreEggPrice;
            buyEggText.text = "BUY EGG (" + currentPrice + " GOLD)";
        }
    }
    
    public void CheckNewDinoDiscovery(int dinoLevel)
    {
        if (!unlockedDinoLevels.Contains(dinoLevel))
        {
            unlockedDinoLevels.Add(dinoLevel); 
            SaveGame(); 

            int index = dinoLevel - 1;
            if (index >= 0 && index < discoverySprites.Length && discoverySprites[index] != null)
            {
                if (discoverySound != null && PlayerPrefs.GetInt("SFXState", 1) == 1) 
                    AudioSource.PlayClipAtPoint(discoverySound, Camera.main.transform.position, 1f);

                discoveryImage.sprite = discoverySprites[index]; 
                discoveryPanel.SetActive(true); 
                Time.timeScale = 0f; 
            }
        }
    }

    public void CloseDiscoveryScreen()
    {
        discoveryPanel.SetActive(false); 
        Time.timeScale = 1f; 
    }

    public void HandleLevel9Merge(Vector3 spawnPos)
    {
        StartCoroutine(Level9MergeRoutine(spawnPos));
    }

    private IEnumerator Level9MergeRoutine(Vector3 spawnPos)
    {
        yield return new WaitForEndOfFrame();

        if (!isCarnivoreUnlocked)
        {
            isCarnivoreUnlocked = true;
            SaveGame(); 
            UpdateAreaVisuals(); 

            pendingSpecialEgg = true;
            pendingEggPos = spawnPos;

            if (newEraSound != null && PlayerPrefs.GetInt("SFXState", 1) == 1) 
                AudioSource.PlayClipAtPoint(newEraSound, Camera.main.transform.position, 1f);

            if (newEraPanel != null) newEraPanel.SetActive(true);
            Time.timeScale = 0f; 
        }
        else
        {
            if (activeArea == 0) SwitchHabitat();
            
            SpawnSpecialEgg(spawnPos);
        }
    }

    public void CloseNewEraScreen()
    {
        if (newEraPanel != null) newEraPanel.SetActive(false);
        Time.timeScale = 1f; 
        
        if (activeArea == 0) SwitchHabitat();

        if (pendingSpecialEgg)
        {
            SpawnSpecialEgg(pendingEggPos);
            pendingSpecialEgg = false;
        }
    }

    private void SpawnSpecialEgg(Vector3 pos)
    {
        GameObject specialEgg = Instantiate(eggPrefab, pos, Quaternion.identity);
        EggHatch eggScript = specialEgg.GetComponent<EggHatch>();
        if (eggScript != null) 
        {
            eggScript.customHatchLevel = 10;
            eggScript.targetArea = 1; 
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            data = JsonUtility.FromJson<SaveData>(json);
        }

        data.savedGold = currentGold;
        data.unlockedDinoLevels = new List<int>(unlockedDinoLevels);
        data.lastActiveArea = activeArea;
        data.isCarnivoreUnlocked = isCarnivoreUnlocked;

        DinoController[] allDinos = FindObjectsByType<DinoController>(FindObjectsSortMode.None);
        GameObject[] activeEggs = GameObject.FindGameObjectsWithTag("Egg");

        if (activeArea == 0) 
        {
            data.herbivoreDinoLevels.Clear();
            foreach (DinoController dino in allDinos) data.herbivoreDinoLevels.Add(dino.dinoLevel);
            data.herbivoreEggCount = activeEggs.Length;
        }
        else 
        {
            data.carnivoreDinoLevels.Clear();
            foreach (DinoController dino in allDinos) data.carnivoreDinoLevels.Add(dino.dinoLevel);
            data.carnivoreEggCount = activeEggs.Length;
        }

        string finalJson = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, finalJson);
        
        CalculateInactiveIncome();
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            currentGold = data.savedGold;
            UpdateGoldUI();

            if (data.unlockedDinoLevels != null) unlockedDinoLevels = data.unlockedDinoLevels;
            activeArea = data.lastActiveArea;
            isCarnivoreUnlocked = data.isCarnivoreUnlocked;

            LoadGameDinosOnly();
            UpdateAreaVisuals(); 
        }
        else
        {
            UpdateGoldUI();
            Instantiate(eggPrefab, RandomSpawnPos(), Quaternion.identity); 
            UpdateAreaVisuals();
        }
        
        CalculateInactiveIncome();
    }

#if UNITY_EDITOR
    [ContextMenu("Reset Save Data (Delete JSON)")]
    public void ResetSaveData()
    {
        string path = Application.persistentDataPath + "/dino_save.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("JSON File DELETED!");
        }
        
        PlayerPrefs.DeleteAll(); 

        currentGold = 0;
        activeArea = 0;
        isCarnivoreUnlocked = false;
        inactiveIncomePerTick = 0; 
        if (unlockedDinoLevels != null) unlockedDinoLevels.Clear();
        
        if (Application.isPlaying)
        {
            DinoController[] dinos = FindObjectsByType<DinoController>(FindObjectsSortMode.None);
            foreach (var d in dinos) Destroy(d.gameObject);
            
            GameObject[] eggs = GameObject.FindGameObjectsWithTag("Egg");
            foreach (var e in eggs) Destroy(e);
            
            UpdateGoldUI();
            UpdateAreaVisuals();
        }
    }
#endif

    private void OnApplicationQuit() { SaveGame(); }
    private void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveGame(); }

    public void AddGold(int amount) { currentGold += amount; UpdateGoldUI(); SaveGame(); }
    
    // Taşmayı önlemek için K/M/B formatıyla güncellenen altın UI metodu
    private void UpdateGoldUI() 
    { 
        if (goldText != null) goldText.text = FormatGold(currentGold); 
    }

    private string FormatGold(int amount)
    {
        if (amount >= 1000000000) return (amount / 1000000000f).ToString("0.#") + "B";
        if (amount >= 1000000) return (amount / 1000000f).ToString("0.#") + "M";
        if (amount >= 1000) return (amount / 1000f).ToString("0.#") + "K";
        return amount.ToString();
    }

    public void BuyDino()
    {
        int currentPrice = (activeArea == 0) ? herbivoreEggPrice : carnivoreEggPrice;

        if (currentGold >= currentPrice)
        {
            currentGold -= currentPrice; 
            UpdateGoldUI(); 
            SaveGame(); 
            Instantiate(eggPrefab, RandomSpawnPos(), Quaternion.identity);
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

    public void LoadMainMenu() 
    { 
        SaveGame(); 
        SceneManager.LoadScene(0); 
    }
    
}
[System.Serializable]
public class SaveData
{
    public int savedGold;
    public List<int> unlockedDinoLevels = new List<int>(); 
    public int lastActiveArea = 0; 
    public bool isCarnivoreUnlocked = false; 

    public int herbivoreEggCount;
    public List<int> herbivoreDinoLevels = new List<int>();

    public int carnivoreEggCount;
    public List<int> carnivoreDinoLevels = new List<int>();
}