using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Gate : GameplayEntity {
    private GateData data;
    public override string typeId => data.id;
    
    private HealthBar m_healthBar;

    public Gate(Side _allegiance) {
        allegiance = _allegiance;
    }

    public async Task Init() {
        var handle = Addressables.LoadAssetAsync<GateData>("Data/Allies Gate");
        data = await handle.Task;

        obj = Object.Instantiate(data.GetEquippedCostume().prefab);
        Prepare();
        transform.position = new Vector3(0f, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, 90f * direction, 0f);

        SaveManager.SetLevel(data, 1);
        m_healthBar = new HealthBar(GameplayManager.healthBarPrefab, this, data.health);
        m_healthBar.position = new Vector3(0f, 150f, 70f);
        m_healthBar.scale *= 100;
        health = data.health;

        FinishInit();
    }

    protected override void EntityUpdate() {
        if (HandleDeathState() == true) return;
    }

    protected override void HandleStateChangeMotion() {
        if (currentState != m_previousState) {
            m_previousState = currentState;
            switch (currentState) {
                case State.Idle:
                    ChangeAnimation("Idle");
                    break;
                case State.Die:
                    ChangeAnimation("Die");
                    data.GetEquippedCostume().audioData.Die();
                    GameplayManager.instance.hero.health = 0;
                    break;
            }
        }
    }
}