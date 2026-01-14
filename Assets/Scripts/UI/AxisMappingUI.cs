using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class AxisMappingUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PointCloudRenderer pointCloudRenderer;
    
    [Header("UI Settings")]
    [SerializeField] private Vector3 panelPosition = new Vector3(-2f, 1.5f, 2f);
    [SerializeField] private Vector3 panelRotation = new Vector3(0f, 45f, 0f);
    
    private DataSet dataSet;
    private List<int> numericIndices = new List<int>();
    
    private int xSelection = 0;
    private int ySelection = 1;
    private int zSelection = 2;
    private int colorSelection = 0;
    
    private TextMeshProUGUI xLabel;
    private TextMeshProUGUI yLabel;
    private TextMeshProUGUI zLabel;
    private TextMeshProUGUI colorLabel;
    
    private List<string> numericNames = new List<string>();
    private List<string> allNames = new List<string>();
    
    private void Start()
    {
        CreateUI();
        
        DataManager.Instance.OnDataLoaded += OnDataLoaded;
        
        if (DataManager.Instance.IsDataLoaded)
        {
            OnDataLoaded();
        }
    }
    
    private void OnDestroy()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnDataLoaded -= OnDataLoaded;
        }
    }
    
    private void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("AxisMappingCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(500, 1050);
        canvasRect.position = panelPosition;
        canvasRect.eulerAngles = panelRotation;
        canvasRect.localScale = Vector3.one * 0.005f;
        
        // Add collider for raycast
        BoxCollider collider = canvasObj.AddComponent<BoxCollider>();
        collider.size = new Vector3(500, 1050, 10);
        
        // Create Panel
        GameObject panelObj = CreatePanel(canvasObj.transform);
        
        // Create Title
        CreateTitle(panelObj.transform, "Axis Mapping", new Vector2(0, 420));
        
        // Create axis rows
        xLabel = CreateAxisRow(panelObj.transform, "X Axis:", new Vector2(0, 330), OnXPrev, OnXNext);
        yLabel = CreateAxisRow(panelObj.transform, "Y Axis:", new Vector2(0, 280), OnYPrev, OnYNext);
        zLabel = CreateAxisRow(panelObj.transform, "Z Axis:", new Vector2(0, 230), OnZPrev, OnZNext);
        colorLabel = CreateAxisRow(panelObj.transform, "Color:", new Vector2(0, 180), OnColorPrev, OnColorNext);
        
        
        // Create Apply button
CreateButton(panelObj.transform, "Apply", new Vector2(0, 80), OnApply);

// Create Reset button
CreateButton(panelObj.transform, "Reset View", new Vector2(0, 15), OnResetView);

// Create Clear Drawings button
CreateButton(panelObj.transform, "Clear Drawings", new Vector2(0, -30), OnClearDrawings);

// Create Undo Drawing button
CreateButton(panelObj.transform, "Undo Drawing", new Vector2(0, -85), OnUndoDrawing);

// Create Save button
CreateButton(panelObj.transform, "Save View", new Vector2(0, -140), OnSaveView);

// Create Load button
CreateButton(panelObj.transform, "Load View", new Vector2(0, -195), OnLoadView);

// Create AR Toggle button
CreateButton(panelObj.transform, "Toggle AR/VR", new Vector2(0, -250), OnToggleAR);

    }
    
    private GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(parent, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        return panel;
    }
    
    private void CreateTitle(Transform parent, string text, Vector2 position)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI tmp = titleObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        
        RectTransform rect = titleObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 50);
        rect.anchoredPosition = position;
    }
    
    private TextMeshProUGUI CreateAxisRow(Transform parent, string labelText, Vector2 position, UnityEngine.Events.UnityAction onPrev, UnityEngine.Events.UnityAction onNext)
    {
        // Container
        GameObject row = new GameObject("Row_" + labelText);
        row.transform.SetParent(parent, false);
        
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(450, 50);
        rowRect.anchoredPosition = position;
        
        // Label (left side)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform, false);
        
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 24;
        label.alignment = TextAlignmentOptions.Left;
        label.color = Color.white;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(100, 50);
        labelRect.anchoredPosition = new Vector2(-175, 0);
        
        // Prev button
        CreateSmallButton(row.transform, "<", new Vector2(-60, 0), onPrev);
        
        // Value display (center)
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(row.transform, false);
        
        TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
        valueText.text = "---";
        valueText.fontSize = 22;
        valueText.alignment = TextAlignmentOptions.Center;
        valueText.color = Color.cyan;
        
        RectTransform valueRect = valueObj.GetComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(150, 50);
        valueRect.anchoredPosition = new Vector2(50, 0);
        
        // Next button
        CreateSmallButton(row.transform, ">", new Vector2(160, 0), onNext);
        
        return valueText;
    }
    
    private void CreateSmallButton(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("Btn_" + text);
        btnObj.transform.SetParent(parent, false);
        
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(onClick);
        
        // Button colors
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.pressedColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        btn.colors = colors;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(50, 40);
        btnRect.anchoredPosition = position;
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = 28;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void CreateButton(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("Btn_" + text);
        btnObj.transform.SetParent(parent, false);
        
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.5f, 0.2f, 1f);
        
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(onClick);
        
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.5f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.8f, 0.1f, 1f);
        btn.colors = colors;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(150, 50);
        btnRect.anchoredPosition = position;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = 26;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void OnDataLoaded()
    {
        dataSet = DataManager.Instance.CurrentDataSet;
        
        numericIndices.Clear();
        numericNames.Clear();
        allNames.Clear();
        
        for (int i = 0; i < dataSet.Columns.Count; i++)
        {
            ColumnInfo col = dataSet.Columns[i];
            allNames.Add(col.Name);
            
            if (col.IsNumeric)
            {
                numericNames.Add(col.Name);
                numericIndices.Add(i);
            }
        }
        
        // Set defaults
        xSelection = 0;
        ySelection = Mathf.Min(1, numericNames.Count - 1);
        zSelection = Mathf.Min(2, numericNames.Count - 1);
        colorSelection = allNames.Count - 1;
        
        UpdateLabels();
        ApplyMapping();
    }
    
    private void UpdateLabels()
    {
        if (numericNames.Count > 0)
        {
            xLabel.text = numericNames[xSelection];
            yLabel.text = numericNames[ySelection];
            zLabel.text = numericNames[zSelection];
        }
        
        if (allNames.Count > 0)
        {
            colorLabel.text = allNames[colorSelection];
        }
    }
    
    private void OnXPrev() { xSelection = (xSelection - 1 + numericNames.Count) % numericNames.Count; UpdateLabels(); }
    private void OnXNext() { xSelection = (xSelection + 1) % numericNames.Count; UpdateLabels(); }
    private void OnYPrev() { ySelection = (ySelection - 1 + numericNames.Count) % numericNames.Count; UpdateLabels(); }
    private void OnYNext() { ySelection = (ySelection + 1) % numericNames.Count; UpdateLabels(); }
    private void OnZPrev() { zSelection = (zSelection - 1 + numericNames.Count) % numericNames.Count; UpdateLabels(); }
    private void OnZNext() { zSelection = (zSelection + 1) % numericNames.Count; UpdateLabels(); }
    private void OnColorPrev() { colorSelection = (colorSelection - 1 + allNames.Count) % allNames.Count; UpdateLabels(); }
    private void OnColorNext() { colorSelection = (colorSelection + 1) % allNames.Count; UpdateLabels(); }
    
    private void OnApply()
    {
        ApplyMapping();
        Debug.Log("Applied mapping!");
    }
    
    private void ApplyMapping()
    {
        if (pointCloudRenderer == null)
        {
            pointCloudRenderer = FindFirstObjectByType<PointCloudRenderer>();
        }
        
        if (pointCloudRenderer == null || numericIndices.Count == 0) return;
        
        int xIndex = numericIndices[xSelection];
        int yIndex = numericIndices[ySelection];
        int zIndex = numericIndices[zSelection];
        
        pointCloudRenderer.SetAxisMapping(xIndex, yIndex, zIndex);
        pointCloudRenderer.SetColorColumn(colorSelection);

        // Update axis labels
        CoordinateSystem coordSystem = FindFirstObjectByType<CoordinateSystem>();
        if (coordSystem != null)
        {
            coordSystem.UpdateAxisLabels(
                numericNames[xSelection],
                numericNames[ySelection],
                numericNames[zSelection]
            );
        }
        
        Debug.Log($"Mapping: X={numericNames[xSelection]}, Y={numericNames[ySelection]}, Z={numericNames[zSelection]}, Color={allNames[colorSelection]}");
    }

    private void OnResetView()
{
    VisualizationGrabber grabber = FindFirstObjectByType<VisualizationGrabber>();
    if (grabber != null)
    {
        grabber.ResetPosition();
    }
    Debug.Log("View reset!");
}

private void OnClearDrawings()
{
    VRDrawing drawing = FindFirstObjectByType<VRDrawing>();
    if (drawing != null)
    {
        drawing.ClearAllDrawings();
    }
    Debug.Log("Drawings cleared!");
}

private void OnUndoDrawing()
{
    VRDrawing drawing = FindFirstObjectByType<VRDrawing>();
    if (drawing != null)
    {
        drawing.UndoLastDrawing();
    }
    Debug.Log("Undo last drawing!");
}

private void OnSaveView()
{
    SaveLoadManager.Instance.SaveViewState();
    Debug.Log("View saved!");
}

private void OnLoadView()
{
    SaveLoadManager.Instance.LoadViewState();
    Debug.Log("View loaded!");
}

private void OnToggleAR()
{
    ARToggle arToggle = FindFirstObjectByType<ARToggle>();
    if (arToggle != null)
    {
        arToggle.ToggleARMode();
    }
    Debug.Log("AR/VR toggled!");
}
}