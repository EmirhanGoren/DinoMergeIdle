using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dinozor Şablonları Listesi")]
    public GameObject[] dinoPrefabs;

    [Header("Yumurta Ayarları")]
    public GameObject eggPrefab; 

    [Header("Ekonomi Ayarları")]
    public int currentGold = 0;
    public int currentAreaEggPrice = 50; // Otçullar için 50, Etçiller için ileride değişecek
    
    [Header("UI Ayarları")]
    public TextMeshProUGUI goldText; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // JSON sistemi gelene kadar başlangıçta altınımız sıfır veya editörden ne girdiysek o.
        UpdateGoldUI();
        
        // OYUN BAŞINDA BEDAVA YUMURTA
        Instantiate(eggPrefab, Vector3.zero, Quaternion.identity); 
    }

    // Altın ekleme fonksiyonu
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = currentGold.ToString();
        }
    }

    // Satın Al butonuna basıldığında çalışacak fonksiyon
    public void BuyDino()
    {
        if (currentGold >= currentAreaEggPrice)
        {
            currentGold -= currentAreaEggPrice; 
            UpdateGoldUI();
            
            // Rastgele bir konum belirle ve yumurtayı fırlat
            Vector3 randomPos = new Vector3(Random.Range(-2f, 2f), Random.Range(-3f, 3f), 0f);
            Instantiate(eggPrefab, randomPos, Quaternion.identity);
        }
        else
        {
            Debug.Log("Altın Yetersiz! Tıklamaya devam et.");
        }
    }

    public void SpawnNextLevelDino(int nextLevel, Vector3 spawnPosition)
    {
        int targetIndex = nextLevel - 1;
        if (targetIndex < dinoPrefabs.Length && dinoPrefabs[targetIndex] != null)
        {
            GameObject yeniDino = Instantiate(dinoPrefabs[targetIndex], spawnPosition, Quaternion.identity);
            
            DinoController controllerScript = yeniDino.GetComponent<DinoController>();
            if (controllerScript != null)
            {
                controllerScript.dinoLevel = nextLevel;
            }
        }
    }

    // Ana Menüye Dönüş (Sol üstteki ev ikonu için)
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0); 
    }
}