using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SceneLoadManager { // Scene Load Manager
    private static bool m_isLoadingScene = false;
    private static GameObject m_loadingScreenInstance;

    public static bool hasFinishedLoading;

    public static async void LoadScene(string id, bool isTheLoadingScreen = true)
    {
        if (m_isLoadingScene) {
            return;
        }
        m_isLoadingScene = true;
        hasFinishedLoading = false;

        if (isTheLoadingScreen) {
            var loadingHandle = Addressables.InstantiateAsync("Prefabs/Loading Screen");
            m_loadingScreenInstance = await loadingHandle.Task;
        }

        var handle = Addressables.LoadSceneAsync("Scenes/" + id, LoadSceneMode.Single);
        await handle.Task;
        if (!isTheLoadingScreen) {
            m_isLoadingScene = false;
        }
    }

    public static void FinishLoading()
    {
        // Remove the loading screen once the scene has decided it is ready.
        if (m_loadingScreenInstance != null) {
            var controller = m_loadingScreenInstance.GetComponent<LoadingScreen>();
            controller.Destroy();
        }
        m_isLoadingScene = false;
        hasFinishedLoading = true;
    }
}