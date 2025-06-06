using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy")]
public class EnemyData : TroopData {
    [SerializeField] private GameObject m_prefab;
    [SerializeField] private Material materialOverride;
    public override GameObject prefab {
        get {
            if (materialOverride)
                m_prefab.GetComponent<SkinnedMeshRenderer>().material = materialOverride;
            return m_prefab;
        }
    }
    [SerializeField] private EntityAudioData m_audioData;
    public override EntityAudioData audioData => m_audioData;

    public bool isBoss;
}
