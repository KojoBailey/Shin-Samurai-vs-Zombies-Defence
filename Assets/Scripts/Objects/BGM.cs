using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;

class BGM {
    private string id;
    private AudioSource bgm;

    public BGM(string bgmId) {
        id = bgmId;
    }

    public async Task Init() {
        GameObject audioObject = new GameObject("BGM");
        bgm = audioObject.AddComponent<AudioSource>();
        var bgmHandle = Addressables.LoadAssetAsync<AudioClip>("BGM/Zen Garden Day");
        bgm.clip = await bgmHandle.Task;
        bgm.loop = true;
        bgm.Play();
    }
};