using UnityEngine;
using System.Runtime.InteropServices;

public class WebFileUpload : MonoBehaviour
{
    // JavaScript function to trigger file picker
    [DllImport("__Internal")]
    private static extern void OpenFilePicker();
    
    private bool showUI = true;
    
    private void OnGUI()
    {
        if (UnityEngine.XR.XRSettings.isDeviceActive) return;
        if (!showUI) return;
        
        float buttonWidth = 150;
        float buttonHeight = 40;
        float x = Screen.width - buttonWidth - 20;
        float y = Screen.height - buttonHeight - 20;
        
        if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Load Custom CSV"))
        {
            TriggerFileUpload();
        }
        
        // Reset to default button
        if (GUI.Button(new Rect(x - buttonWidth - 10, y, buttonWidth, buttonHeight), "Load Iris Dataset"))
        {
            DataManager.Instance.LoadIrisDataset();
        }
        // Reset to more visual button
        if (GUI.Button(new Rect(x - buttonWidth - 180, y, buttonWidth, buttonHeight), "Load Lorenz Dataset"))
        {
            DataManager.Instance.LoadLorenzDataset();
        }
    }
    
    public void TriggerFileUpload()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            OpenFilePicker();
        #else
            Debug.Log("File upload only works in WebGL build");

        #endif
    }
    
    // Called from JavaScript when file is selected
    public void OnFileSelected(string csvContent)
    {
        DataManager.Instance.LoadCSVFromBrowser(csvContent);
    }
}