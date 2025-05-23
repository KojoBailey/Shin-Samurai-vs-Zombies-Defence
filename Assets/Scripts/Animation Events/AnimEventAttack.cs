using UnityEngine;

/* To be used on wrapper prefabs. */
[AddComponentMenu("Animation Event/Attack")]
public class AnimEventAttack : MonoBehaviour { // Animation Event: Attack
    public string entityId;

    public void DealDamage() {
        GameplayManager.DealDamage(entityId);
    }
    public void FireProjectile() {
        GameplayManager.FireProjectile(entityId);
    }
    public void ActivateAbility() {
        AbilityManager.ActivateAbility();
    }
};