using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Game Data/Wave")]
public class WaveData : ScriptableObject {
    public int index;
    public string title;
    public enum Stage {
        ZenGarden
    }
    public Stage stage;
    
    [System.Serializable] public class Entry {
        public float delay;
        public EnemyData enemy;
        public int enemyQuanitity;
    }
    public Entry[] entries;

    public static string StageToString(Stage _stage) {
        switch (_stage) {
            case Stage.ZenGarden: return "Zen Garden";
        }
        return "";
    }
}