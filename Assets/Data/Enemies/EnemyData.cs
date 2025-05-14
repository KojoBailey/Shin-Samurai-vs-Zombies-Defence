using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy")]
public class EnemyData : ScriptableObject {
    public string DisplayName;
    public string Desc;
    public Sprite Icon;
    public GameObject Prefab;
    public WeaponData WeaponData;

    public EntityAudioData AudioData;

    public bool Boss;
    public float Health;
    public float Speed;
    public bool Flying;
    public bool Knockbackable;
    public float MeleeRange;
    public float MeleeDamage;
    public float AttackFrequency;
}
