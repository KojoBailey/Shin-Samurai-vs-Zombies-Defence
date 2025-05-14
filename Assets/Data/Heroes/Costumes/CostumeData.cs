using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCostume", menuName = "Game Data/Costume")]
public class CostumeData : ScriptableObject {
    public string DisplayName;
    public GameObject Prefab;
    public Material MaterialOverride;
    public EntityAudioData AudioData;
}
