using System.Threading.Tasks;
using UnityEngine;

/* Common audio that is shared among different entities. */
[AddComponentMenu("Animation Event/Audio/Common")]
public class AnimEventAudioCommon : MonoBehaviour { // Animation Event: Common Audio
    public async Task SwooshSmall() {
        await SFXManager.Play($"Combat/Swoosh Small 0{Random.Range(0, 5)}");
    }
    public async Task SwooshMedium() {
        await SFXManager.Play($"Combat/Swoosh Medium 0{Random.Range(0, 5)}");
    }
    public async Task ArrowFire() {
        await SFXManager.Play($"Combat/Arrow Fire 0{Random.Range(0, 3)}");
    }
};