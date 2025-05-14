using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class SceneLoadManager {
    private static bool loadingScene = false;
    private static GameObject loadingScreenInstance;

    public static bool finishedLoading;

    public static async void LoadScene(string id, bool loadingScreen = true) {
        // Prevent duplicate scene loading.
        if (loadingScene == true) return;
        loadingScene = true;
        finishedLoading = false;

        // Start loading screen.
        if (loadingScreen) {
            var loadingHandle = Addressables.InstantiateAsync("Scenes/LoadingScreen");
            loadingScreenInstance = await loadingHandle.Task;
        }

        // Load next scene.
        var handle = Addressables.LoadSceneAsync("Scenes/" + id, LoadSceneMode.Single);
        await handle.Task;
        Debug.LogFormat("Successfully loaded scene: {0}", id);
        if (!loadingScreen) loadingScene = false;
    }

    public static void FinishLoading() {
        // Remove the loading screen once the scene has decided it is ready.
        if (loadingScreenInstance != null) {
            var controller = loadingScreenInstance.GetComponent<LoadingScreen>();
            controller.Destroy();
        }
        loadingScene = false;
        finishedLoading = true;
        Debug.Log("Finished loading scene.");
    }
}
