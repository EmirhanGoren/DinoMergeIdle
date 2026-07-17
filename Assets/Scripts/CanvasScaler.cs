using UnityEngine;
using UnityEngine.UI;

public class AppSettings : MonoBehaviour
{
    [Header("Screen Ratio")]
    public float screenRatio;
    public CanvasScaler canvasScaler;

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        SetCanvasScaler();
    }


    private void SetCanvasScaler()
    {
        screenRatio = (float)Screen.height / (float)Screen.width;

        if (screenRatio >= (float)16 / 9)
        {
            canvasScaler.matchWidthOrHeight = 0;
        }
        else
        {
            canvasScaler.matchWidthOrHeight = 1;
        }

    }

}