using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewHero", menuName = "Game Data/Hero")]
public class HeroData : ScriptableObject, IUpgradable {
    public string displayName;
    public string description;
    public Sprite icon;
    public GameObject prefabWrapper;
    public CostumeData[] costumes;
    public MeleeWeaponData meleeWeaponData;
    public RangedWeaponData rangedWeaponData;

    public CostumeData GetEquippedCostume() {
        CostumeData costume = costumes[SaveManager.equippedCostumes[SaveManager.selectedHero]];
        if (costume.material)
            costume.prefab.GetComponent<SkinnedMeshRenderer>().material = costume.material;
        return costume;
    }

    public enum Stat {
        Health,
        HealthRegen,
        HealthRegenDelay,
        Acceleration
    }
    public GenericDictionary<Stat, float> stats;
    public GenericDictionary<Stat, float>[] upgrades;

    public float health {
        get {
            return GetStat(Stat.Health);
        }
    }
    public float healthRegen {
        get {
            return GetStat(Stat.HealthRegen);
        }
    }
    public float healthRegenDelay {
        get {
            return GetStat(Stat.HealthRegenDelay);
        }
    }
    public float acceleration {
        get {
            return GetStat(Stat.Acceleration);
        }
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
