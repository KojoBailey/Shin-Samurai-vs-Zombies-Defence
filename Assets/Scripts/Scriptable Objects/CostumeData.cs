using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCostume", menuName = "Game Data/Costume")]
public class CostumeData : ScriptableObject { // Costume Data
    public string displayName;
    public GameObject prefab;
    public Material material;
    public EntityAudioData audioData;
}
