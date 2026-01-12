using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AxisMappingUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PointCloudRenderer pointCloudRenderer;
    
    [Header("UI Settings")]
    [SerializeField] private Vector3 panelPosition = new Vector3(-2f, 1.5f, 2f);
    [SerializeField] private Vector3 panelRotation = new Vector3(0f, 45f, 0f);
    
    private TMP_Dropdown xAxisDropdown;
    private TMP_Dropdown yAxisDropdown;
    private TMP_Dropdown zAxisDropdown;
    private TMP_Dropdown colorDropdown;
    
    private DataSet dataSet;
    private List<int> numericIndices = new List<int>();
    private bool isInitialized = false;
    
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
        canvasObj.AddComponent<GraphicRaycaster>();
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 350);
        canvasRect.position = panelPosition;
        canvasRect.eulerAngles = panelRotation;
        canvasRect.localScale = Vector3.one * 0.005f;
        
        // Create Panel
        GameObject panelObj = CreatePanel(canvasObj.transform);
        
        // Create Title
        CreateLabel(panelObj.transform, "Axis Mapping", new Vector2(0, 130), 28);
        
        // Create Dropdowns with Labels
        xAxisDropdown = CreateDropdownWithLabel(panelObj.transform, "X Axis:", new Vector2(0, 70));
        yAxisDropdown = CreateDropdownWithLabel(panelObj.transform, "Y Axis:", new Vector2(0, 20));
        zAxisDropdown = CreateDropdownWithLabel(panelObj.transform, "Z Axis:", new Vector2(0, -30));
        colorDropdown = CreateDropdownWithLabel(panelObj.transform, "Color:", new Vector2(0, -80));
    }
    
    private GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(parent, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        return panel;
    }
    
    private TextMeshProUGUI CreateLabel(Transform parent, string text, Vector2 position, int fontSize = 20)
    {
        GameObject labelObj = new GameObject("Label_" + text);
        labelObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        RectTransform rect = labelObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(380, 30);
        rect.anchoredPosition = position;
        
        return tmp;
    }
    
    private TMP_Dropdown CreateDropdownWithLabel(Transform parent, string labelText, Vector2 position)
    {
        // Label
        GameObject labelObj = new GameObject("Label_" + labelText);
        labelObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 18;
        label.alignment = TextAlignmentOptions.Left;
        label.color = Color.white;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(80, 30);
        labelRect.anchoredPosition = position + new Vector2(-140, 0);
        
        // Dropdown
        GameObject dropdownObj = new GameObject("Dropdown_" + labelText);
        dropdownObj.transform.SetParent(parent, false);
        
        Image dropdownImage = dropdownObj.AddComponent<Image>();
        dropdownImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        
        RectTransform dropdownRect = dropdownObj.GetComponent<RectTransform>();
        dropdownRect.sizeDelta = new Vector2(200, 30);
        dropdownRect.anchoredPosition = position + new Vector2(50, 0);
        
        // Dropdown Label (selected value)
        GameObject captionObj = new GameObject("Caption");
        captionObj.transform.SetParent(dropdownObj.transform, false);
        
        TextMeshProUGUI captionText = captionObj.AddComponent<TextMeshProUGUI>();
        captionText.fontSize = 16;
        captionText.alignment = TextAlignmentOptions.Left;
        captionText.color = Color.white;
        
        RectTransform captionRect = captionObj.GetComponent<RectTransform>();
        captionRect.anchorMin = Vector2.zero;
        captionRect.anchorMax = Vector2.one;
        captionRect.offsetMin = new Vector2(10, 0);
        captionRect.offsetMax = new Vector2(-10, 0);
        
        dropdown.captionText = captionText;
        
        // Template (dropdown list)
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        templateObj.SetActive(false);
        
        Image templateImage = templateObj.AddComponent<Image>();
        templateImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        RectTransform templateRect = templateObj.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.sizeDelta = new Vector2(0, 150);
        templateRect.anchoredPosition = Vector2.zero;
        
        ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = Color.white;
        viewportObj.AddComponent<Mask>().showMaskGraphic = false;
        
        RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0, 30);
        contentRect.anchoredPosition = Vector2.zero;
        
        // Item Template
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);
        
        Image itemImage = itemObj.AddComponent<Image>();
        itemImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        
        Toggle itemToggle = itemObj.AddComponent<Toggle>();
        
        RectTransform itemRect = itemObj.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 30);
        
        // Item Label
        GameObject itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemObj.transform, false);
        
        TextMeshProUGUI itemLabel = itemLabelObj.AddComponent<TextMeshProUGUI>();
        itemLabel.fontSize = 16;
        itemLabel.alignment = TextAlignmentOptions.Left;
        itemLabel.color = Color.white;
        
        RectTransform itemLabelRect = itemLabelObj.GetComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(10, 0);
        itemLabelRect.offsetMax = new Vector2(-10, 0);
        
        // Connect everything
        dropdown.template = templateRect;
        dropdown.itemText = itemLabel;
        itemToggle.targetGraphic = itemImage;
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        
        return dropdown;
    }
    
    private void OnDataLoaded()
    {
        dataSet = DataManager.Instance.CurrentDataSet;
        PopulateDropdowns();
    }
    
    private void PopulateDropdowns()
    {
        if (dataSet == null) return;
        
        List<string> numericColumns = new List<string>();
        List<string> allColumns = new List<string>();
        numericIndices.Clear();
        
        for (int i = 0; i < dataSet.Columns.Count; i++)
        {
            ColumnInfo col = dataSet.Columns[i];
            allColumns.Add(col.Name);
            
            if (col.IsNumeric)
            {
                numericColumns.Add(col.Name);
                numericIndices.Add(i);
            }
        }
        
        // Populate dropdowns
        SetupDropdown(xAxisDropdown, numericColumns, 0);
        SetupDropdown(yAxisDropdown, numericColumns, Mathf.Min(1, numericColumns.Count - 1));
        SetupDropdown(zAxisDropdown, numericColumns, Mathf.Min(2, numericColumns.Count - 1));
        SetupDropdown(colorDropdown, allColumns, allColumns.Count - 1);
        
        // Add listeners
        xAxisDropdown.onValueChanged.AddListener(OnAxisChanged);
        yAxisDropdown.onValueChanged.AddListener(OnAxisChanged);
        zAxisDropdown.onValueChanged.AddListener(OnAxisChanged);
        colorDropdown.onValueChanged.AddListener(OnColorChanged);
        
        isInitialized = true;
        ApplyMapping();
    }
    
    private void SetupDropdown(TMP_Dropdown dropdown, List<string> options, int defaultIndex)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = defaultIndex;
        dropdown.RefreshShownValue();
    }
    
    private void OnAxisChanged(int value)
    {
        if (!isInitialized) return;
        ApplyMapping();
    }
    
    private void OnColorChanged(int value)
    {
        if (!isInitialized) return;
        ApplyMapping();
    }
    
    private void ApplyMapping()
    {
        if (pointCloudRenderer == null)
        {
            pointCloudRenderer = FindObjectOfType<PointCloudRenderer>();
        }
        
        if (pointCloudRenderer == null || numericIndices.Count == 0) return;
        
        int xIndex = numericIndices[xAxisDropdown.value];
        int yIndex = numericIndices[yAxisDropdown.value];
        int zIndex = numericIndices[zAxisDropdown.value];
        int colorIndex = colorDropdown.value;
        
        pointCloudRenderer.SetAxisMapping(xIndex, yIndex, zIndex);
        pointCloudRenderer.SetColorColumn(colorIndex);
        
        Debug.Log($"Mapping: X={dataSet.Columns[xIndex].Name}, Y={dataSet.Columns[yIndex].Name}, Z={dataSet.Columns[zIndex].Name}, Color={dataSet.Columns[colorIndex].Name}");
    }
}