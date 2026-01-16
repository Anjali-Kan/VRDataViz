using UnityEngine;
using UnityEngine.InputSystem;

public class WebControls : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSpeed = 2f;
    
    private Transform cameraTransform;
    private float rotationX = 0f;
    private float rotationY = 0f;
    
    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }
    
    private void Update()
    {
        // Only use these controls if not in VR
        if (IsInVR()) return;
        
        HandleMovement();
        HandleLook();
    }
    
    private bool IsInVR()
    {
        return UnityEngine.XR.XRSettings.isDeviceActive;
    }
    
    private void HandleMovement()
    {
        if (Keyboard.current == null) return;
        
        Vector3 move = Vector3.zero;
        
        if (Keyboard.current.wKey.isPressed) move += cameraTransform.forward;
        if (Keyboard.current.sKey.isPressed) move -= cameraTransform.forward;
        if (Keyboard.current.aKey.isPressed) move -= cameraTransform.right;
        if (Keyboard.current.dKey.isPressed) move += cameraTransform.right;
        if (Keyboard.current.qKey.isPressed) move -= Vector3.up;
        if (Keyboard.current.eKey.isPressed) move += Vector3.up;
        
        move.Normalize();
        cameraTransform.position += move * moveSpeed * Time.deltaTime;
    }
    
    private void HandleLook()
    {
        if (Mouse.current == null) return;
        
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            
            rotationY += delta.x * lookSpeed * 0.1f;
            rotationX -= delta.y * lookSpeed * 0.1f;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);
            
            cameraTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
    }
}