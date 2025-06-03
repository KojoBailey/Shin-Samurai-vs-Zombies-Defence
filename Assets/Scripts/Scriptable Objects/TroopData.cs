using UnityEngine;

public abstract class TroopData : ScriptableObject {
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;

    public virtual GameObject prefab => null;
    public virtual EntityAudioData audioData => null;

    public enum WeaponType { Melee, Ranged }
    [System.Serializable] public class MeleeWeapon {
        public GameObject prefab;
        public WeaponAnchor.Side hand;
        public AudioClip[] hitAudio;

        public void PlayHit() {
            if (hitAudio.Length > 0) {
                SFXManager.Play(hitAudio[Random.Range(0, hitAudio.Length)]);
            }
        }
    }
    public MeleeWeapon meleeWeapon;
    [System.Serializable] public class RangedWeapon {
        public GameObject prefab;
        public WeaponAnchor.Side hand;
        public GameObject projectile;
    }
    public RangedWeapon rangedWeapon;

    public AnimationEventData animationEvents;

    /* AI Types */
    public bool isMeleeAttacker;    // Attacks with a melee weapon, up-close.
    public bool isRangedAttacker;   // Atatcks with a ranged weapon from afar.
    public bool isHealer;           // Targets fellow allies with healing projectiles.
    public bool isFlying;           // Cannot be hit by ground-only melee troops.
    public bool isGateRusher;       // Ignores all enemies and goes straight for their gate.
    public bool isAbsorbant;        // Absorbs splash damage when hit.

    public enum Stat {
        Health,
        Speed,
        Damage,
        AttackFrequency,
        KnockbackCount,
        Range
    }
    public GenericDictionary<Stat, float> stats;

    public virtual int health => (int)stats[Stat.Health];
    public virtual float speed => stats[Stat.Speed];
    public virtual float damage => stats[Stat.Damage];
    public virtual float attackFrequency => stats[Stat.AttackFrequency];
    public virtual int knockbackCount => (int)stats[Stat.KnockbackCount];
    public virtual float range => stats[Stat.Range];
}