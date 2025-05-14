using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SFXManager {
    static private GameObject audioObject = new GameObject("SFX");
    static private List<AudioSource> audioSources;

    public static void Init() {
        UnityEngine.Object.DontDestroyOnLoad(audioObject);
        audioSources = new();
        for (int i = 0; i < 30; i++) {
            audioSources.Add(audioObject.AddComponent<AudioSource>());
        }
    }

    public static void Play(AudioClip clip) {
        AudioSource availableSlot = null;
        foreach (var audioSource in audioSources) {
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