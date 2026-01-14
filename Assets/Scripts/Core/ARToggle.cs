using UnityEngine;

public class ARToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Material skyboxMaterial;
    
    [Header("Settings")]
    [SerializeField] private Color vrBackgroundColor = Color.black;
    
    private bool isARMode = false;
    
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Store original skybox
        if (skyboxMaterial == null && RenderSettings.skybox != null)
        {
            skyboxMaterial = RenderSettings.skybox;
        }
        
        // Start in VR mode
        SetVRMode();
    }
    
    public void ToggleARMode()
    {
        isARMode = !isARMode;
        
        if (isARMode)
        {
            SetARMode();
        }
        else
        {
            SetVRMode();
        }
    }
    
    private void SetARMode()
    {
        // Clear background for passthrough
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0, 0, 0, 0);
        RenderSettings.skybox = null;
        
        // On Quest, this enables passthrough
        // For editor testing, we just see black/transparent
        
        Debug.Log("AR Mode enabled - Passthrough active");
    }
    
    private void SetVRMode()
    {
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f); // Dark charcoal
        RenderSettings.skybox = null;
        RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.45f);
        
        Debug.Log("VR Mode enabled");
    }
    
    public bool IsARMode()
    {
        return isARMode;
    }
}