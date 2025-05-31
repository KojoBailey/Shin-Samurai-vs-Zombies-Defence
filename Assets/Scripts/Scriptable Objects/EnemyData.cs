using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy")]
public class EnemyData : ScriptableObject {
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    public EntityAudioData audioData;

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

    public enum Stat {
        Health,
        Speed,
        Damage,
        AttackFrequency,
        KnockbackCount,
        Range
    }
    public GenericDictionary<Stat, float> stats;

    public float health {
        get => GetStat(Stat.Health);
    }
    public float speed {
        get => GetStat(Stat.Speed);
    }
    public float damage {
        get => GetStat(Stat.Damage);
    }
    public float attackFrequency {
        get => GetStat(Stat.AttackFrequency);
    }
    public int knockbackCount {
        get => (int)GetStat(Stat.KnockbackCount);
    }
    public float range {
        get => GetStat(Stat.Range);
    }

    public float GetStat(Stat stat) {
        
        return stats[stat];
    }
}
