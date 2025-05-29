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

    public static async Task<AudioClip> Load(string address) {
        var audioHandle = Addressables.LoadAssetAsync<AudioClip>($"Audio/{address}");
        AudioClip clip = await audioHandle.Task;
        if (clip == null) {
            Debug.LogError($"Could not load AudioClip from address \"Audio/{address}\".");
            return null;
        }
        return clip;
    }

    public static float Play(string address) {
        return Play(GameplayManager.instance.audioClips[address]);
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