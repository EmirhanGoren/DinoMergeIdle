using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DinoRoam : MonoBehaviour
{
    [Header("Yapay Zeka Ayarları")]
    public float moveSpeed = 5f;        
    public float minWaitTime = 3f;      
    public float maxWaitTime = 7f;      
    public float walkDuration = 1.2f;   

    private Rigidbody2D rb;
    private DinoController dinoController; // GÜNCELLENDİ: Artık ana kontrolcüyü okuyor

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dinoController = GetComponent<DinoController>();

        StartCoroutine(RoamRoutine());
    }

    IEnumerator RoamRoutine()
    {
        while (true)
        {
            float randomWait = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(randomWait);

            if (dinoController != null && dinoController.isDragging) continue;

            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float timer = 0f;

            while (timer < walkDuration)
            {
                if (dinoController != null && dinoController.isDragging) break;

                rb.linearVelocity = randomDirection * moveSpeed;

                timer += Time.deltaTime;
                yield return null; 
            }

            if (dinoController != null && !dinoController.isDragging)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}