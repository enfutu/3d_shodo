using UnityEditor;
using UnityEngine;
using System.IO;

public class MeshBoundsEditor : EditorWindow
{
    private MeshFilter selectedMeshFilter;
    private Bounds newBounds;

    // 保存先パス（固定）
    private const string saveFolder = "Assets/enfutu/changeBoundsMesh/";

    [MenuItem("Tools/Mesh Bounds Editor")]
    public static void ShowWindow()
    {
        GetWindow<MeshBoundsEditor>("Mesh Bounds Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("Mesh Bounds Editor", EditorStyles.boldLabel);

        selectedMeshFilter = EditorGUILayout.ObjectField("Mesh Filter", selectedMeshFilter, typeof(MeshFilter), true) as MeshFilter;

        if (selectedMeshFilter == null || selectedMeshFilter.sharedMesh == null)
        {
            EditorGUILayout.HelpBox("MeshFilter with a valid mesh must be assigned.", MessageType.Warning);
            return;
        }

        Mesh originalMesh = selectedMeshFilter.sharedMesh;

        EditorGUILayout.LabelField("Original Mesh: " + originalMesh.name);

        Bounds currentBounds = originalMesh.bounds;
        EditorGUILayout.LabelField("Current Bounds:");
        EditorGUILayout.Vector3Field("Center", currentBounds.center);
        EditorGUILayout.Vector3Field("Size", currentBounds.size);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("New Bounds:");

        newBounds.center = EditorGUILayout.Vector3Field("New Center", newBounds.center);
        newBounds.size = EditorGUILayout.Vector3Field("New Size", newBounds.size);

        if (GUILayout.Button("Create and Save Modified Mesh"))
        {
            if (!AssetDatabase.IsValidFolder(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
                AssetDatabase.Refresh();
            }

            Mesh newMesh = Object.Instantiate(originalMesh);
            newMesh.name = originalMesh.name + "_Modified";
            newMesh.bounds = newBounds;

            string assetPath = saveFolder + newMesh.name + ".asset";

            // ユニークなパスに調整
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(newMesh, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            selectedMeshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

            Debug.Log($"Saved modified mesh to: {assetPath}");
        }

        if (GUILayout.Button("Reset Bounds (Recalculate)"))
        {
            Undo.RecordObject(originalMesh, "Recalculate Bounds");
            originalMesh.RecalculateBounds();
            EditorUtility.SetDirty(originalMesh);
            Debug.Log("Bounds recalculated for mesh: " + originalMesh.name);
        }
    }
}
