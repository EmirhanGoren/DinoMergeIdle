using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DinoRoam : MonoBehaviour
{
    [Header("Yapay Zeka Ayarları")]
    public float moveSpeed = 5f;        // Yürüme hızı (Damping 25 olduğu için biraz yüksek tutuyoruz)
    public float minWaitTime = 3f;      // İki yürüyüş arası minimum bekleme süresi
    public float maxWaitTime = 7f;      // İki yürüyüş arası maksimum bekleme süresi
    public float walkDuration = 1.2f;    // Bir adımda ne kadar süre boyunca yürüyecek?

    private Rigidbody2D rb;
    private DraggableDino dragScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dragScript = GetComponent<DraggableDino>();

        // Yapay zeka döngüsünü (Coroutine) başlatıyoruz
        StartCoroutine(RoamRoutine());
    }

    IEnumerator RoamRoutine()
    {
        while (true)
        {
            // 1. Durma/Bekleme Aşaması (Idle)
            // Dinozor bir süre durup etrafa bakınıyor
            float randomWait = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(randomWait);

            // Eğer oyuncu o an dinozoru tutuyorsa, yürüyüşe geçme, döngünün başına dön
            if (dragScript != null && dragScript.isDragging) continue;

            // 2. Yön Seçme Aşaması
            // 2D düzlemde rastgele bir yön seçip onu normalize ediyoruz (hızı standart olsun diye)
            Vector2 randomDirection = Random.insideUnitCircle.normalized;

            // 3. Yürüme Aşaması (Walk)
            // Belirlenen süre boyunca dinozoru o yöne doğru sürekli hareket ettir
            float timer = 0f;
            while (timer < walkDuration)
            {
                // Yürürken oyuncu birden üstümüze tıklarsa yürümeyi anında kes!
                if (dragScript != null && dragScript.isDragging) break;

                // Fizik motoruna yürüme hızını sürekli dikte ediyoruz (Damping'i yenmek için)
                rb.linearVelocity = randomDirection * moveSpeed;

                timer += Time.deltaTime;
                yield return null; // Bir sonraki kareye (frame) kadar bekle
            }

            // 4. Durma Aşaması
            // Adım bittiğinde hızı sıfırla ki sürtünmeyle pürüzsüzce dursun
            if (dragScript != null && !dragScript.isDragging)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}