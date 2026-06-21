using UnityEngine;
using TMPro; // TextMeshPro (Yazı) sistemi için şart

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dinozor Şablonları Listesi")]
    public GameObject[] dinoPrefabs;

    [Header("Ekonomi Ayarları")]
    public int currentGold = 0;
    public int dinoPrice = 50; // 1. Seviye dinozorun fiyatı
    public TextMeshProUGUI goldText; // Ekrana yazdıracağımız yer

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateGoldUI();
        
        // OYUN BAŞINDA BEDAVA DİNOZOR VERME: 
        // Oyun başlar başlamaz ekranın tam ortasına (0,0,0) 1. seviye bir dinozor fırlat.
        SpawnNextLevelDino(1, Vector3.zero);
    }

    // Altın ekleme fonksiyonu (Dinozorlar bu fonksiyonu çağıracak)
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = "Altın: " + currentGold;
        }
    }

    // Satın Al butonuna basıldığında çalışacak fonksiyon
    public void BuyDino()
    {
        if (currentGold >= dinoPrice)
        {
            currentGold -= dinoPrice; // Parayı kes
            UpdateGoldUI();
            
            // Yeni dinozorun diğerlerinin tam üstüne değil, rastgele hafif kaymış bir yere düşmesi için:
            Vector3 randomPos = new Vector3(Random.Range(-2f, 2f), Random.Range(-3f, 3f), 0f);
            SpawnNextLevelDino(1, randomPos);
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
            
            DinoMerge mergeScript = yeniDino.GetComponent<DinoMerge>();
            if (mergeScript != null)
            {
                mergeScript.dinoLevel = nextLevel;
            }
        }
    }
}