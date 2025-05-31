using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Animation Event Data", menuName = "Game Data/Animation Event Data")]
public class AnimationEventData : ScriptableObject { // Animation Tag Handler
    public enum AttackType {
        DealDamage,
        FireProjectile,
        ActivateAbility
    }
    [Serializable] public class Tag {
        public int frame;
        public AttackType action;
    };
    [SerializeField] public GenericDictionary<string, List<Tag>> tags;
}