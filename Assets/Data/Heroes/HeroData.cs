using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewHero", menuName = "Game Data/Hero")]
public class HeroData : ScriptableObject {
    public string DisplayName;
    public string Desc;
    public Sprite Icon;
    public GameObject Wrapper;
    public CostumeData[] Costumes;
    public WeaponData MeleeWeaponData;
    public WeaponData RangedWeaponData;

    public CostumeData GetEquippedCostume() {
        CostumeData costume = Costumes[SaveManager.EquippedCostumes["Samurai"]];
        if (costume.MaterialOverride)
            costume.Prefab.GetComponent<SkinnedMeshRenderer>().material = costume.MaterialOverride;
        return costume;
    }
}
