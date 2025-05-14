using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "NewEntityAudioData", menuName = "Game Data/Entity Audio")]
public class EntityAudioData : ScriptableObject {
    public enum Size { Normal, Large }
    public AudioClip[] SpawnSounds;
    public Size FootstepSize; 
    public AudioClip[] FootstepSounds;
    public AudioClip[] AttackSounds;
    public AudioClip[] BigAttackSounds;
    public AudioClip[] DeathSounds;

    public void Spawn() {
        SFXManager.Play(SpawnSounds[Random.Range(0, SpawnSounds.Length)]);
    }
    public async Task Footstep() {
        if (FootstepSounds.Length == 0) {
            switch (FootstepSize) {
                case Size.Large:
                    await SFXManager.Play($"Combat/Footstep Large 0{Random.Range(0, 3)}");
                    break;
                default:
                    await SFXManager.Play($"Combat/Footstep 0{Random.Range(0, 5)}");
                    break;
            }
            return;
        }
        SFXManager.Play(FootstepSounds[Random.Range(0, FootstepSounds.Length)]);
    }
    public void Attack() {
        SFXManager.Play(AttackSounds[Random.Range(0, AttackSounds.Length)]);
    }
    public void Die() {
        SFXManager.Play(DeathSounds[Random.Range(0, AttackSounds.Length)]);
    }
}
