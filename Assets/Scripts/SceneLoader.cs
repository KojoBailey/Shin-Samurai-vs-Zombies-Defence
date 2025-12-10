using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class SceneLoader
{
    private static bool _isLoadingScene;
    private static GameObject _loadingScreenInstance;

    public static bool hasFinishedLoading { get; private set; }
    
    public static void StartLoading(string id)
    {
        _ = StartLoadingInternal(id);
    }
    
    private static async Task StartLoadingInternal(string id)
    {
        if (_isLoadingScene) return;
        
        _isLoadingScene = true;
        hasFinishedLoading = false;

        try
        {
            if (id == "LoadingScreen")
            {
                var loadingScreenHandle = Addressables.InstantiateAsync("Prefabs/Loading Screen");
                _loadingScreenInstance = await loadingScreenHandle.Task;
            }

            var handle = Addressables.LoadSceneAsync($"Scenes/{id}");
            await handle.Task;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            _isLoadingScene = false;
        }
    }

    public static void FinishLoading()
    {
        if (_loadingScreenInstance != null)
        {
            _loadingScreenInstance.GetComponent<LoadingScreen>().Destroy();
            Addressables.ReleaseInstance(_loadingScreenInstance);
        }
        hasFinishedLoading = true;
        _isLoadingScene = false;
    }
}