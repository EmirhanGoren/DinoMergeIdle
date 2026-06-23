using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panel Ayarları")]
    public GameObject settingsPanel;
    
    [Header("Ses Ayarları")]
    public Image toggleImage; 
    public Sprite onSprite;   
    public Sprite offSprite;  
    private bool isSoundOn = true;

    void Start()
    {
        // 1. OYUN AÇILDIĞINDA HAFIZAYI KONTROL ET
        // "SoundState" adında bir kayıt var mı bak. Yoksa varsayılan olarak 1 (Açık) kabul et.
        int savedSound = PlayerPrefs.GetInt("SoundState", 1);
        
        // Eğer 1 ise isSoundOn true olur, 0 ise false olur.
        isSoundOn = (savedSound == 1);

        // Başlangıçta sesi ve görseli bu hafızaya göre ayarla
        AudioListener.pause = !isSoundOn;
        if(toggleImage != null)
        {
            toggleImage.sprite = isSoundOn ? onSprite : offSprite;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        AudioListener.pause = !isSoundOn;
        
        if(toggleImage != null)
        {
            toggleImage.sprite = isSoundOn ? onSprite : offSprite;
        }

        // 2. DEĞİŞİKLİĞİ HAFIZAYA KAYDET
        // isSoundOn true ise 1 kaydet, false ise 0 kaydet.
        PlayerPrefs.SetInt("SoundState", isSoundOn ? 1 : 0);
        PlayerPrefs.Save(); // Kaydı kesinleştir
    }
}