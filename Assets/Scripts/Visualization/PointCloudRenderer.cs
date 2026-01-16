using UnityEngine;
using System.Collections.Generic;


public class PointCloudRenderer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pointSize = 0.1f;
    [SerializeField] private float axisLength = 5f;
    
    [Header("Column Mapping")]
    [SerializeField] private int xColumnIndex = 2;
    [SerializeField] private int yColumnIndex = 3;
    [SerializeField] private int zColumnIndex = 0;
    [SerializeField] private int colorColumnIndex = 4;
    
    [Header("Colors")]
    [SerializeField] private Gradient numericGradient;
    [SerializeField] private Color[] categoryColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.magenta
    };
    
    private List<GameObject> pointObjects = new List<GameObject>();
    private DataSet dataSet;
    private bool isSubscribed = false;
    
    private void Start()
    {
        SetupDefaultGradient();
        
        // Wait a frame to ensure DataManager.Instance is set (Awake runs before Start, but be defensive)
        StartCoroutine(InitializeAfterFrame());
    }
    
    private System.Collections.IEnumerator InitializeAfterFrame()
    {
        yield return null; // Wait one frame
        
        SubscribeToDataManager();
        
        // If data is already loaded, visualize it
        if (DataManager.Instance != null && DataManager.Instance.IsDataLoaded)
        {
            Debug.Log("[PointCloudRenderer] Data already loaded on Start, visualizing immediately");
            OnDataLoaded();
        }
        else if (DataManager.Instance == null)
        {
            Debug.LogError("[PointCloudRenderer] DataManager.Instance is null after frame wait!");
        }
    }
    
    private void OnEnable()
    {
        // Ensure subscription if component gets re-enabled
        SubscribeToDataManager();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromDataManager();
    }
    
    private void SubscribeToDataManager()
    {
        if (DataManager.Instance != null && !isSubscribed)
        {
            DataManager.Instance.OnDataLoaded += OnDataLoaded;
            isSubscribed = true;
            Debug.Log("[PointCloudRenderer] Subscribed to OnDataLoaded event");
        }
    }
    
    private void UnsubscribeFromDataManager()
    {
        if (DataManager.Instance != null && isSubscribed)
        {
            DataManager.Instance.OnDataLoaded -= OnDataLoaded;
            isSubscribed = false;
            Debug.Log("[PointCloudRenderer] Unsubscribed from OnDataLoaded event");
        }
    }
    
    private void OnDestroy()
    {
        if (DataManager.Instance != null && isSubscribed)
        {
            DataManager.Instance.OnDataLoaded -= OnDataLoaded;
            isSubscribed = false;
        }
        
        // Clean up all point objects
        ClearAllPoints();
    }
    
    private void SetupDefaultGradient()
    {
        numericGradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(Color.blue, 0f);
        colorKeys[1] = new GradientColorKey(Color.green, 0.5f);
        colorKeys[2] = new GradientColorKey(Color.red, 1f);
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);
        
        numericGradient.SetKeys(colorKeys, alphaKeys);
    }
    
    private void OnDataLoaded()
    {
        Debug.Log("[PointCloudRenderer] OnDataLoaded event received");
        dataSet = DataManager.Instance?.CurrentDataSet;
        
        if (dataSet == null)
        {
            Debug.LogWarning("[PointCloudRenderer] CurrentDataSet is null");
            return;
        }
        
        // Validate and reset column indices for new dataset
        ValidateColumnIndices();
        
        // Wait a frame to allow AxisMappingUI to apply its mapping first
        // This ensures consistent visualization regardless of load order
        // AxisMappingUI will call SetAxisMapping/SetColorColumn which triggers GeneratePoints
        // So we delay to avoid double-rendering with different indices
        StartCoroutine(DelayedGeneratePoints());
    }
    
    private System.Collections.IEnumerator DelayedGeneratePoints()
    {
        yield return null; // Wait one frame for AxisMappingUI to apply mapping via ApplyMapping()
        
        // Only generate if AxisMappingUI hasn't already triggered it
        // (AxisMappingUI calls SetAxisMapping/SetColorColumn which calls GeneratePoints)
        // Check if we're still on the same dataset to avoid race conditions
        if (dataSet != null && dataSet == DataManager.Instance?.CurrentDataSet)
        {
            Debug.Log($"[PointCloudRenderer] Rebuilding visualization with {dataSet.Parser.RowCount} rows (after AxisMappingUI delay)");
            GeneratePoints();
        }
    }
    
    private void ValidateColumnIndices()
    {
        if (dataSet == null || dataSet.Columns == null || dataSet.Columns.Count == 0)
        {
            Debug.LogWarning("[PointCloudRenderer] Cannot validate indices - no columns available");
            return;
        }
        
        int columnCount = dataSet.Columns.Count;
        bool indicesChanged = false;
        
        // Clamp indices to valid range
        if (xColumnIndex < 0 || xColumnIndex >= columnCount)
        {
            Debug.LogWarning($"[PointCloudRenderer] xColumnIndex {xColumnIndex} out of range (0-{columnCount-1}), resetting to 0");
            xColumnIndex = 0;
            indicesChanged = true;
        }
        
        if (yColumnIndex < 0 || yColumnIndex >= columnCount)
        {
            Debug.LogWarning($"[PointCloudRenderer] yColumnIndex {yColumnIndex} out of range (0-{columnCount-1}), resetting to {Mathf.Min(1, columnCount-1)}");
            yColumnIndex = Mathf.Min(1, columnCount - 1);
            indicesChanged = true;
        }
        
        if (zColumnIndex < 0 || zColumnIndex >= columnCount)
        {
            Debug.LogWarning($"[PointCloudRenderer] zColumnIndex {zColumnIndex} out of range (0-{columnCount-1}), resetting to {Mathf.Min(2, columnCount-1)}");
            zColumnIndex = Mathf.Min(2, columnCount - 1);
            indicesChanged = true;
        }
        
        if (colorColumnIndex < 0 || colorColumnIndex >= columnCount)
        {
            Debug.LogWarning($"[PointCloudRenderer] colorColumnIndex {colorColumnIndex} out of range (0-{columnCount-1}), resetting to {columnCount-1}");
            colorColumnIndex = columnCount - 1;
            indicesChanged = true;
        }
        
        if (indicesChanged)
        {
            Debug.Log($"[PointCloudRenderer] Column indices validated: X={xColumnIndex}, Y={yColumnIndex}, Z={zColumnIndex}, Color={colorColumnIndex}");
        }
    }
    
    private void ClearAllPoints()
    {
        foreach (GameObject obj in pointObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        pointObjects.Clear();
    }
    
    public void GeneratePoints()
    {
        if (dataSet == null || !dataSet.IsLoaded)
        {
            Debug.LogWarning("[PointCloudRenderer] No data loaded - cannot generate points");
            return;
        }
        
        // Validate indices before using them
        ValidateColumnIndices();
        
        Debug.Log($"[PointCloudRenderer] Clearing {pointObjects.Count} old point objects");
        // Clear old points
        ClearAllPoints();
        
        // Get normalized positions
        float[] xValues = dataSet.GetNormalizedColumn(xColumnIndex, 0, axisLength);
        float[] yValues = dataSet.GetNormalizedColumn(yColumnIndex, 0, axisLength);
        float[] zValues = dataSet.GetNormalizedColumn(zColumnIndex, 0, axisLength);
        
        // Get colors
        Color[] colors = GetColorsForColumn(colorColumnIndex);
        
        int totalPoints = dataSet.Parser.RowCount;
        
        for (int i = 0; i < totalPoints; i++)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.name = $"Point_{i}";
            point.transform.SetParent(transform);
            
            point.transform.localPosition = new Vector3(
                xValues[i],
                yValues[i],
                zValues[i]
            );
            
            point.transform.localScale = Vector3.one * pointSize;
            
            
            
            // Set color
            Renderer renderer = point.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = colors[i];
            
            pointObjects.Add(point);
        }
        
        Debug.Log($"[PointCloudRenderer] Generated {totalPoints} colored points");
    }
    
    private Color[] GetColorsForColumn(int columnIndex)
    {
        if (dataSet == null || dataSet.Columns == null)
        {
            Debug.LogError("[PointCloudRenderer] GetColorsForColumn: dataSet or Columns is null");
            return new Color[0];
        }
        
        if (columnIndex < 0 || columnIndex >= dataSet.Columns.Count)
        {
            Debug.LogError($"[PointCloudRenderer] GetColorsForColumn: columnIndex {columnIndex} out of range (0-{dataSet.Columns.Count-1})");
            // Return default color array
            int defaultRowCount = dataSet.Parser?.RowCount ?? 0;
            Color[] defaultColors = new Color[defaultRowCount];
            for (int i = 0; i < defaultRowCount; i++)
            {
                defaultColors[i] = Color.white;
            }
            return defaultColors;
        }
        
        int rowCount = dataSet.Parser.RowCount;
        Color[] colors = new Color[rowCount];
        
        ColumnInfo colInfo = dataSet.Columns[columnIndex];
        
        if (colInfo.IsNumeric)
        {
            float[] normalized = dataSet.GetNormalizedColumn(columnIndex, 0, 1);
            
            for (int i = 0; i < rowCount; i++)
            {
                colors[i] = numericGradient.Evaluate(normalized[i]);
            }
        }
        else
        {
            for (int i = 0; i < rowCount; i++)
            {
                int catIndex = dataSet.GetCategoryIndex(columnIndex, i);
                colors[i] = categoryColors[catIndex % categoryColors.Length];
            }
        }
        
        return colors;
    }
    
    public void SetAxisMapping(int x, int y, int z)
    {
        if (dataSet == null || dataSet.Columns == null)
        {
            Debug.LogWarning("[PointCloudRenderer] SetAxisMapping: No dataset loaded, ignoring");
            return;
        }
        
        int columnCount = dataSet.Columns.Count;
        
        // Validate and clamp indices
        xColumnIndex = Mathf.Clamp(x, 0, columnCount - 1);
        yColumnIndex = Mathf.Clamp(y, 0, columnCount - 1);
        zColumnIndex = Mathf.Clamp(z, 0, columnCount - 1);
        
        if (x != xColumnIndex || y != yColumnIndex || z != zColumnIndex)
        {
            Debug.LogWarning($"[PointCloudRenderer] SetAxisMapping: Indices clamped to valid range (0-{columnCount-1})");
        }
        
        GeneratePoints();
    }
    
    public void SetColorColumn(int columnIndex)
    {
        if (dataSet == null || dataSet.Columns == null)
        {
            Debug.LogWarning("[PointCloudRenderer] SetColorColumn: No dataset loaded, ignoring");
            return;
        }
        
        int columnCount = dataSet.Columns.Count;
        
        // Validate and clamp index
        int oldIndex = colorColumnIndex;
        colorColumnIndex = Mathf.Clamp(columnIndex, 0, columnCount - 1);
        
        if (columnIndex != colorColumnIndex)
        {
            Debug.LogWarning($"[PointCloudRenderer] SetColorColumn: Index {columnIndex} out of range (0-{columnCount-1}), clamped to {colorColumnIndex}");
        }
        
        // Only regenerate if color column actually changed
        if (oldIndex != colorColumnIndex)
        {
            GeneratePoints();
        }
    }
    
    // Method to set both axis mapping and color column at once (used by AxisMappingUI to avoid double-rendering)
    public void SetMappingAndColor(int x, int y, int z, int color)
    {
        if (dataSet == null || dataSet.Columns == null)
        {
            Debug.LogWarning("[PointCloudRenderer] SetMappingAndColor: No dataset loaded, ignoring");
            return;
        }
        
        int columnCount = dataSet.Columns.Count;
        
        // Validate and clamp all indices
        xColumnIndex = Mathf.Clamp(x, 0, columnCount - 1);
        yColumnIndex = Mathf.Clamp(y, 0, columnCount - 1);
        zColumnIndex = Mathf.Clamp(z, 0, columnCount - 1);
        colorColumnIndex = Mathf.Clamp(color, 0, columnCount - 1);
        
        // Generate once with all new indices
        GeneratePoints();
    }

    public void ApplyFilters(Dictionary<int, float> minFilters, Dictionary<int, float> maxFilters)
{
    if (dataSet == null) return;
    
    for (int i = 0; i < pointObjects.Count; i++)
    {
        bool visible = true;
        
        foreach (var filter in minFilters)
        {
            int colIndex = filter.Key;
            float minVal = filter.Value;
            float maxVal = maxFilters[colIndex];
            
            if (dataSet.Columns[colIndex].IsNumeric)
            {
                float value = float.Parse(dataSet.Parser.Rows[i][colIndex]);
                
                if (value < minVal || value > maxVal)
                {
                    visible = false;
                    break;
                }
            }
        }
        
        pointObjects[i].SetActive(visible);
    }
    
    Debug.Log("Filters applied to points");
}

public void ClearFilters()
{
    foreach (var point in pointObjects)
    {
        point.SetActive(true);
    }
    
    Debug.Log("Filters cleared");
}

public int GetXColumn() { return xColumnIndex; }
public int GetYColumn() { return yColumnIndex; }
public int GetZColumn() { return zColumnIndex; }
public int GetColorColumn() { return colorColumnIndex; }

}