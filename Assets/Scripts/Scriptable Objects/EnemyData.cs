using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy")]
public class EnemyData : ScriptableObject {
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;
    public GameObject prefabWrapper;
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
