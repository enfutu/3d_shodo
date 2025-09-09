using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class QuadGridMeshGenerator : EditorWindow
{
    private float uvMargin = 0.00001f; // Inspectorから調整可能

    [MenuItem("enfutu/Generate/QuadGridMesh")]
    static void ShowWindow()
    {
        GetWindow<QuadGridMeshGenerator>("QuadGridMesh Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Quad Grid Mesh Settings", EditorStyles.boldLabel);

        uvMargin = EditorGUILayout.FloatField("UV Margin (absolute)", uvMargin);

        if (GUILayout.Button("Generate Mesh"))
        {
            GenerateMesh();
        }
    }

    private void GenerateMesh()
    {
        int quadCount = 4096;
        float width = 4.096f;
        float height = 0.01f;
        float range = 2.0f;//1.0f; // [-1,1]立方体

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();

        int vertexOffset = 0;

        for (int i = 0; i < quadCount; i++)
        {
            // ランダム位置＆回転
            Vector3 pos = new Vector3(
                Random.Range(-range, range),
                Random.Range(-range, range),
                Random.Range(-range, range)
            );
            Quaternion rot = Random.rotation;

            // UV範囲（全体UVの絶対値マージンを適用）
            float vMin = (float)i / quadCount + uvMargin;
            float vMax = (float)(i + 1) / quadCount - uvMargin;

            Vector3[] quadVerts = new Vector3[4]
            {
                new Vector3(-width * 0.5f, -height * 0.5f, 0),
                new Vector3( width * 0.5f, -height * 0.5f, 0),
                new Vector3( width * 0.5f,  height * 0.5f, 0),
                new Vector3(-width * 0.5f,  height * 0.5f, 0)
            };

            Vector2[] quadUVs = new Vector2[4]
            {
                new Vector2(0, vMin),
                new Vector2(1, vMin),
                new Vector2(1, vMax),
                new Vector2(0, vMax)
            };

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = rot * quadVerts[j] + pos;
                vertices.Add(v);
                uvs.Add(quadUVs[j]);
                colors.Add(new Color(quadUVs[j].x, quadUVs[j].y, 0f, 1f));
            }

            triangles.Add(vertexOffset + 0);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 2);

            triangles.Add(vertexOffset + 0);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 3);

            vertexOffset += 4;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.RecalculateNormals();

        string folder = "Assets/enfutu/GeneratedMeshes";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/enfutu", "GeneratedMeshes");

        string path = folder + "/QuadGridMesh.asset";
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        GameObject parent = GameObject.Find("Washi");
        if (parent == null) parent = new GameObject("Washi");

        GameObject obj = new GameObject("QuadGridMesh");
        obj.transform.SetParent(parent.transform);
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;

        MeshCollider mc = obj.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;

        Debug.Log("QuadGridMesh生成完了: " + path);
    }
}
