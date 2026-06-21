using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DraggableDino : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private Rigidbody2D rb;

    private Vector2 sonFarePozisyonu;
    private Vector2 firlatmaHizi;

    // DinoMerge kodunun okuyabilmesi için bu değişkeni ekledik
    [HideInInspector] public bool isDragging = false; 

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    void OnMouseDown()
    {
        isDragging = true; // Oyuncu tutmaya başladı

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        offset = transform.position - mouseWorldPos;

        rb.linearVelocity = Vector2.zero;
        sonFarePozisyonu = mouseWorldPos; 
    }

    void OnMouseDrag()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        firlatmaHizi = ((Vector2)mouseWorldPos - sonFarePozisyonu) / Time.deltaTime;
        sonFarePozisyonu = mouseWorldPos;

        rb.MovePosition(mouseWorldPos + offset);
    }

    void OnMouseUp()
    {
        isDragging = false; // Oyuncu bıraktı

        rb.linearVelocity = Vector2.ClampMagnitude(firlatmaHizi * 0.8f, 25f);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }
}