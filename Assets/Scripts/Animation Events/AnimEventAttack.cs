using UnityEngine;

/* To be used on wrapper prefabs. */
[AddComponentMenu("Animation Event/Attack")]
public class AnimEventAttack : MonoBehaviour { // Animation Event: Attack
    public string entityId;

    public void DealDamage() {
        GameplayManager.instance.DealDamage(entityId);
    }
    public void FireProjectile() {
        GameplayManager.instance.FireProjectile(entityId);
    }
    public void ActivateAbility() {
        GameplayManager.instance.abilityManager.ActivateAbility(entityId);
    }
};