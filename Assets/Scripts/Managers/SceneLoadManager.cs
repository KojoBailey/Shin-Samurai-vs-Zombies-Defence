using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class SceneLoadManager { // Scene Load Manager
    private static bool m_loadingScene = false;
    private static GameObject m_loadingScreenInstance;

    public static bool finishedLoading;

    public static async void LoadScene(string id, bool loadingScreen = true) {
        // Prevent duplicate scene loading.
        if (m_loadingScene == true) return;
        m_loadingScene = true;
        finishedLoading = false;

        // Start loading screen.
        if (loadingScreen) {
            var loadingHandle = Addressables.InstantiateAsync("Prefabs/Loading Screen");
            m_loadingScreenInstance = await loadingHandle.Task;
        }

        // Load next scene.
        var handle = Addressables.LoadSceneAsync("Scenes/" + id, LoadSceneMode.Single);
        await handle.Task;
        // Debug.LogFormat("Successfully loaded scene: {0}", id);
        if (!loadingScreen) m_loadingScene = false;
    }

    public static void FinishLoading() {
        // Remove the loading screen once the scene has decided it is ready.
        if (m_loadingScreenInstance != null) {
            var controller = m_loadingScreenInstance.GetComponent<LoadingScreen>();
            controller.Destroy();
        }
        m_loadingScene = false;
        finishedLoading = true;
        // Debug.Log("Finished loading scene.");
    }
}
