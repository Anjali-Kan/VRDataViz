using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
     public static DataManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string defaultFileName = "sample.csv";
    [SerializeField] private char delimiter = ',';
    
    [Header("Debug")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool logColumnInfo = true;
    
    public DataSet CurrentDataSet { get; private set; }
    public bool IsDataLoaded => CurrentDataSet != null && CurrentDataSet.IsLoaded;
    
    public event System.Action OnDataLoaded;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (loadOnStart)
        {
            LoadDefaultFile();
        }
    }

    public void LoadDefaultFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, defaultFileName);
        LoadFromPath(path);
    }

    public void LoadFromPath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }
        
        string content = File.ReadAllText(filePath);
        LoadFromString(content);
    }


    public void LoadFromString(string csvContent)
    {
        CurrentDataSet = new DataSet();
        
        if (CurrentDataSet.LoadFromCSV(csvContent, delimiter))
        {
            Debug.Log($"Data loaded: {CurrentDataSet.Parser.RowCount} rows");
            
            if (logColumnInfo)
            {
                LogColumns();
            }
            
            OnDataLoaded?.Invoke();
        }
        else
        {
            Debug.LogError("Failed to load CSV data");
            CurrentDataSet = null;
        }
    }

    private void LogColumns()
    {
        Debug.Log("Column Info:");
        
        foreach (ColumnInfo col in CurrentDataSet.Columns)
        {
            if (col.IsNumeric)
            {
                Debug.Log($"[NUM] {col.Name}: {col.Min} to {col.Max}");
            }
            else
            {
                Debug.Log($"[CAT] {col.Name}: {col.UniqueCategories.Length} categories");
            }
        }
    }
    

}