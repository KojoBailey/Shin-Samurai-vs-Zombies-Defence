using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGate", menuName = "Game Data/Gate")]
public class GateData : ScriptableObject, IUpgradable {
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;
    public CostumeData[] costumes;

    public CostumeData GetEquippedCostume() {
        CostumeData costume = costumes[SaveManager.equippedCostumes[id]];
        if (costume.material) {
            SkinnedMeshRenderer[] meshes = costume.prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer mesh in meshes)
                mesh.material = costume.material;
        }
        return costume;
    }

    public enum Stat {
        UpgradeCost,
        Health
    }
    public GenericDictionary<Stat, float>[] upgrades;

    public int upgradeCost {
        get => (int)GetStat(Stat.UpgradeCost);
    }
    public float health {
        get => GetStat(Stat.Health);
    }

    public float GetStat(Stat stat) {
        return GetStat(SaveManager.levels[this], stat);
    }
    public float GetStat(int level, Stat stat) {
        return upgrades[level - 1][stat];
    }
}
