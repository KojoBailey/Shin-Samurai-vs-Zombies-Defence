using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Game Data/Ability")]
public class AbilityData : ScriptableObject, IUpgradable {
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;
    public AudioClip soundEffect;
    public Color materialColour;

    public enum Stat {
        Cost,
        Cooldown,
        Duration,
        Range,
        SpeedMultiplier,
        Damage
    }
    public GenericDictionary<Stat, float> stats;
    public GenericDictionary<Stat, float>[] upgrades;

    public float cost {
        get => GetStat(Stat.Cost);
    }
    public float cooldown {
        get => GetStat(Stat.Cooldown);
    }
    public float duration {
        get => GetStat(Stat.Duration);
    }
    public float range {
        get => GetStat(Stat.Range);
    }
    public float speedMultiplier {
        get => GetStat(Stat.SpeedMultiplier);
    }
    public float damage {
        get => GetStat(Stat.Damage);
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