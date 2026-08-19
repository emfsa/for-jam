#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Startup
{
    static Startup()    
    {
        EditorPrefs.SetInt("showCounts_lightingtools2025", EditorPrefs.GetInt("showCounts_lightingtools2025") + 1);

        if (EditorPrefs.GetInt("showCounts_lightingtools2025") == 1)       
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/326099");
            // System.IO.File.Delete("Assets/SportCar/Racing_Game.cs");
        }
    }     
}
#endif
