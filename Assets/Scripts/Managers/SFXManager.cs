using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
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

    public static void Play(AudioClip clip) {
        AudioSource availableSlot = null;
        foreach (var audioSource in m_audioSources) {
            if (!audioSource.isPlaying) {
                availableSlot = audioSource;
                break;
            }
        }
        if (availableSlot == null) {
            Debug.LogWarning("Maximum number of SFX slots reached. Could not play audio.");
            return;
        }
        availableSlot.clip = clip;
        availableSlot.Play();
    }
    public static async Task Play(string address) {
        var audioHandle = Addressables.LoadAssetAsync<AudioClip>($"Audio/{address}");
        AudioClip clip = await audioHandle.Task;
        Play(clip);
    }
};