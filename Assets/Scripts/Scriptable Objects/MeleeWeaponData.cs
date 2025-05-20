using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Game Data/Melee Weapon")]
public class MeleeWeaponData : ScriptableObject, IUpgradable {
    public string displayName;
    public string description;
    public AudioClip[] hitAudio;

    public enum Stat {
        Range,
        Damage,
        AttackFrequency
    }
    public GenericDictionary<Stat, float> stats;
    [System.Serializable] public class Upgrade {
        public Sprite icon;
        public GameObject leftHandPrefab;
        public GameObject rightHandPrefab;
        public GenericDictionary<Stat, float> stats;
    }
    public List<Upgrade> upgrades;

    public GameObject leftHandPrefab {
        get => upgrades[SaveManager.levels[this] - 1].leftHandPrefab;
    }
    public GameObject rightHandPrefab {
        get => upgrades[SaveManager.levels[this] - 1].rightHandPrefab;
    }
    public float range {
        get => GetStat(Stat.Range);
    }
    public float damage {
        get => GetStat(Stat.Damage);
    }
    public float attackFrequency {
        get => GetStat(Stat.AttackFrequency);
    }

    public float GetStat(Stat stat) {
        return GetStat(SaveManager.levels[this], stat);
    }
    public float GetStat(int level, Stat stat) {
        if (upgrades[level - 1].stats.ContainsKey(stat))
            return upgrades[level - 1].stats[stat];
        return stats[stat];
    }

    public void PlayHit() {
        if (hitAudio.Length > 0) {
            SFXManager.Play(hitAudio[Random.Range(0, hitAudio.Length)]);
        }
    }
}
