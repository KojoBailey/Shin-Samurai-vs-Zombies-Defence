using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Gate {
    public GameObject obj;
    public GameplayEntity.Side allegiance;
    private HealthBar m_healthBar;

    public Gate(GameplayEntity.Side _allegiance) {
        allegiance = _allegiance;
    }

    public async Task Init() {
        var handle = Addressables.InstantiateAsync("Prefabs/Sacred Gate");
        obj = await handle.Task;
    }
}