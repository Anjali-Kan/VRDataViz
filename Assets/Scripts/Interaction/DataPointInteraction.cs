using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataPointInteraction : MonoBehaviour
{
    [Header("Tooltip Settings")]
    [SerializeField] private Vector3 tooltipOffset = new Vector3(0.1f, 0.1f, 0);
    
    private GameObject tooltip;
    private TextMeshProUGUI tooltipText;
    private Camera mainCamera;
    private GameObject currentHoveredPoint;
    
    private void Start()
    {
        mainCamera = Camera.main;
        CreateTooltip();
    }
    
    private void CreateTooltip()
    {
        // Create canvas for tooltip
        GameObject canvasObj = new GameObject("TooltipCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasScaler>();
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(300, 200);
        canvasRect.localScale = Vector3.one * 0.002f;
        
        // Background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);
        
        UnityEngine.UI.Image bg = panel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.85f);
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panel.transform, false);
        
        tooltipText = textObj.AddComponent<TextMeshProUGUI>();
        tooltipText.fontSize = 24;
        tooltipText.alignment = TextAlignmentOptions.TopLeft;
        tooltipText.color = Color.white;
        tooltipText.margin = new Vector4(10, 10, 10, 10);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        tooltip = canvasObj;
        tooltip.SetActive(false);
    }
    
    private void Update()
    {
        CheckHover();
        UpdateTooltipPosition();
    }
    
    private void CheckHover()
    {
        // Raycast from right controller
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 20f))
        {
            // Check if hit a data point
            if (hit.collider.gameObject.name.StartsWith("Point_"))
            {
                if (currentHoveredPoint != hit.collider.gameObject)
                {
                    currentHoveredPoint = hit.collider.gameObject;
                    ShowTooltip(currentHoveredPoint);
                }
                return;
            }
        }
        
        // No hit or not a data point
        if (currentHoveredPoint != null)
        {
            currentHoveredPoint = null;
            HideTooltip();
        }
    }
    
    private void ShowTooltip(GameObject point)
    {
        // Get point index from name (Point_0, Point_1, etc.)
        string indexStr = point.name.Replace("Point_", "");
        if (!int.TryParse(indexStr, out int index)) return;
        
        // Get data from DataManager
        DataSet data = DataManager.Instance.CurrentDataSet;
        if (data == null || index >= data.Parser.RowCount) return;
        
        // Build tooltip text
        string text = "<b>Data Point " + index + "</b>\n";
        text += "─────────────\n";
        
        for (int i = 0; i < data.Columns.Count; i++)
        {
            string colName = data.Columns[i].Name;
            string value = data.Parser.Rows[index][i];
            text += colName + ": " + value + "\n";
        }
        
        tooltipText.text = text;
        tooltip.SetActive(true);
    }
    
    private void HideTooltip()
    {
        tooltip.SetActive(false);
    }
    
    private void UpdateTooltipPosition()
    {
        if (!tooltip.activeSelf) return;
        
        if (currentHoveredPoint != null)
        {
            tooltip.transform.position = currentHoveredPoint.transform.position + tooltipOffset;
            
            // Face the camera
            tooltip.transform.LookAt(mainCamera.transform);
            tooltip.transform.Rotate(0, 180, 0);
        }
    }
}