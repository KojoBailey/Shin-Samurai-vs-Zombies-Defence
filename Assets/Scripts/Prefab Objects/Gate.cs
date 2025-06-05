using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Gate : GameplayEntity {
    private GateData data;
    public override string typeId => data.id;
    
    private HealthBar healthBar;

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
        healthBar = new HealthBar(GameplayManager.healthBarPrefab, this, data.health);
        healthBar.position = new Vector3(0f, 150f, 70f);
        healthBar.scale *= 100;
        health = data.health;
        onDeath += HandleDeath;

        FinishInit();
    }

    private void HandleDeath() {
        animationHandler.Play("Collapse", false);
        data.GetEquippedCostume().audioData.Die();
        GameplayManager.instance.hero.Damage(float.MaxValue);
    }

    protected override void EntityUpdate() {
        if (!isDead)
            animationHandler.Play("Idle", true);
        animationHandler.Update();
        healthBar.Update();
    }

    public override void Damage(float damage) {
        base.Damage(damage);
        data.GetEquippedCostume().audioData.Damaged();
    }
}