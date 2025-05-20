using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Ally : GameplayEntity {
    private string m_id;
    private AllyData data;
    private HealthBar m_healthBar;

    public Ally(string enemyId) {
        m_id = enemyId;
    }

    public async Task Init(float spawnX) {
        var handle = Addressables.LoadAssetAsync<AllyData>($"Data/Allies/Samurai/{m_id}");
        data = await handle.Task;
        if (data == null) {
            Debug.LogError($"Could not find or load Enemy of m_id `{m_id}`.");
            return;
        }
        obj = Object.Instantiate(data.prefabWrapper);
        Prepare();
        transform.position = new Vector3(spawnX, 0f, Random.Range(-0.4f, 0.4f));
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        // Attach weapon.
        if (data.meleeWeaponData != null) {
            meleeWeapon = new MeleeWeapon(data.meleeWeaponData);
            await meleeWeapon.Init(obj);
            meleeRange = meleeWeapon.data.range;
        }
        if (data.rangedWeaponData != null) {
            rangedWeapon = new RangedWeapon(data.rangedWeaponData);
            await rangedWeapon.Init(obj);
            rangedRange = rangedWeapon.data.range;
        }

        health = data.health;
        data.audioData.Spawn();
        m_healthBar = new HealthBar(this, data.health);
        await m_healthBar.Init();

        m_loaded = true;
    }
}