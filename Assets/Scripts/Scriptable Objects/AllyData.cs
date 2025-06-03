using UnityEngine;

[CreateAssetMenu(fileName = "NewAlly", menuName = "Game Data/Ally")]
public class AllyData : TroopData, IUpgradable {
    public int cost;
    public float cooldown;
    public bool isUnique; // Can only be on the field once at a time.
    public CostumeData[] costumes;

    public CostumeData GetEquippedCostume() {
        CostumeData costume = costumes[SaveManager.equippedCostumes[id]];
        if (costume.material)
            costume.prefab.GetComponent<SkinnedMeshRenderer>().material = costume.material;
        return costume;
    }
    public override GameObject prefab => GetEquippedCostume().prefab;
    public override EntityAudioData audioData => GetEquippedCostume().audioData;

    public GenericDictionary<Stat, float>[] upgrades;

    public override int health => (int)GetStat(Stat.Health);
    public override float speed => GetStat(Stat.Speed);
    public override float damage => GetStat(Stat.Damage);
    public override float attackFrequency => GetStat(Stat.AttackFrequency);
    public override int knockbackCount => (int)GetStat(Stat.KnockbackCount);
    public override float range => GetStat(Stat.Range);

    private float GetStat(Stat stat) {
        return GetStat(SaveManager.levels[this], stat);
    }
    public float GetStat(int level, Stat stat) {
        if (upgrades[level - 1].ContainsKey(stat))
            return upgrades[level - 1][stat];
        return stats[stat];
    }
}
