using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

class BGM { // Background Music
    private string m_id;
    private AudioSource m_bgm;

    public BGM(string bgmId) {
        m_id = bgmId;
    }

    public async Task Init() {
        GameObject audioObject = new GameObject("BGM");
        m_bgm = audioObject.AddComponent<AudioSource>();
        var bgmHandle = Addressables.LoadAssetAsync<AudioClip>($"BGM/{m_id}");
        m_bgm.clip = await bgmHandle.Task;
        m_bgm.loop = true;
        m_bgm.Play();
    }
};