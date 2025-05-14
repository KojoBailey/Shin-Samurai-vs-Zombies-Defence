using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAlly", menuName = "Game Data/Ally")]
public class Ally : ScriptableObject {
    public string DisplayName;
    public string Desc;
    public int Cost;
    public float Cooldown;
    public float AttackFrequency;

    public GameObject Prefab;
    public Sprite Icon;
}
