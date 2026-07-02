using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1f; 
    public float destroyTime = 1.2f; 

    void Start()
    {
        // Yazıyı belirlenen süre sonunda sahneden sil
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Yazıyı yukarı doğru uçur
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
    }
}