using UnityEngine;

public class DinoMerge : MonoBehaviour
{
    [Header("Dinozor Bilgileri")]
    public int dinoLevel = 1; 
    
    private bool isMerging = false; 

    void OnCollisionEnter2D(Collision2D collision)
    {
        DinoMerge digerDino = collision.gameObject.GetComponent<DinoMerge>();

        if (digerDino != null)
        {
            DraggableDino benimDrag = GetComponent<DraggableDino>();
            DraggableDino digerDrag = collision.gameObject.GetComponent<DraggableDino>();

            if (benimDrag != null && digerDrag != null)
            {
                if (!benimDrag.isDragging && !digerDrag.isDragging) return; 
            }

            if (this.dinoLevel == digerDino.dinoLevel && !this.isMerging && !digerDino.isMerging)
            {
                if (this.gameObject.GetInstanceID() > digerDino.gameObject.GetInstanceID())
                {
                    this.isMerging = true;
                    digerDino.isMerging = true;

                    // TAM YOK OLMADAN ÖNCE BİR ÜST SEVİYEYİ ÇAĞIRIYORUZ
                    // İki dinozorun tam ortasında doğması için pozisyon ortalamasını aldık
                    Vector3 ortaNokta = (transform.position + collision.transform.position) / 2f;
                    GameManager.Instance.SpawnNextLevelDino(dinoLevel + 1, ortaNokta);
                    
                    Destroy(collision.gameObject);
                    Destroy(this.gameObject);
                }
            }
        }
    }
}