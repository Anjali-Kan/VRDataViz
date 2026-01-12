using UnityEngine;
using System.Collections.Generic;

public class PointCloudRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Mesh pointMesh;
    [SerializeField] private Material pointMaterial;
    
    [Header("Settings")]
    [SerializeField] private float pointSize = 0.05f;
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

    private List<Matrix4x4[]> batches = new List<Matrix4x4[]>();
    private List<MaterialPropertyBlock[]> propertyBlocks = new List<MaterialPropertyBlock[]>();
    private DataSet dataSet;
    private bool isRendering = false;
    
    private const int BATCH_SIZE = 1023; // Unity's instancing limit

    private void Start()
    {
        SetupDefaultGradient();
        CreateDefaultMesh();
        CreateDefaultMaterial();
        
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
        if (numericGradient == null)
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
    }
    
    private void CreateDefaultMesh()
    {
        if (pointMesh == null)
        {
            pointMesh = CreateSphereMesh(8, 8);
        }
    }


    private Mesh CreateSphereMesh(int latSegments, int lonSegments)
    {
        Mesh mesh = new Mesh();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        for (int lat = 0; lat <= latSegments; lat++)
        {
            float theta = lat * Mathf.PI / latSegments;
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);
            
            for (int lon = 0; lon <= lonSegments; lon++)
            {
                float phi = lon * 2f * Mathf.PI / lonSegments;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);
                
                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;
                
                vertices.Add(new Vector3(x, y, z) * 0.5f);
            }
        }
        
        for (int lat = 0; lat < latSegments; lat++)
        {
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int first = lat * (lonSegments + 1) + lon;
                int second = first + lonSegments + 1;
                
                triangles.Add(first);
                triangles.Add(second);
                triangles.Add(first + 1);
                
                triangles.Add(second);
                triangles.Add(second + 1);
                triangles.Add(first + 1);
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        
        return mesh;
    }

private void CreateDefaultMaterial()
    {
        if (pointMaterial == null)
        {
            pointMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pointMaterial.enableInstancing = true;
        }
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
        
        batches.Clear();
        propertyBlocks.Clear();
        
        // Get normalized positions
        float[] xValues = dataSet.GetNormalizedColumn(xColumnIndex, 0, axisLength);
        float[] yValues = dataSet.GetNormalizedColumn(yColumnIndex, 0, axisLength);
        float[] zValues = dataSet.GetNormalizedColumn(zColumnIndex, 0, axisLength);
        
        // Get colors
        Color[] colors = GetColorsForColumn(colorColumnIndex);
        
        int totalPoints = dataSet.Parser.RowCount;
        int batchCount = Mathf.CeilToInt((float)totalPoints / BATCH_SIZE);
        
        for (int b = 0; b < batchCount; b++)
        {
            int startIndex = b * BATCH_SIZE;
            int endIndex = Mathf.Min(startIndex + BATCH_SIZE, totalPoints);
            int batchSize = endIndex - startIndex;
            
            Matrix4x4[] matrices = new Matrix4x4[batchSize];
            MaterialPropertyBlock[] props = new MaterialPropertyBlock[batchSize];
            
            for (int i = 0; i < batchSize; i++)
            {
                int dataIndex = startIndex + i;
                
                Vector3 position = new Vector3(
                    xValues[dataIndex],
                    yValues[dataIndex],
                    zValues[dataIndex]
                );
                
                matrices[i] = Matrix4x4.TRS(
                    position,
                    Quaternion.identity,
                    Vector3.one * pointSize
                );
                
                props[i] = new MaterialPropertyBlock();
                props[i].SetColor("_BaseColor", colors[dataIndex]);
            }
            
            batches.Add(matrices);
            propertyBlocks.Add(props);
        }
        
        isRendering = true;
        Debug.Log($"Generated {totalPoints} points in {batchCount} batches");
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
    
    private void Update()
    {
        if (!isRendering) return;
        
        for (int b = 0; b < batches.Count; b++)
        {
            Graphics.DrawMeshInstanced(
                pointMesh,
                0,
                pointMaterial,
                batches[b]
            );
        }
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








}