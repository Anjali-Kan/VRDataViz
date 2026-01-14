using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;
    
    public SerializableVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class DrawingData
{
    public List<SerializableVector3> points = new List<SerializableVector3>();
}

[System.Serializable]
public class ViewState
{
    public float[] vizPosition = new float[3];
    public float[] vizRotation = new float[3];
    public int xColumn;
    public int yColumn;
    public int zColumn;
    public int colorColumn;
    public List<DrawingData> drawings = new List<DrawingData>();
}

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    
    private string savePath;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        savePath = Path.Combine(Application.persistentDataPath, "viewstate.json");
        Debug.Log("Save path: " + savePath);
    }
    
    public void SaveViewState()
    {
        ViewState state = new ViewState();
        
        // Get visualization transform
        CoordinateSystem coordSystem = FindFirstObjectByType<CoordinateSystem>();
        if (coordSystem != null)
        {
            state.vizPosition[0] = coordSystem.transform.position.x;
            state.vizPosition[1] = coordSystem.transform.position.y;
            state.vizPosition[2] = coordSystem.transform.position.z;
            
            state.vizRotation[0] = coordSystem.transform.eulerAngles.x;
            state.vizRotation[1] = coordSystem.transform.eulerAngles.y;
            state.vizRotation[2] = coordSystem.transform.eulerAngles.z;
        }
        
        // Get axis mapping
        PointCloudRenderer renderer = FindFirstObjectByType<PointCloudRenderer>();
        if (renderer != null)
        {
            state.xColumn = renderer.GetXColumn();
            state.yColumn = renderer.GetYColumn();
            state.zColumn = renderer.GetZColumn();
            state.colorColumn = renderer.GetColorColumn();
        }
        
        // Get drawings
        VRDrawing drawing = FindFirstObjectByType<VRDrawing>();
        if (drawing != null)
        {
            List<List<Vector3>> drawingsData = drawing.GetAllDrawingsData();
            foreach (List<Vector3> points in drawingsData)
            {
                DrawingData dd = new DrawingData();
                foreach (Vector3 p in points)
                {
                    dd.points.Add(new SerializableVector3(p));
                }
                state.drawings.Add(dd);
            }
        }
        
        // Save to JSON
        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(savePath, json);
        
        Debug.Log("View state saved with " + state.drawings.Count + " drawings!");
    }
    
    public void LoadViewState()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No saved view state found");
            return;
        }
        
        string json = File.ReadAllText(savePath);
        ViewState state = JsonUtility.FromJson<ViewState>(json);
        
        // Apply visualization transform
        CoordinateSystem coordSystem = FindFirstObjectByType<CoordinateSystem>();
        if (coordSystem != null)
        {
            coordSystem.transform.position = new Vector3(
                state.vizPosition[0],
                state.vizPosition[1],
                state.vizPosition[2]
            );
            
            coordSystem.transform.eulerAngles = new Vector3(
                state.vizRotation[0],
                state.vizRotation[1],
                state.vizRotation[2]
            );
        }
        
        // Apply axis mapping
        PointCloudRenderer renderer = FindFirstObjectByType<PointCloudRenderer>();
        if (renderer != null)
        {
            renderer.SetAxisMapping(state.xColumn, state.yColumn, state.zColumn);
            renderer.SetColorColumn(state.colorColumn);
        }
        
        // Load drawings
        VRDrawing drawing = FindFirstObjectByType<VRDrawing>();
        if (drawing != null && state.drawings != null)
        {
            List<List<Vector3>> drawingsData = new List<List<Vector3>>();
            foreach (DrawingData dd in state.drawings)
            {
                List<Vector3> points = new List<Vector3>();
                foreach (SerializableVector3 sv in dd.points)
                {
                    points.Add(sv.ToVector3());
                }
                drawingsData.Add(points);
            }
            drawing.LoadDrawings(drawingsData);
        }
        
        Debug.Log("View state loaded!");
    }
}