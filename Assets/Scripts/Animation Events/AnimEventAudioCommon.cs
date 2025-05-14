using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[AddComponentMenu("Animation Event/Audio/Common")]
public class AnimEventAudioCommon : MonoBehaviour {
    public async Task SwooshSmall() {
        await SFXManager.Play($"Combat/Swoosh Small 0{Random.Range(0, 5)}");
    }
    public async Task SwooshMedium() {
        await SFXManager.Play($"Combat/Swoosh Medium 0{Random.Range(0, 5)}");
    }
};