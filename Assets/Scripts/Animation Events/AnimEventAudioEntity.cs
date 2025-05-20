using System.Threading.Tasks;
using UnityEngine;

/* Audio that is specific to certain entities, but with consistent categories. */
[AddComponentMenu("Animation Event/Audio/Entity")]
public class AnimEventAudioEntity : MonoBehaviour { // Animation Event: Entity Audio
    [SerializeField] private EntityAudioData m_audioData;

    public void Spawn() {
        m_audioData.Spawn();
    }
    public async Task Footstep() {
        await m_audioData.Footstep();
    }
    public void Attack() {
        m_audioData.Attack();
    }
    public void BigAttack() {
        m_audioData.BigAttack();
    }
    public void LongAttack() {
        m_audioData.LongAttack();
    }
    public void Die() {
        m_audioData.Die();
    }
};