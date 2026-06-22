using UnityEngine;

public class EggHatch : MonoBehaviour
{
    [Header("Görseller ve Şablonlar")]
    public Sprite crackedEggSprite;
    public GameObject level1DinoPrefab;

    private SpriteRenderer spriteRenderer;
    private int clickCount = 0;

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
        // DEĞİŞEN KISIM: 0.9f yerine 1.1f yaptık. Artık basınca %10 BÜYÜYECEK.
        // Eğer daha da çok şişmesini istersen bu sayıyı 1.15f veya 1.2f yapabilirsin.
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
        // Bırakınca tekrar orijinal (normal) boyutuna dönüyor
        transform.localScale = originalScale;

        if (!isDragging)
        {
            HatchProcess();
        }
    }

    void HatchProcess()
    {
        clickCount++;

        if (clickCount == 1)
        {
            if (crackedEggSprite != null)
            {
                spriteRenderer.sprite = crackedEggSprite;
            }
        }
        else if (clickCount >= 2)
        {
            if (level1DinoPrefab != null)
            {
                Instantiate(level1DinoPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}