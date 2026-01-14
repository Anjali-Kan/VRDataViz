using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class VRDrawing : MonoBehaviour
{
    [Header("Drawing Settings")]
    [SerializeField] private Color drawColor = Color.yellow;
    [SerializeField] private float lineWidth = 0.01f;
    [SerializeField] private float minDistance = 0.01f;
    
    [Header("References")]
    [SerializeField] private Transform drawPoint;
    
    private LineRenderer currentLine;
    private List<Vector3> currentPositions = new List<Vector3>();
    private List<GameObject> allDrawings = new List<GameObject>();
    private bool isDrawing = false;
    private int drawingCount = 0;
    
    private void Start()
    {
        if (drawPoint == null)
        {
            drawPoint = transform;
        }
    }
    
    private void Update()
    {
        CheckDrawInput();
        
        if (isDrawing)
        {
            UpdateDrawing();
        }
    }
    
    private void CheckDrawInput()
    {
        bool triggerPressed = false;
        
        // Check VR trigger
        UnityEngine.XR.InputDevice controller = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
        if (controller.isValid)
        {
            controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);
        }
        
        // Check keyboard for simulator (F key for draw)
        if (Keyboard.current != null && Keyboard.current.fKey.isPressed)
        {
            triggerPressed = true;
        }
        
        // Start drawing
        if (triggerPressed && !isDrawing)
        {
            StartDrawing();
        }
        
        // Stop drawing
        if (!triggerPressed && isDrawing)
        {
            StopDrawing();
        }
    }
    
    private void StartDrawing()
    {
        isDrawing = true;
        
        // Create new line
        GameObject lineObj = new GameObject("Drawing_" + drawingCount);

        // Parent to CoordinateSystem so it moves with visualization
        CoordinateSystem coordSystem = FindFirstObjectByType<CoordinateSystem>();
        if (coordSystem != null)
        {
            lineObj.transform.SetParent(coordSystem.transform);
        }
        drawingCount++;
        
        currentLine = lineObj.AddComponent<LineRenderer>();
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.material = new Material(Shader.Find("Sprites/Default"));
        currentLine.startColor = drawColor;
        currentLine.endColor = drawColor;
        currentLine.positionCount = 0;
        currentLine.useWorldSpace = true;
        
        currentPositions.Clear();
        allDrawings.Add(lineObj);
        
        Debug.Log("Drawing started");
    }
    
    private void UpdateDrawing()
    {
        if (currentLine == null) return;
        
        Vector3 currentPos = drawPoint.position;
        
        // Only add point if moved enough
        if (currentPositions.Count == 0 || Vector3.Distance(currentPos, currentPositions[currentPositions.Count - 1]) > minDistance)
        {
            currentPositions.Add(currentPos);
            currentLine.positionCount = currentPositions.Count;
            currentLine.SetPositions(currentPositions.ToArray());
        }
    }
    
    private void StopDrawing()
    {
        isDrawing = false;
        currentLine = null;
        Debug.Log("Drawing stopped. Points: " + currentPositions.Count);
    }
    
    public void ClearAllDrawings()
    {
        foreach (GameObject drawing in allDrawings)
        {
            if (drawing != null)
            {
                Destroy(drawing);
            }
        }
        
        allDrawings.Clear();
        drawingCount = 0;
        Debug.Log("All drawings cleared");
    }
    
    public void SetColor(Color color)
    {
        drawColor = color;
    }
    
    public void UndoLastDrawing()
    {
        if (allDrawings.Count > 0)
        {
            GameObject last = allDrawings[allDrawings.Count - 1];
            allDrawings.RemoveAt(allDrawings.Count - 1);
            
            if (last != null)
            {
                Destroy(last);
            }
            
            Debug.Log("Undo drawing");
        }
    }
}