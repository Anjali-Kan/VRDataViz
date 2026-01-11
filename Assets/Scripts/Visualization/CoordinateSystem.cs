using UnityEngine;

public class CoordinateSystem : MonoBehaviour
{
    [Header("Axis Settings")]
    [SerializeField] private float axisLength = 5f;
    [SerializeField] private float axisThickness = 0.02f;

    [Header("Grid Settings")]
    [SerializeField] private int gridDivisions = 10;
    [SerializeField] private float gridLineThickness = 0.005f;

    [Header("Colors")]
    [SerializeField] private Color xAxisColor = Color.red;
    [SerializeField] private Color yAxisColor = Color.green;
    [SerializeField] private Color zAxisColor = Color.blue;
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);

    private void Start()
    {
        CreateAxes();
        CreateGrid();
        CreateLabels();
    }

    private void CreateAxes()
    {
        CreateAxisLine(Vector3.right, axisLength, xAxisColor, "X-Axis");  //X-Axis
        CreateAxisLine(Vector3.up, axisLength, yAxisColor, "Y-Axis"); //Y-Axis
        CreateAxisLine(Vector3.forward, axisLength, zAxisColor, "Z-Axis"); //Z-Axis
        
       
    }
    
    private void CreateGrid()
    {
        float step = axisLength / gridDivisions;

        //Grid on XZ 
        for(int i = -gridDivisions; i <= gridDivisions; i++){
            //Lines parallel to X-Axis
            CreateGridLine(
                new Vector3(-axisLength, 0f, i * step),
                new Vector3(axisLength, 0f, i * step),
                "GridX_" + i
            );

            //Lines parallel to Z-Axis
            CreateGridLine(
                new Vector3(i * step, 0f, -axisLength),
                new Vector3(i * step, 0f, axisLength),
                "GridZ_" + i
            );

        }
    }
    
    private void CreateLabels()
    {
        CreateLabel("X", new Vector3(axisLength + 0.3f, 0, 0), xAxisColor);
        CreateLabel("Y", new Vector3(0, axisLength + 0.3f, 0), yAxisColor);
        CreateLabel("Z", new Vector3(0, 0, axisLength + 0.3f), zAxisColor);
       
    }

    private void CreateLabel(string text, Vector3 position, Color color)
    {
        GameObject label = new GameObject("Label_" + text);
        label.transform.SetParent(transform);
        label.transform.localPosition = position;

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 50;
        textMesh.characterSize = 0.1f;
        textMesh.color = color;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
    }

    private void CreateAxisLine(Vector3 direction,float length, Color color, string name)
    {
        GameObject axis = new GameObject(name);
        axis.transform.SetParent(transform);
        
        LineRenderer lr = axis.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, direction * -length);  // Negative direction
        lr.SetPosition(1, direction * length);   // Positive direction
        
        lr.startWidth = axisThickness;
        lr.endWidth = axisThickness;
        
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        lr.material = mat;
        lr.startColor = color;
        lr.endColor = color;

    }

    private void CreateGridLine(Vector3 start, Vector3 end, string name)
    {
        GameObject gridLine = new GameObject(name);
        gridLine.transform.SetParent(transform);

        LineRenderer lineRenderer = gridLine.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.startWidth = gridLineThickness;
        lineRenderer.endWidth = gridLineThickness;

        // //Material and Color
        // lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        // lineRenderer.startColor = gridColor;
        // lineRenderer.endColor = gridColor;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = gridColor;
        lineRenderer.material = mat;
        lineRenderer.startColor = gridColor;
        lineRenderer.endColor = gridColor;
    }




}