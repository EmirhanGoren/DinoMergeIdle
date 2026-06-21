using System.Collections;
using UnityEngine;

public class DinoEconomy : MonoBehaviour
{
    private DinoMerge mergeScript;
    
    // Büyüyüp küçülme için gereken değişkenler
    private Vector3 orijinalBoyut;
    private bool isScaling = false; // Ardı ardına tıklamada bug olmasını engeller

    void Start()
    {
        mergeScript = GetComponent<DinoMerge>();
        
        // Oyun başında objenin ilk boyutunu hafızaya kazı
        orijinalBoyut = transform.localScale; 
        
        StartCoroutine(AutoEarnGold());
    }

    IEnumerator AutoEarnGold()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            GameManager.Instance.AddGold(mergeScript.dinoLevel * 10); 
        }
    }

    void OnMouseDown()
    {
        // 1. Altın kazanma mantığı
        GameManager.Instance.AddGold(mergeScript.dinoLevel * 1);

        // 2. Büyüyüp Küçülme Efekti (Eğer o an zaten animasyon oynamıyorsa)
        if (!isScaling)
        {
            StartCoroutine(ClickEffect());
        }
    }

    // Saf Unity zamanlayıcısı ile animasyon
    IEnumerator ClickEffect()
    {
        isScaling = true; // Animasyon başladı kilidi
        
        // Objenin boyutunu anında %20 büyüt (1.2 katı)
        transform.localScale = orijinalBoyut * 1.2f;
        
        // Havada 0.1 saniye asılı kal
        yield return new WaitForSeconds(0.1f);
        
        // Geri orijinal boyuta dön
        transform.localScale = orijinalBoyut;
        
        isScaling = false; // Kilit açıldı
    }
}