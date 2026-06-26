using UnityEngine;

public class EggHatch : MonoBehaviour
{
    [Header("Görseller")]
    public Sprite crackedEggSprite; 

    [Header("Ses Ayarları")]
    public AudioClip crackSound;  
    public AudioClip breakSound;  

    [Header("Özel Yumurta Ayarı")]
    public int customHatchLevel = 0; 
    public int targetArea = 0; // YENİ: Yumurtanın ait olduğu hedef alan (0=Otçul, 1=Etçil)

    private SpriteRenderer spriteRenderer;
    private int clickCount = 0;
    private bool hasSpawned = false; 

    [Header("Sürükleme ve Tıklama Ayarları")]
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 startMousePos;
    private Vector3 originalScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale; 
    }

    void OnMouseDown()
    {
        transform.localScale = originalScale * 1.1f;
        startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - startMousePos;
        isDragging = false; 
    }

    void OnMouseDrag()
    {
        Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if (Vector3.Distance(startMousePos, currentMousePos) > 0.1f)
        {
            isDragging = true;
        }

        if (isDragging)
        {
            transform.position = currentMousePos + offset;
        }
    }

    void OnMouseUp()
    {
        transform.localScale = originalScale;

        if (isDragging || hasSpawned)
        {
            return;
        }

        HatchProcess();
    }

    void HatchProcess()
    {
        clickCount++;

        if (clickCount == 1)
        {
            if (crackedEggSprite != null) spriteRenderer.sprite = crackedEggSprite;
            if (crackSound != null) AudioSource.PlayClipAtPoint(crackSound, Camera.main.transform.position, 1f);
        }
        else if (clickCount >= 2)
        {
            hasSpawned = true; 

            if (breakSound != null) AudioSource.PlayClipAtPoint(breakSound, Camera.main.transform.position, 1f);

            if (GameManager.Instance != null)
            {
                // Mevcut alanı hafızaya al
                int originalArea = GameManager.Instance.activeArea;
                
                // Eğer bu özel bir yumurtaysa (9+9'dan geldiyse), alanı zorla hedef alana çek
                if (customHatchLevel > 0)
                {
                    GameManager.Instance.activeArea = targetArea;
                }

                // Yumurtadan çıkacak seviyeyi belirle
                int levelToSpawn = (customHatchLevel > 0) ? customHatchLevel : ((GameManager.Instance.activeArea == 0) ? 1 : 10);

                // Dinozoru doğur (Geçici olarak değiştirilen alana kaydedilecek)
                GameManager.Instance.SpawnNextLevelDino(levelToSpawn, transform.position);
                
                // Doğum bittikten sonra alanı tekrar eski (gerçek) haline döndür
                GameManager.Instance.activeArea = originalArea;
            }
            
            Destroy(gameObject);
        }
    }
}