using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AllyManager {
    // A list of all Ally IDs, without loading the Allies themselves.
    public static List<string> AllyIds;

    public static async Task Init() {
        var handle = Addressables.LoadResourceLocationsAsync("Ally");
        await handle.Task;
        AllyIds = new();
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            IList<IResourceLocation> locations = handle.Result;
            foreach (var loc in locations) {
                AllyIds.Add(System.IO.Path.GetFileName(loc.PrimaryKey));
            }
        } else Debug.LogError("Failed to get list of Allies.");
    }

    // Loads an Ally's data into memory.
    public static async Task<Ally> LoadAlly(string _id) {
        var handle = Addressables.LoadAssetAsync<Ally>("Data/Allies/" + _id);
        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;
        else
            Debug.LogError("Failed to load sprite from Addressables!");
        return null;
    }
}
