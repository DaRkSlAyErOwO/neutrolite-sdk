using UnityEditor;
using UnityEngine;

public class NitroEditorWindow : EditorWindow
{
    [MenuItem("Nitrolite/Settings")]
    public static void ShowWindow()
    {
        GetWindow<NitroEditorWindow>("Nitrolite");
    }

    private void OnGUI()
    {
        GUILayout.Label("Nitrolite Settings", EditorStyles.boldLabel);
        GUILayout.Label("Configure your RPC base URL and test tools here.", EditorStyles.wordWrappedLabel);
    }
}
