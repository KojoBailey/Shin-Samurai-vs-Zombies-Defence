using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy")]
public class EnemyData : ScriptableObject, IUpgradable {
    public string displayName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    public MeleeWeaponData meleeWeaponData;
    public RangedWeaponData rangedWeaponData;

    public EntityAudioData audioData;

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
        get {
            return stats[Stat.Health];
        }
    }
    public int knockbackCount {
        get {
            return (int)stats[Stat.KnockbackCount];
        }
    }

    public float GetStat(Stat stat) {
        return stats[stat];
    }
}
