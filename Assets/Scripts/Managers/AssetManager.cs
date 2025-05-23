using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Threading.Tasks;

/* Stores assets to be pre-loaded for reduced lag. */
public class AssetManager { // Asset Manager
    public const bool fastLoad = false;
    /* If true:
        - Disable loading of all SFX
    */

    public static List<AllyData> alliesData;
    public static Dictionary<string, EnemyData> enemiesData;
    
    public static GameObject healthBarPrefab;

    public static Dictionary<string, AudioClip> audioClips;

    public static async Task LoadGameplay() {
        /* Allies */
        alliesData = new List<AllyData>();
        var ashigaruDataHandle = Addressables.LoadAssetAsync<AllyData>($"Data/Allies/Humans/Ashigaru");
        AllyData ashigaruData = await ashigaruDataHandle.Task;
        if (ashigaruData == null) {
            Debug.LogError($"Could not find or load Ally of ID \"{"Humans/Ashigaru"}\".");
            return;
        }
        alliesData.Add(ashigaruData);
        SaveManager.SetLevel(ashigaruData, 1); // !! Remove once save system implemented

        /* Enemies*/
        enemiesData = new Dictionary<string, EnemyData>();
        var zombieDataHandle = Addressables.LoadAssetAsync<EnemyData>($"Data/Enemies/Zombies/LightZombie");
        EnemyData zombieData = await zombieDataHandle.Task;
        if (zombieData == null) {
            Debug.LogError($"Could not find or load Enemy of ID \"{"Zombies/LightZombie"}\".");
            return;
        }
        enemiesData.Add("LightZombie", zombieData);

        /* Health Bar */
        var healthBarHandle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Entity Health Bar");
        healthBarPrefab = await healthBarHandle.Task;

        /* Audio */
        audioClips = new Dictionary<string, AudioClip>();
        if (!fastLoad) {
            for (int i = 0; i < 5; i++)
                await LoadAudioClip($"Combat/Swoosh Small 0{i}");
            for (int i = 0; i < 5; i++)
                await LoadAudioClip($"Combat/Swoosh Medium 0{i}");
            for (int i = 0; i < 3; i++)
                await LoadAudioClip($"Combat/Arrow Fire 0{i}");
            for (int i = 0; i < 3; i++)
                await LoadAudioClip($"Combat/Footstep Large 0{i}");
            for (int i = 0; i < 5; i++)
                await LoadAudioClip($"Combat/Footstep 0{i}");
        }
    }

    private static async Task LoadAudioClip(string address) {
        var audioHandle = Addressables.LoadAssetAsync<AudioClip>($"Audio/{address}");
        AudioClip clip = await audioHandle.Task;
        if (clip == null) {
            Debug.LogError($"Could not load AudioClip from address \"Audio/{address}\".");
            return;
        }
        audioClips.Add(address, clip);
    }
}