using UnityEngine;

[AddComponentMenu("Animation Event/Attack")]
public class AnimEventAttack : MonoBehaviour {
    public string EntityId;

    public void DealDamage() {
        GameplayManager.DealDamage(EntityId);
    }
};