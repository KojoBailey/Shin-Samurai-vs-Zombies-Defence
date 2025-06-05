using UnityEngine;
using System.Threading.Tasks;

/* Contains references to specific audio clips for gameplay entities. */
[CreateAssetMenu(fileName = "NewEntityAudioData", menuName = "Game Data/Entity Audio")]
public class EntityAudioData : ScriptableObject {
    public enum Size { Normal, Large }
    public AudioBundle spawnAudio;
    public Size footstepSize; 
    public AudioBundle footstepAudio;
    public AudioBundle attackAudio;
    public AudioBundle bigAttackAudio;
    public AudioBundle longAttackAudio;
    public GenericDictionary<string, AudioBundle> personalAbilityAudio;
    public AudioBundle damagedAudio;
    public AudioBundle deathAudio;

    public void Spawn() {
        SFXManager.PlayFromBundle(spawnAudio);
    }
    public void Footstep() {
        if (footstepAudio == null) {
            switch (footstepSize) {
                case Size.Large:
                    SFXManager.PlayFromBundle("Combat/Footstep Large");
                    break;
                default:
                    SFXManager.PlayFromBundle("Combat/Footstep");
                    break;
            }
            return;
        }
        SFXManager.PlayFromBundle(footstepAudio);
    }
    public void Attack() {
        SFXManager.PlayFromBundle(attackAudio);
    }
    public void BigAttack() {
        SFXManager.PlayFromBundle(bigAttackAudio);
    }
    public void LongAttack() {
        SFXManager.PlayFromBundle(longAttackAudio);
    }
    public void PersonalAbility(string abilityId) {
        SFXManager.PlayFromBundle(personalAbilityAudio[abilityId]);
    }
    public void Damaged() {
        SFXManager.PlayFromBundle(damagedAudio);
    }
    public void Die() {
        SFXManager.PlayFromBundle(deathAudio);
    }
}
