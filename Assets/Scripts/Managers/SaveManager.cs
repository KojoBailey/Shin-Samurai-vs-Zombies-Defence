using UnityEngine;
using System.Collections.Generic;

/* Manages the storage of persistent - not temporary - save data. */
public class SaveManager { // Save Manager
    public static string selectedHero = "Samurai";
    public static Dictionary<IUpgradable, int> levels { get; } = new Dictionary<IUpgradable, int>();

    public static void SetLevel(IUpgradable item, int level) {
        levels[item] = level;
    }

    public static Dictionary<string, int> equippedCostumes { get; } = new Dictionary<string, int>();

    public static void EquipCostume(string heroId, int costumeIndex) {
        equippedCostumes[heroId] = costumeIndex;
    }
};