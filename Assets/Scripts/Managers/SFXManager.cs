using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class SFXManager { // Sound Effects Manager
    static private GameObject m_audioObject = new GameObject("SFX");
    static private List<AudioSource> m_audioSources;

    public static void Init() {
        Object.DontDestroyOnLoad(m_audioObject);
        m_audioSources = new();
        for (int i = 0; i < 30; i++) {
            m_audioSources.Add(m_audioObject.AddComponent<AudioSource>());
        }
    }

        public static async Task<AudioBundle> Load(string address) {
        var audioHandle = Addressables.LoadAssetAsync<AudioBundle>($"Audio/{address}");
        AudioBundle bundle = await audioHandle.Task;
        if (bundle == null) {
            Debug.LogError($"Could not load AudioBundle from address \"Audio/{address}\".");
            return null;
        }
        return bundle;
    }

    public static float PlayFromBundle(string address) {
        AudioBundle bundle = GameplayManager.instance.loadedAudio[address];
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
};