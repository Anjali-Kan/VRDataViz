using UnityEngine;

public class WebInstructionsUI : MonoBehaviour
{
    private bool showInstructions = true;
    
    private void OnGUI()
    {
        // Skip in VR mode
        if (UnityEngine.XR.XRSettings.isDeviceActive) return;
        
        // Toggle with H key
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.H)
        {
            showInstructions = !showInstructions;
        }
        
        if (!showInstructions) return;
        
        // Create style
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 14;
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.normal.textColor = Color.white;
        
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;
        
        // Draw instructions box
        float width = 250;
        float height = 220;
        float x = 10;
        float y = 10;
        
        GUI.Box(new Rect(x, y, width, height), "");
        
        GUILayout.BeginArea(new Rect(x + 10, y + 10, width - 20, height - 20));
        
        GUILayout.Label("VR DataViz Controls", titleStyle);
        GUILayout.Space(10);
        GUILayout.Label("WASD - Move");
        GUILayout.Label("Q/E - Up/Down");
        GUILayout.Label("Right Click + Mouse - Look");
        GUILayout.Label("Left Click - Select/Interact");
        GUILayout.Label("G - Grab visualization");
        GUILayout.Label("F - Draw annotation");
        GUILayout.Space(10);
        GUILayout.Label("Press H to hide this");
        GUILayout.Space(10);
        GUILayout.Label("VR: Click 'Enter VR' below");
        
        GUILayout.EndArea();
    }
}
