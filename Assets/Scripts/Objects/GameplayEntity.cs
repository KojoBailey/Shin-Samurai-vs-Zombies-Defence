using UnityEngine;

public class GameplayEntity {
    public enum Side { Left, Right }
    public string EntityId;
    public Side Allegiance;
    protected bool loaded = false;

    public GameObject obj;
    public Transform transform;
    public Animation animation;
    public GameObject wrapperObj;
    public Animation wrapperAnimation;

    public float Health;

    public void SetEntityId(string newId) {
        EntityId = newId;
        if (wrapperAnimation != null)
            wrapperObj.GetComponent<AnimEventAttack>().EntityId = EntityId;
        else
            obj.GetComponent<AnimEventAttack>().EntityId = EntityId;
    }

    protected void Prepare() {
        obj.SetActive(false);
        transform = obj.transform;
        animation = obj.GetComponent<Animation>();
    }

    public void Damage(float damage) {
        Health -= damage;
        Debug.Log($"Entity \"{EntityId}\" took {damage} damage. Health: {Health}");
    }
    public void Heal(float damage) {
        Health += damage;
    }

    public virtual float GetMeleeDamage() { return 0; }
    public virtual bool IsInMeleeRange(float _x) { return false; }
    public virtual void MeleeAttackSFX() {}
};