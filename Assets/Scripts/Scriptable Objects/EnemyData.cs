using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy")]
public class EnemyData : TroopData {
    [SerializeField] private GameObject m_prefab;
    public override GameObject prefab => m_prefab;
    [SerializeField] private EntityAudioData m_audioData;
    public override EntityAudioData audioData => m_audioData;

    public bool isBoss;
}
