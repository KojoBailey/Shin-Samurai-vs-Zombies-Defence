using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Game Data/Wave")]
public class WaveData : ScriptableObject {
    public int index;
    public string title;
    
    [System.Serializable] public class Entry {
        public float delay;
        public EnemyData enemy;
        public int enemyQuanitity;
    }
    public Entry[] entries;
}