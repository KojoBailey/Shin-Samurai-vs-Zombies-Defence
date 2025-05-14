using UnityEditor;
using UnityEngine;

public class MeshSaver
{
    [MenuItem("Tools/Save Mesh From MeshFilter")]
    static void SaveMeshFromMeshFilter()
    {
        var selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("No GameObject selected!");
            return;
        }

        MeshFilter meshFilter = selected.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("Selected object has no MeshFilter with a mesh.");
            return;
        }

        Mesh mesh = Object.Instantiate(meshFilter.sharedMesh);

        string path = "Assets/ExtractedMeshes/" + mesh.name + "_extracted.asset";

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Saved mesh as: " + path);
    }
}