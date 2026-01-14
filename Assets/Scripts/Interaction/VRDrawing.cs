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
    private Transform visualizationRoot;
    
    private void Start()
    {
        if (drawPoint == null)
        {
            drawPoint = transform;
        }

            // Cache visualization root
        CoordinateSystem coordSystem = FindFirstObjectByType<CoordinateSystem>();
        if (coordSystem != null)
        {
            visualizationRoot = coordSystem.transform;
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
    
    // Get fresh reference to visualization root
    CoordinateSystem coordSystem = FindFirstObjectByType<CoordinateSystem>();
    if (coordSystem != null)
    {
        visualizationRoot = coordSystem.transform;
    }
    
    // Create new line as child of visualization
    GameObject lineObj = new GameObject("Drawing_" + drawingCount);
    drawingCount++;
    
    if (visualizationRoot != null)
    {
        lineObj.transform.SetParent(visualizationRoot);
        lineObj.transform.localPosition = Vector3.zero;
        lineObj.transform.localRotation = Quaternion.identity;
    }
    
    currentLine = lineObj.AddComponent<LineRenderer>();
    currentLine.startWidth = lineWidth;
    currentLine.endWidth = lineWidth;
    currentLine.material = new Material(Shader.Find("Sprites/Default"));
    currentLine.startColor = drawColor;
    currentLine.endColor = drawColor;
    currentLine.positionCount = 0;
    currentLine.useWorldSpace = false;
    
    currentPositions.Clear();
    allDrawings.Add(lineObj);
    
    Debug.Log("Drawing started");
}
    
    private void UpdateDrawing()
{
    if (currentLine == null) return;
    
    // Get the parent transform (should be CoordinateSystem)
    Transform parent = currentLine.transform.parent;
    
    if (parent == null)
    {
        // Fallback - find CoordinateSystem
        CoordinateSystem coordSystem = FindFirstObjectByType<CoordinateSystem>();
        if (coordSystem != null)
        {
            currentLine.transform.SetParent(coordSystem.transform);
            parent = coordSystem.transform;
        }
    }
    
    // Convert controller world position to local space of parent
    Vector3 localPos;
    if (parent != null)
    {
        localPos = parent.InverseTransformPoint(drawPoint.position);
    }
    else
    {
        localPos = drawPoint.position;
    }
    
    // Only add point if moved enough
    if (currentPositions.Count == 0 || Vector3.Distance(localPos, currentPositions[currentPositions.Count - 1]) > minDistance)
    {
        currentPositions.Add(localPos);
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

    public List<List<Vector3>> GetAllDrawingsData()
{
    List<List<Vector3>> data = new List<List<Vector3>>();
    
    foreach (GameObject drawing in allDrawings)
    {
        if (drawing == null) continue;
        
        LineRenderer lr = drawing.GetComponent<LineRenderer>();
        if (lr != null)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);
            points.AddRange(positions);
            data.Add(points);
        }
    }
    
    return data;
}

public void LoadDrawings(List<List<Vector3>> data)
{
    ClearAllDrawings();
    
    CoordinateSystem coordSystem = FindFirstObjectByType<CoordinateSystem>();
    Transform parent = coordSystem != null ? coordSystem.transform : null;
    
    foreach (List<Vector3> points in data)
    {
        if (points.Count < 2) continue;
        
        GameObject lineObj = new GameObject("Drawing_" + drawingCount);
        drawingCount++;
        
        if (parent != null)
        {
            lineObj.transform.SetParent(parent);
        }
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = drawColor;
        lr.endColor = drawColor;
        lr.useWorldSpace = false;
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
        
        allDrawings.Add(lineObj);
    }
    
    Debug.Log("Loaded " + data.Count + " drawings");
}
}