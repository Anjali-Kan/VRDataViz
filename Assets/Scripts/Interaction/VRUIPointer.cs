using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class VRUIPointer : MonoBehaviour
{
    [SerializeField] private float rayLength = 10f;
    
    private LineRenderer lineRenderer;
    private GameObject currentHovered;
    private bool triggerPressed = false;
    private bool triggerWasPressed = false;
    
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.cyan;
    }
    
    private void Update()
    {
        UpdateInput();
        UpdateRaycast();
        UpdateLine();
    }
    
    private void UpdateInput()
    {
        triggerWasPressed = triggerPressed;
        
        // Check mouse for simulator
        if (Mouse.current != null)
        {
            triggerPressed = Mouse.current.leftButton.isPressed;
        }
        
        // Check VR trigger
        UnityEngine.XR.InputDevice controller = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
        if (controller.isValid)
        {
            bool vrTrigger = false;
            controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out vrTrigger);
            if (vrTrigger) triggerPressed = true;
        }
    }
    
    private void UpdateRaycast()
    {
        // Create pointer event
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        
        // Convert ray to screen point for UI raycasting
        Vector3 rayEnd = transform.position + transform.forward * rayLength;
        Camera cam = Camera.main;
        
        // Raycast against all canvases
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        bool hitUI = false;
        
        if (Physics.Raycast(ray, out hit, rayLength))
        {
            // Check all components in hit object and parents
            GameObject hitObj = hit.collider.gameObject;
            
            // Look for any UI element in children at hit point
            Canvas canvas = hitObj.GetComponent<Canvas>();
            if (canvas == null) canvas = hitObj.GetComponentInParent<Canvas>();
            
            if (canvas != null)
            {
                // Found canvas, now find specific UI element
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null) raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                
                // Create screen position from world hit point
                pointerData.position = cam.WorldToScreenPoint(hit.point);
                
                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(pointerData, results);
                
                if (results.Count > 0)
                {
                    GameObject uiElement = results[0].gameObject;
                    hitUI = true;
                    
                    // Handle hover
                    if (currentHovered != uiElement)
                    {
                        if (currentHovered != null)
                        {
                            ExecuteEvents.Execute(currentHovered, pointerData, ExecuteEvents.pointerExitHandler);
                        }
                        
                        currentHovered = uiElement;
                        ExecuteEvents.Execute(currentHovered, pointerData, ExecuteEvents.pointerEnterHandler);
                        Debug.Log("Hovering: " + uiElement.name);
                    }
                    
                    // Handle click
                    // Handle click
if (triggerPressed && !triggerWasPressed)
{
    Debug.Log("Click detected on: " + uiElement.name);
    
    // Execute pointer down, then up, then click (full click sequence)
    ExecuteEvents.Execute(uiElement, pointerData, ExecuteEvents.pointerDownHandler);
    ExecuteEvents.Execute(uiElement, pointerData, ExecuteEvents.pointerUpHandler);
    ExecuteEvents.Execute(uiElement, pointerData, ExecuteEvents.pointerClickHandler);
    
    // Try to find and interact with selectable
    Selectable selectable = uiElement.GetComponent<Selectable>();
    if (selectable == null) selectable = uiElement.GetComponentInParent<Selectable>();
    
    if (selectable != null)
    {
        selectable.Select();
        
        // Special handling for TMP_Dropdown
        TMPro.TMP_Dropdown dropdown = selectable as TMPro.TMP_Dropdown;
        if (dropdown != null)
        {
            dropdown.Show();
            Debug.Log("Opening dropdown: " + dropdown.name);
        }
        
        Debug.Log("Clicked selectable: " + selectable.gameObject.name);
    }
}
                }
            }
        }
        
        if (!hitUI && currentHovered != null)
        {
            ExecuteEvents.Execute(currentHovered, pointerData, ExecuteEvents.pointerExitHandler);
            currentHovered = null;
        }
    }
    
    private void UpdateLine()
    {
        if (lineRenderer == null) return;
        
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + transform.forward * rayLength);
        
        if (currentHovered != null)
        {
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
        }
        else
        {
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.cyan;
        }
    }
}