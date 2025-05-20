using UnityEngine;

[CreateAssetMenu(fileName = "NewAlly", menuName = "Game Data/Ally")]
public class AllyData : ScriptableObject, IUpgradable {
    public string displayName;
    public string description;
    public Sprite icon;
    public GameObject prefabWrapper;
    public CostumeData[] costumes;
    public MeleeWeaponData meleeWeaponData;
    public RangedWeaponData rangedWeaponData;

    public EntityAudioData audioData;

    public enum Stat {
        Cost,
        Health,
        Speed,
        Damage,
        AttackFrequency,
        KnockbackCount,
        Range
    }
    public GenericDictionary<Stat, float> stats;
    public GenericDictionary<Stat, float>[] upgrades;

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

    public object GetStat(Stat stat) {
        return GetStat(SaveManager.levels[this], stat);
    }
    public object GetStat(int level, Stat stat) {
        if (upgrades[level - 1].ContainsKey(stat))
            return upgrades[level - 1][stat];
        return stats[stat];
    }
}
