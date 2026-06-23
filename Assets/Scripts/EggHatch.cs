using UnityEngine;

public class EggHatch : MonoBehaviour
{
    [Header("Görseller")]
    public Sprite crackedEggSprite; // Çatlak yumurta resmi

    [Header("Ses Ayarları")]
    public AudioClip crackSound;  // Çatlama sesi
    public AudioClip breakSound;  // Kırılma sesi

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
        // Tıklanınca yumurtayı %10 büyüt (Squish efekti)
        transform.localScale = originalScale * 1.1f;

        startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - startMousePos;
        isDragging = false; 
    }

    void OnMouseDrag()
    {
        Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Eğer 0.1f'den fazla fare hareket ettiyse bunu "sürükleme" olarak say
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
        // Parmağı çekince yumurtayı eski normal boyutuna döndür
        transform.localScale = originalScale;

        // Eğer yumurtayı sürüklediysek veya içinden dinozor çoktan çıktıysa kodu burada durdur!
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
            // 1. Tıklama: Görseli değiştir ve çatlama sesini çal
            if (crackedEggSprite != null)
            {
                spriteRenderer.sprite = crackedEggSprite;
            }
            if (crackSound != null)
            {
                AudioSource.PlayClipAtPoint(crackSound, Camera.main.transform.position, 1f);
            }
        }
        else if (clickCount >= 2)
        {
            hasSpawned = true; // İkiz dinozor doğmasını engeller

            // 2. Tıklama: Kırılma sesini çal ve GameManager'dan dinozor iste
            if (breakSound != null)
            {
                AudioSource.PlayClipAtPoint(breakSound, Camera.main.transform.position, 1f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SpawnNextLevelDino(1, transform.position);
            }
            
            Destroy(gameObject);
        }
    }
}