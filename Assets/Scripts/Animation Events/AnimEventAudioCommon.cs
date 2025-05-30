using System.Threading.Tasks;
using UnityEngine;

/* Common audio that is shared among different entities. */
[AddComponentMenu("Animation Event/Audio/Common")]
public class AnimEventAudioCommon : MonoBehaviour { // Animation Event: Common Audio
    public void SwooshSmall() {
        SFXManager.PlayFromBundle("Combat/Swoosh Small");
    }
    public void SwooshMedium() {
        SFXManager.PlayFromBundle("Combat/Swoosh Medium");
    }
    public void ArrowFire() {
        SFXManager.PlayFromBundle("Combat/Arrow Fire");
    }
};