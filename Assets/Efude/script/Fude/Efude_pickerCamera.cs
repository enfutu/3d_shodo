
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_pickerCamera : UdonSharpBehaviour
{
    public Texture2D pickerBuffer;
    [SerializeField] private Camera targetCamera;
    [SerializeField]

    //起動時に一度solidcolor
    private void Start()
    {
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = Color.black;
        SendCustomEventDelayedFrames("ResetCamera", 10);
        sendTexture();
    }

    public void ResetCamera()
    {   
        targetCamera.clearFlags = CameraClearFlags.Nothing;
    }

    private void OnPostRender()
    {
        sendTexture();
    }

    private void sendTexture()
    {
        pickerBuffer.ReadPixels(targetCamera.pixelRect, 0, 0);
        pickerBuffer.Apply(false);
    }
}
