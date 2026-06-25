using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DinoController : MonoBehaviour
{
    [Header("Dinozor Kimliği")]
    public int dinoLevel = 1;

    [Header("Ses Ayarları")]
    public AudioClip clickSound;

    [Header("Sürükleme Ayarları")]
    [HideInInspector] public bool isDragging = false;
    private Vector3 offset;
    private Vector2 sonFarePozisyonu;
    private Vector2 firlatmaHizi;

    [Header("Görsel Efektler")]
    private Vector3 orijinalBoyut;
    private bool isScaling = false;

    private Rigidbody2D rb;
    private bool isMerging = false;

   void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        orijinalBoyut = transform.localScale;

        // --- YENİ EKLENEN KEŞİF KONTROLÜ ---
        // Dinozor doğduğu an GameManager'a sorar: "Beni daha önce gördün mü?"
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckNewDinoDiscovery(dinoLevel);
        }
        // ------------------------------------

        // Otomatik altın kazanma döngüsünü başlat
        StartCoroutine(AutoEarnGold());
    }

    // --- 1. EKONOMİ (OTOMATİK ALTIN) ---
    IEnumerator AutoEarnGold()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGold(dinoLevel * 5);
            }
        }
    }

    // --- 2. INPUT: TIKLAMA VE SÜRÜKLEME ---
    void OnMouseDown()
    {
        isDragging = true;

        // 1. Ses Çal
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, 1f);
        }

        // 2. Tıklama Altını Kazan
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(1);
        }

        // 3. Squish (Zıplama/Büyüme) Efekti
        if (!isScaling)
        {
            StartCoroutine(ClickEffect());
        }

        // 4. Sürükleme Fiziği Başlangıcı
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        offset = transform.position - mouseWorldPos;
        rb.linearVelocity = Vector2.zero;
        sonFarePozisyonu = mouseWorldPos;
    }

   void OnMouseDrag()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        // EĞER OYUN DURMAMIŞSA (Zaman akıyorsa) fırlatma hızını hesapla
        if (Time.deltaTime > 0f)
        {
            firlatmaHizi = ((Vector2)mouseWorldPos - sonFarePozisyonu) / Time.deltaTime;
        }
        else
        {
            // Zaman durmuşsa (Keşif ekranı açıksa) fırlatma hızı olmasın
            firlatmaHizi = Vector2.zero;
        }

        sonFarePozisyonu = mouseWorldPos;
        rb.MovePosition(mouseWorldPos + offset);
    }

    void OnMouseUp()
    {
        isDragging = false;
        
        // GÜVENLİK KONTROLÜ: Hızın içinde tanımsız (NaN) bir sayı kalmışsa sıfırla
        if (float.IsNaN(firlatmaHizi.x) || float.IsNaN(firlatmaHizi.y))
        {
            firlatmaHizi = Vector2.zero;
        }

        // Bırakınca fırlatma hızı uygula
        rb.linearVelocity = Vector2.ClampMagnitude(firlatmaHizi * 0.8f, 25f);
        
        // Bırakıldığı an boyutu kesin olarak normale döndür
        transform.localScale = orijinalBoyut; 
    }

    // --- 3. BİRLEŞME (MERGE MANTIĞI) ---
    void OnCollisionEnter2D(Collision2D collision)
    {
        DinoController digerDino = collision.gameObject.GetComponent<DinoController>();

        if (digerDino != null)
        {
            // İkisi de yerde duruyorsa birleşmeye izin verme (biri sürüklenip bırakılmış olmalı)
            if (!this.isDragging && !digerDino.isDragging) return;

            // Seviyeler aynıysa ve o an başka bir birleşme işlemi yoksa
            if (this.dinoLevel == digerDino.dinoLevel && !this.isMerging && !digerDino.isMerging)
            {
                // İki obje aynı anda birbirini yok etmeye çalışmasın diye ID kontrolü
                if (this.gameObject.GetInstanceID() > digerDino.gameObject.GetInstanceID())
                {
                    this.isMerging = true;
                    digerDino.isMerging = true;

                    Vector3 ortaNokta = (transform.position + collision.transform.position) / 2f;
                    
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SpawnNextLevelDino(dinoLevel + 1, ortaNokta);
                    }
                    
                    Destroy(collision.gameObject);
                    Destroy(this.gameObject);
                }
            }
        }
    }

    // --- YARDIMCI FONKSİYONLAR ---
    IEnumerator ClickEffect()
    {
        isScaling = true;
        transform.localScale = orijinalBoyut * 1.2f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = orijinalBoyut;
        isScaling = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}