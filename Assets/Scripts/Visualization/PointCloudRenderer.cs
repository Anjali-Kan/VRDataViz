using UnityEngine;
using System.Collections.Generic;


public class PointCloudRenderer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pointSize = 0.1f;
    [SerializeField] private float axisLength = 5f;
    
    [Header("Column Mapping")]
    [SerializeField] private int xColumnIndex = 1;
    [SerializeField] private int yColumnIndex = 2;
    [SerializeField] private int zColumnIndex = 3;
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
    
    private void Start()
    {
        SetupDefaultGradient();
        
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
        dataSet = DataManager.Instance.CurrentDataSet;
        GeneratePoints();
    }
    
    public void GeneratePoints()
    {
        if (dataSet == null || !dataSet.IsLoaded)
        {
            Debug.LogWarning("No data loaded");
            return;
        }
        
        // Clear old points
        foreach (GameObject obj in pointObjects)
        {
            Destroy(obj);
        }
        pointObjects.Clear();
        
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
        
        Debug.Log($"Generated {totalPoints} colored points");
    }
    
    private Color[] GetColorsForColumn(int columnIndex)
    {
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
        xColumnIndex = x;
        yColumnIndex = y;
        zColumnIndex = z;
        GeneratePoints();
    }
    
    public void SetColorColumn(int columnIndex)
    {
        colorColumnIndex = columnIndex;
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
}