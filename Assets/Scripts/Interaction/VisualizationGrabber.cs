using UnityEngine;
using UnityEngine.InputSystem;

public class VisualizationGrabber : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visualizationRoot;
    
    [Header("Settings")]
    [SerializeField] private float grabDistance = 5f;
    
    private bool isGrabbing = false;
    private Vector3 grabOffset;
    private Transform controllerTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    private void Start()
    {
        controllerTransform = transform;
        
        // Find visualization root if not assigned
        if (visualizationRoot == null)
        {
            CoordinateSystem coordSystem = FindFirstObjectByType<CoordinateSystem>();
            if (coordSystem != null)
            {
                visualizationRoot = coordSystem.transform;
            }
        }
        
        // Store original transform
        if (visualizationRoot != null)
        {
            originalPosition = visualizationRoot.position;
            originalRotation = visualizationRoot.rotation;
        }
    }
    
    private void Update()
    {
        CheckGrabInput();
        
        if (isGrabbing)
        {
            UpdateGrab();
        }
    }
    
    private void CheckGrabInput()
    {
        bool gripPressed = false;
        
        // Check VR grip
        UnityEngine.XR.InputDevice controller = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
        if (controller.isValid)
        {
            controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripPressed);
        }
        
        // Also check keyboard for simulator (G key)
        if (Keyboard.current != null && Keyboard.current.gKey.isPressed)
        {
            gripPressed = true;
        }
        
        // Start grab
        if (gripPressed && !isGrabbing)
        {
            TryStartGrab();
        }
        
        // End grab
        if (!gripPressed && isGrabbing)
        {
            EndGrab();
        }
    }
    
    private void TryStartGrab()
{
    if (visualizationRoot == null) return;
    
    isGrabbing = true;
    grabOffset = visualizationRoot.position - controllerTransform.position;
    Debug.Log("Grab started");
}

private void UpdateGrab()
{
    if (visualizationRoot == null)
    {
        Debug.LogWarning("No visualization root!");
        return;
    }
    
    Vector3 newPosition = controllerTransform.position + grabOffset;
    Debug.Log($"Moving to: {newPosition}");
    visualizationRoot.position = newPosition;
}
    
    private void EndGrab()
    {
        isGrabbing = false;
        Debug.Log("Grab ended");
    }
    
    public void ResetPosition()
    {
        if (visualizationRoot != null)
        {
            visualizationRoot.position = originalPosition;
            visualizationRoot.rotation = originalRotation;
            Debug.Log("Position reset");
        }
    }
}