using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

/* Manages sound effects and dialogue, but not music. */
public class SFXManager { // Sound Effects Manager
    static private GameObject m_audioObject = new GameObject("SFX");
    static private List<AudioSource> m_audioSources;

    static private Dictionary<string, AudioBundle> loadedAudio = new Dictionary<string, AudioBundle>();
    // For keeping track of what uses what audio.
    static private Dictionary<string, List<string>> ownershipLog = new Dictionary<string, List<string>>();

    public static void Init() {
        Object.DontDestroyOnLoad(m_audioObject);
        m_audioSources = new();
        for (int i = 0; i < 30; i++) {
            m_audioSources.Add(m_audioObject.AddComponent<AudioSource>());
        }
    }

    public static async Task Load(string owner, string address) {
        var audioHandle = Addressables.LoadAssetAsync<AudioBundle>($"Audio/{address}");
        AudioBundle bundle = await audioHandle.Task;
        if (bundle == null) {
            Debug.LogError($"Could not load AudioBundle from address \"Audio/{address}\".");
            return;
        }
        loadedAudio.Add(address, bundle);
        if (!ownershipLog.ContainsKey(owner))
            ownershipLog.Add(owner, new List<string>());
        ownershipLog[owner].Add(address);
        Debug.Log($"Loaded AudioBundle of address \"{address}\" for \"{owner}\".");
    }

    public static float PlayFromBundle(string address) {
        AudioBundle bundle = loadedAudio[address];
        return Play(bundle.GetRandom());
    }
    public static float PlayFromBundle(AudioBundle bundle) {
        return Play(bundle.GetRandom());
    }
    public static float Play(AudioClip clip) {
        AudioSource availableSlot = null;
        foreach (var audioSource in m_audioSources) {
            if (!audioSource.isPlaying) {
                availableSlot = audioSource;
                break;
            }
        }
        if (availableSlot == null) {
            Debug.LogWarning("Maximum number of SFX slots reached. Could not play audio.");
            return 0;
        }
        availableSlot.clip = clip;
        availableSlot.Play();
        return availableSlot.clip.length;
    }

    public static void Clear(string owner) {
        foreach (string address in ownershipLog[owner]) {
            Addressables.Release(loadedAudio[address]);
            loadedAudio.Remove(address);
            Debug.Log($"Freed the cache of addressable \"{address}\" from \"{owner}\".");
        }
        ownershipLog.Remove(owner);
    }
};