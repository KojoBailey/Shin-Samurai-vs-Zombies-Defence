using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

class BGM { // Background Music
    private string id;
    private AudioSource bgm;

    public BGM(string _id) {
        id = _id;
    }

    public async Task Init() {
        GameObject audioObject = new GameObject("BGM");
        bgm = audioObject.AddComponent<AudioSource>();
        var bgmHandle = Addressables.LoadAssetAsync<AudioClip>($"BGM/{id}");
        bgm.clip = await bgmHandle.Task;
        bgm.loop = true;
    }

    public void Play() {
        bgm.Play();
    }
    public void Stop() {
        bgm.Stop();
    }
};