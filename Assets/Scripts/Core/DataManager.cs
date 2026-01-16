using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private string defaultFileName = "lorenz_attractor_4k_webgl.csv";
     private string irisFileName = "iris.csv";
     private string lorenzFileName = "lorenz_attractor_4k_webgl.csv";
    [SerializeField] private char delimiter = ',';
    
    [Header("Debug")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool logColumnInfo = true;
    
    public DataSet CurrentDataSet { get; private set; }
    public bool IsDataLoaded => CurrentDataSet != null && CurrentDataSet.IsLoaded;
    
    public event System.Action OnDataLoaded;

    private Coroutine activeLoadRoutine;
    private int loadVersion = 0;

    
    private void Awake()
    {
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


    public void LoadLorenzDataset()
    {
        string path =Path.Combine(Application.streamingAssetsPath, lorenzFileName);
        LoadFromPath(path);
    }

    public void LoadIrisDataset()
    {
        string path =Path.Combine(Application.streamingAssetsPath, irisFileName);
        LoadFromPath(path);
    }


   public void LoadFromPath(string filePath)
    {
        Debug.Log($"[DataManager] LoadFromPath called: {filePath}");
        loadVersion++;

        if (activeLoadRoutine != null)
        {
            Debug.Log("[DataManager] Cancelling previous load coroutine");
            StopCoroutine(activeLoadRoutine);
            activeLoadRoutine = null;
        }

        activeLoadRoutine = StartCoroutine(LoadFileCoroutine(filePath, loadVersion));
    }

    private IEnumerator LoadFileCoroutine(string filePath, int version)
{
    Debug.Log($"[DataManager] LoadFileCoroutine started (version {version}): {filePath}");
    
    // Normalize path for UnityWebRequest
    string requestPath = filePath;

    // In Editor/Standalone, UnityWebRequest for local files needs file://
    // (StreamingAssetsPath is a normal folder path there)
    if (!requestPath.StartsWith("http") && !requestPath.StartsWith("file://"))
        requestPath = "file://" + requestPath;

    using (UnityWebRequest request = UnityWebRequest.Get(requestPath))
    {
        yield return request.SendWebRequest();

        // If a newer load started while we were waiting, ignore this result
        if (version != loadVersion)
        {
            Debug.Log($"[DataManager] Load cancelled - newer load started (version {version} vs {loadVersion})");
            activeLoadRoutine = null;
            yield break;
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            string content = request.downloadHandler.text;
            Debug.Log($"[DataManager] File loaded successfully, content length: {content?.Length ?? 0}");
            LoadFromString(content, version);
        }
        else
        {
            Debug.LogError($"[DataManager] Failed to load file: {request.error} ({filePath})");

            // Fallback for editor/standalone local read
            if (File.Exists(filePath))
            {
                Debug.Log($"[DataManager] Attempting fallback file read");
                string content = File.ReadAllText(filePath);
                LoadFromString(content, version);
            }
            else
            {
                Debug.LogError($"[DataManager] File does not exist: {filePath}");
            }
        }
    }
    
    activeLoadRoutine = null;
}

    
    public void LoadFromString(string csvContent, int version = -1)
{
    Debug.Log($"[DataManager] LoadFromString called (version: {version}, current: {loadVersion})");
    
    // If version is provided and doesn't match current loadVersion, ignore this load
    if (version >= 0 && version != loadVersion)
    {
        Debug.Log($"[DataManager] LoadFromString ignored - outdated version ({version} vs {loadVersion})");
        return;
    }
    
    Debug.Log($"CSV chars: {csvContent?.Length ?? 0}");
    if (string.IsNullOrWhiteSpace(csvContent))
    {
        Debug.LogError("[DataManager] CSV content is empty/whitespace");
        CurrentDataSet = null;
        return;
    }

    CurrentDataSet = new DataSet();

    if (CurrentDataSet.LoadFromCSV(csvContent, delimiter))
    {
        Debug.Log($"[DataManager] Data loaded: {CurrentDataSet.Parser.RowCount} rows");
        if (CurrentDataSet.Parser.RowCount == 0)
        {
            Debug.LogError("[DataManager] Parsed 0 rows — header mismatch or parse failure.");
            CurrentDataSet = null;
            return;
        }

        if (logColumnInfo) LogColumns();
        
        // Double-check version before firing event
        if (version >= 0 && version != loadVersion)
        {
            Debug.Log($"[DataManager] OnDataLoaded cancelled - outdated version ({version} vs {loadVersion})");
            return;
        }
        
        Debug.Log($"[DataManager] Invoking OnDataLoaded event (version {loadVersion})");
        OnDataLoaded?.Invoke();
    }
    else
    {
        Debug.LogError("[DataManager] Failed to load CSV data");
        CurrentDataSet = null;
    }
}

    
    private void LogColumns()
    {
        Debug.Log("=== Column Info ===");
        
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
    
    // Called from JavaScript in WebGL
    public void LoadCSVFromBrowser(string csvContent)
    {
        Debug.Log("[DataManager] Loading CSV from browser upload...");
        loadVersion++;
        
        // Cancel any active file load
        if (activeLoadRoutine != null)
        {
            Debug.Log("[DataManager] Cancelling previous load coroutine (browser upload)");
            StopCoroutine(activeLoadRoutine);
            activeLoadRoutine = null;
        }
        
        LoadFromString(csvContent, loadVersion);
    }
}