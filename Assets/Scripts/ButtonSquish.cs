using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSquish : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 originalScale;
    
    [Header("Animasyon Ayarı")]
    [Tooltip("Basıldığında butonun yeni boyutu (0.9 = %10 küçülür, 1.1 = %10 büyür)")]
    public float squishAmount = 0.9f; 

    [Header("Ses Ayarı")]
    [Tooltip("Butona basıldığında çıkacak sesi buraya sürükle")]
    public AudioClip buttonSound;

    void Start()
    {
        // Butonun orijinal boyutunu hafızaya al
        originalScale = transform.localScale;
    }

    // Fareyle veya parmakla butona BASILDIĞI AN çalışır
    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. Görsel Efekt: Butonu küçült/büyüt
        transform.localScale = originalScale * squishAmount;

        // 2. İşitsel Efekt: Sesi çal (YENİ: Sadece SFX açıksa çalacak)
        if (buttonSound != null && PlayerPrefs.GetInt("SFXState", 1) == 1)
        {
            // Sesi kameranın olduğu konumda net bir şekilde çal
            AudioSource.PlayClipAtPoint(buttonSound, Camera.main.transform.position, 1f);
        }
    }

    // Fare veya parmak butondan ÇEKİLDİĞİ AN çalışır
    public void OnPointerUp(PointerEventData eventData)
    {
        // Butonu orijinal boyutuna geri döndür
        transform.localScale = originalScale;
    }
}