using UnityEngine;
using System.Linq;

/// <summary>
/// Prevents duplicate WebXRManager instances by destroying this GameObject if another WebXRManager already exists.
/// </summary>
public class WebXRManagerDuplicateFix : MonoBehaviour
{
    private void Awake()
    {
        // Find all WebXRManager components (using reflection to avoid direct dependency)
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var webXRManagers = allMonoBehaviours.Where(mb => mb != null && mb.GetType().Name == "WebXRManager").ToList();
        
        if (webXRManagers.Count > 1)
        {
            // Check if this GameObject has a WebXRManager
            var thisWebXR = webXRManagers.FirstOrDefault(mb => mb.gameObject == this.gameObject);
            
            if (thisWebXR != null)
            {
                // Keep the first one found, destroy this one
                var firstWebXR = webXRManagers.FirstOrDefault(mb => mb.gameObject != this.gameObject);
                if (firstWebXR != null)
                {
                    Debug.Log($"[WebXRManagerDuplicateFix] Found {webXRManagers.Count} WebXRManager instances. Destroying duplicate on {gameObject.name}");
                    Destroy(gameObject);
                }
            }
        }
        else if (webXRManagers.Count == 1)
        {
            Debug.Log("[WebXRManagerDuplicateFix] Single WebXRManager found - no duplicates");
        }
    }
}
