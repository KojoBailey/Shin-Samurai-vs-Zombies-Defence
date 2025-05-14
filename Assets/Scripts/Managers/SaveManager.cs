using UnityEngine;
using System.Collections.Generic;

public class SaveManager {
    public static Dictionary<string, int> EquippedCostumes { get; } = new Dictionary<string, int>();

    public static void EquipCostume(string heroId, int costumeIndex) {
        EquippedCostumes[heroId] = costumeIndex;
    }
};