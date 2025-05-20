using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Weapon", menuName = "Game Data/Ranged Weapon")]
public class RangedWeaponData : ScriptableObject, IUpgradable {
    public string displayName;
    public string description;
    public Sprite icon;
    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;
    public GameObject projectile;
    public AudioClip[] hitAudio;

    public enum Stat {
        Range,
        Damage,
        AttackFrequency
    }
    public GenericDictionary<Stat, float> stats;
    public GenericDictionary<Stat, float>[] upgrades;

    public float range {
        get {
            return GetStat(Stat.Range);
        }
    }
    public float damage {
        get {
            return GetStat(Stat.Damage);
        }
    }
    public float attackFrequency {
        get {
            return GetStat(Stat.AttackFrequency);
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

    public void PlayHit() {
        if (hitAudio.Length > 0) {
            SFXManager.Play(hitAudio[Random.Range(0, hitAudio.Length)]);
        }
    }
}
