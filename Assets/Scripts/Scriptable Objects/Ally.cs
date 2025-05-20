using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAlly", menuName = "Game Data/Ally")]
public class Ally : ScriptableObject {
    public string displayName;
    public string description;
    public int cost;
    public float cooldown;
    public float attackFrequency;

    public GameObject prefab;
    public Sprite icon;
}
