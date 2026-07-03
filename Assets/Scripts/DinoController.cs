using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class DinoController : MonoBehaviour
{
    [Header("Dinozor Kimliği")]
    public int dinoLevel = 1;

    [Header("Ses Ayarları")]
    public AudioClip clickSound;
    public AudioClip mergeSound; 

    [Header("UI Ayarları")]
    public GameObject floatingTextPrefab; 

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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckNewDinoDiscovery(dinoLevel);
        }

        StartCoroutine(AutoEarnGold());
    }

    IEnumerator AutoEarnGold()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (GameManager.Instance != null)
            {
                int earnedGold = dinoLevel * 5;
                GameManager.Instance.AddGold(earnedGold);
                ShowFloatingGold(earnedGold);
            }
        }
    }

    void OnMouseDown()
    {
        isDragging = true;

        // YENİ: Sadece SFX açıksa tıklama sesini çal
        if (clickSound != null && PlayerPrefs.GetInt("SFXState", 1) == 1)
        {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, 1f);
        }

        if (GameManager.Instance != null)
        {
            int earnedGold = dinoLevel * 1;
            GameManager.Instance.AddGold(earnedGold);
            ShowFloatingGold(earnedGold);
        }

        if (!isScaling)
        {
            StartCoroutine(ClickEffect());
        }

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        offset = transform.position - mouseWorldPos;
        rb.linearVelocity = Vector2.zero;
        sonFarePozisyonu = mouseWorldPos;
    }

   void OnMouseDrag()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        if (Time.deltaTime > 0f)
        {
            firlatmaHizi = ((Vector2)mouseWorldPos - sonFarePozisyonu) / Time.deltaTime;
        }
        else
        {
            firlatmaHizi = Vector2.zero;
        }

        sonFarePozisyonu = mouseWorldPos;
        rb.MovePosition(mouseWorldPos + offset);
    }

    void OnMouseUp()
    {
        isDragging = false;
        
        if (float.IsNaN(firlatmaHizi.x) || float.IsNaN(firlatmaHizi.y))
        {
            firlatmaHizi = Vector2.zero;
        }

        rb.linearVelocity = Vector2.ClampMagnitude(firlatmaHizi * 0.8f, 25f);
        transform.localScale = orijinalBoyut; 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        DinoController digerDino = collision.gameObject.GetComponent<DinoController>();

        if (digerDino != null)
        {
            if (!this.isDragging && !digerDino.isDragging) return;

            if (this.dinoLevel == digerDino.dinoLevel && !this.isMerging && !digerDino.isMerging)
            {
                if (this.gameObject.GetInstanceID() > digerDino.gameObject.GetInstanceID())
                {
                    this.isMerging = true;
                    digerDino.isMerging = true;

                    // YENİ: Sadece SFX açıksa birleşme sesini çal
                    if (mergeSound != null && PlayerPrefs.GetInt("SFXState", 1) == 1)
                    {
                        AudioSource.PlayClipAtPoint(mergeSound, Camera.main.transform.position, 1f);
                    }

                    Vector3 ortaNokta = (transform.position + collision.transform.position) / 2f;
                    
                    if (GameManager.Instance != null)
                    {
                        if (this.dinoLevel == 9)
                        {
                            GameManager.Instance.HandleLevel9Merge(ortaNokta);
                        }
                        else
                        {
                            GameManager.Instance.SpawnNextLevelDino(dinoLevel + 1, ortaNokta);
                        }
                    }
                    
                    Destroy(collision.gameObject);
                    Destroy(this.gameObject);
                }
            }
        }
    }

    private void ShowFloatingGold(int amount)
    {
        if (floatingTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 0.8f, 0); 
            GameObject textObj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
            textObj.GetComponentInChildren<TextMeshPro>().text = "+" + amount.ToString();
        }
    }

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