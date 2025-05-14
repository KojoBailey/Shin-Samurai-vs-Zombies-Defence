using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class ImageRenderer : EditorWindow {
	[SerializeField] private Camera cam;

	[SerializeField] int resolutionX = 1920;
	[SerializeField] int resolutionY = 1080;

	[SerializeField] string outputPath = "Rendering/output.png";

	[MenuItem("Tools/Snapshot")]
	private static void GetMe() {
		GetWindow<ImageRenderer>();
	}

	private void OnGUI() {
		cam = (Camera)EditorGUILayout.ObjectField("Rendering Camera", cam, typeof(Camera), true);
		resolutionX = EditorGUILayout.IntField("Resolution X", resolutionX);
		resolutionY = EditorGUILayout.IntField("Resolution Y", resolutionY);
		outputPath = GUILayout.TextField(outputPath);

        if (GUILayout.Button("Snap")) Snap();
	}

	private void Snap() {
		if (cam == null) {
			EditorUtility.DisplayDialog("Camera not assigned", "Assign the camera to be used for the snapshot.", "OK");
			return;
		}
		
		cam.clearFlags = CameraClearFlags.Skybox;
		cam.backgroundColor = Color.black;
		cam.cullingMask = -1;

        RenderTexture rTex = new RenderTexture(resolutionX, resolutionY, 24, RenderTextureFormat.ARGB32) {
			useMipMap = false,
			autoGenerateMips = false
		};
        cam.targetTexture = rTex;

        Texture2D snapshot = new Texture2D(resolutionX, resolutionY, TextureFormat.RGBA32, false);

		SceneView.RepaintAll();
        cam.Render();

        RenderTexture.active = rTex;

        snapshot.ReadPixels(new Rect(0, 0, resolutionX, resolutionY), 0, 0);

        cam.targetTexture = null;
        RenderTexture.active = null;

		byte[] b = snapshot.EncodeToPNG();

		string path = Path.Combine(Application.dataPath, outputPath);

        if (!Directory.Exists(Path.GetDirectoryName(path)))
			Directory.CreateDirectory(Path.GetDirectoryName(path));

		File.WriteAllBytes(path, b);

        // Prevent memory leaks.
        DestroyImmediate(rTex);
        DestroyImmediate(snapshot);

        AssetDatabase.Refresh();
	}
}
