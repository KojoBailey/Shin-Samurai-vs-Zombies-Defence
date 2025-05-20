using UnityEngine;
using System.Threading.Tasks;

/* Contains references to specific audio clips for gameplay entities. */
[CreateAssetMenu(fileName = "NewEntityAudioData", menuName = "Game Data/Entity Audio")]
public class EntityAudioData : ScriptableObject {
    public enum Size { Normal, Large }
    public AudioClip[] spawnAudio;
    public Size footstepSize; 
    public AudioClip[] footstepAudio;
    public AudioClip[] attackAudio;
    public AudioClip[] bigAttackAudio;
    public AudioClip[] longAttackAudio;
    public AudioClip[] deathAudio;

    public void Spawn() {
        if (spawnAudio.Length > 0)
            SFXManager.Play(spawnAudio[Random.Range(0, spawnAudio.Length)]);
    }
    public async Task Footstep() {
        if (footstepAudio.Length == 0) {
            switch (footstepSize) {
                case Size.Large:
                    await SFXManager.Play($"Combat/Footstep Large 0{Random.Range(0, 3)}");
                    break;
                default:
                    await SFXManager.Play($"Combat/Footstep 0{Random.Range(0, 5)}");
                    break;
            }
            return;
        }
        SFXManager.Play(footstepAudio[Random.Range(0, footstepAudio.Length)]);
    }
    public void Attack() {
        SFXManager.Play(attackAudio[Random.Range(0, attackAudio.Length)]);
    }
    public void BigAttack() {
        SFXManager.Play(bigAttackAudio[Random.Range(0, bigAttackAudio.Length)]);
    }
    public void LongAttack() {
        SFXManager.Play(longAttackAudio[Random.Range(0, longAttackAudio.Length)]);
    }
    public void Die() {
        SFXManager.Play(deathAudio[Random.Range(0, deathAudio.Length)]);
    }
}
