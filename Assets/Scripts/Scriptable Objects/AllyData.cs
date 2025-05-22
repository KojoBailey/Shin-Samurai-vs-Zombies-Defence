using UnityEngine;

[CreateAssetMenu(fileName = "NewAlly", menuName = "Game Data/Ally")]
public class AllyData : ScriptableObject, IUpgradable {
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;
    public GameObject prefabWrapper;
    public CostumeData[] costumes;
    public MeleeWeaponData meleeWeaponData;
    public RangedWeaponData rangedWeaponData;

    public CostumeData GetEquippedCostume() {
        CostumeData costume = costumes[SaveManager.equippedCostumes[id]];
        if (costume.material)
            costume.prefab.GetComponent<SkinnedMeshRenderer>().material = costume.material;
        return costume;
    }

    public enum Stat {
        Cost,
        Cooldown,
        Health,
        Speed,
        Damage,
        AttackFrequency,
        KnockbackCount,
        Range
    }
    public GenericDictionary<Stat, float> stats;
    public GenericDictionary<Stat, float>[] upgrades;

    public int cost {
        get => (int)GetStat(Stat.Cost);
    }
    public float cooldown {
        get => GetStat(Stat.Cooldown);
    }
    public float health {
        get => GetStat(Stat.Health);
    }
    public float speed {
        get => GetStat(Stat.Speed);
    }
    public int knockbackCount {
        get => (int)GetStat(Stat.KnockbackCount);
    }

    public float GetStat(Stat stat) {
        return GetStat(SaveManager.levels[this], stat);
    }
    public float GetStat(int level, Stat stat) {
        if (upgrades[level - 1].ContainsKey(stat))
            return upgrades[level - 1][stat];
        return stats[stat];
    }
}
