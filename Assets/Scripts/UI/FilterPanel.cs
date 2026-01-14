using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class FilterPanel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 panelPosition = new Vector3(2f, 1.5f, 2f);
    [SerializeField] private Vector3 panelRotation = new Vector3(0f, -45f, 0f);
    
    private DataSet dataSet;
    private PointCloudRenderer pointCloudRenderer;
    
    private Dictionary<int, float> minFilters = new Dictionary<int, float>();
    private Dictionary<int, float> maxFilters = new Dictionary<int, float>();
    private Dictionary<int, TextMeshProUGUI> minLabels = new Dictionary<int, TextMeshProUGUI>();
    private Dictionary<int, TextMeshProUGUI> maxLabels = new Dictionary<int, TextMeshProUGUI>();
    
    private void Start()
    {
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
    
    private void OnDataLoaded()
    {
        dataSet = DataManager.Instance.CurrentDataSet;
        pointCloudRenderer = FindFirstObjectByType<PointCloudRenderer>();
        CreateUI();
    }
    
    private void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("FilterCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 600);
        canvasRect.position = panelPosition;
        canvasRect.eulerAngles = panelRotation;
        canvasRect.localScale = Vector3.one * 0.005f;
        
        BoxCollider collider = canvasObj.AddComponent<BoxCollider>();
        collider.size = new Vector3(400, 600, 10);
        
        // Create Panel
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Title
        CreateTitle(panelObj.transform, "Filters", new Vector2(0, 220));
        
        // Create filter rows for numeric columns
        float yPos = 160f;
        int count = 0;
        
        for (int i = 0; i < dataSet.Columns.Count && count < 4; i++)
        {
            ColumnInfo col = dataSet.Columns[i];
            
            if (col.IsNumeric)
            {
                CreateFilterRow(panelObj.transform, col, i, new Vector2(0, yPos));
                
                minFilters[i] = col.Min;
                maxFilters[i] = col.Max;
                
                yPos -= 90f;
                count++;
            }
        }
        
        // Apply button
        CreateButton(panelObj.transform, "Apply Filters", new Vector2(0, yPos - 20), OnApplyFilters);
        
        // Reset button
        CreateButton(panelObj.transform, "Reset Filters", new Vector2(0, yPos - 80), OnResetFilters);
    }
    
    private void CreateTitle(Transform parent, string text, Vector2 position)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI tmp = titleObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        
        RectTransform rect = titleObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(380, 40);
        rect.anchoredPosition = position;
    }
    
    private void CreateFilterRow(Transform parent, ColumnInfo col, int colIndex, Vector2 position)
    {
        // Container
        GameObject row = new GameObject("Filter_" + col.Name);
        row.transform.SetParent(parent, false);
        
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(380, 80);
        rowRect.anchoredPosition = position;
        
        // Column name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(row.transform, false);
        
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = col.Name;
        nameText.fontSize = 20;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.color = Color.cyan;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(380, 25);
        nameRect.anchoredPosition = new Vector2(0, 25);
        
        // Min row
        CreateValueRow(row.transform, "Min:", col.Min, colIndex, true, new Vector2(0, 0));
        
        // Max row
        CreateValueRow(row.transform, "Max:", col.Max, colIndex, false, new Vector2(0, -25));
    }
    
    private void CreateValueRow(Transform parent, string label, float value, int colIndex, bool isMin, Vector2 position)
    {
        GameObject rowObj = new GameObject(label);
        rowObj.transform.SetParent(parent, false);
        
        RectTransform rowRect = rowObj.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(380, 25);
        rowRect.anchoredPosition = position;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(rowObj.transform, false);
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 16;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.color = Color.white;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(50, 25);
        labelRect.anchoredPosition = new Vector2(-160, 0);
        
        // Decrease button
        CreateSmallButton(rowObj.transform, "-", new Vector2(-90, 0), () => AdjustFilter(colIndex, isMin, -1));
        
        // Value display
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(rowObj.transform, false);
        
        TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
        valueText.text = value.ToString("F1");
        valueText.fontSize = 16;
        valueText.alignment = TextAlignmentOptions.Center;
        valueText.color = Color.yellow;
        
        RectTransform valueRect = valueObj.GetComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(100, 25);
        valueRect.anchoredPosition = new Vector2(0, 0);
        
        // Store reference
        if (isMin)
        {
            minLabels[colIndex] = valueText;
        }
        else
        {
            maxLabels[colIndex] = valueText;
        }
        
        // Increase button
        CreateSmallButton(rowObj.transform, "+", new Vector2(90, 0), () => AdjustFilter(colIndex, isMin, 1));
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
        
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.pressedColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        btn.colors = colors;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(40, 25);
        btnRect.anchoredPosition = position;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = 20;
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
        btnImage.color = new Color(0.2f, 0.4f, 0.6f, 1f);
        
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(onClick);
        
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        colors.pressedColor = new Color(0.1f, 0.6f, 0.8f, 1f);
        btn.colors = colors;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 40);
        btnRect.anchoredPosition = position;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = 22;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void AdjustFilter(int colIndex, bool isMin, int direction)
    {
        ColumnInfo col = dataSet.Columns[colIndex];
        float step = (col.Max - col.Min) / 10f;
        
        if (isMin)
        {
            minFilters[colIndex] += step * direction;
            minFilters[colIndex] = Mathf.Clamp(minFilters[colIndex], col.Min, maxFilters[colIndex] - step);
            minLabels[colIndex].text = minFilters[colIndex].ToString("F1");
        }
        else
        {
            maxFilters[colIndex] += step * direction;
            maxFilters[colIndex] = Mathf.Clamp(maxFilters[colIndex], minFilters[colIndex] + step, col.Max);
            maxLabels[colIndex].text = maxFilters[colIndex].ToString("F1");
        }
    }
    
    private void OnApplyFilters()
    {
        if (pointCloudRenderer == null) return;
        
        pointCloudRenderer.ApplyFilters(minFilters, maxFilters);
        Debug.Log("Filters applied!");
    }
    
    private void OnResetFilters()
    {
        foreach (var col in dataSet.Columns)
        {
            if (col.IsNumeric)
            {
                minFilters[col.Index] = col.Min;
                maxFilters[col.Index] = col.Max;
                
                if (minLabels.ContainsKey(col.Index))
                    minLabels[col.Index].text = col.Min.ToString("F1");
                if (maxLabels.ContainsKey(col.Index))
                    maxLabels[col.Index].text = col.Max.ToString("F1");
            }
        }
        
        if (pointCloudRenderer != null)
        {
            pointCloudRenderer.ClearFilters();
        }
        
        Debug.Log("Filters reset!");
    }
}