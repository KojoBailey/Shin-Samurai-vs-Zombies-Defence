using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;

public class RangedWeapon { // Ranged Weapon
    private string m_id;
    public RangedWeaponData data;
    private GameObject m_leftObj, m_rightObj;
    private Transform m_projectileSpawn;

    private List<Projectile> m_projectiles;

    public RangedWeapon(string weaponId) {
        m_id = weaponId;
    }
    public RangedWeapon(RangedWeaponData inputData) {
        data = inputData;
    }

    public async Task Init(GameObject heroLink) {
        if (data == null) {
            var handle = Addressables.LoadAssetAsync<RangedWeaponData>($"Data/Weapons/{m_id}");
            data = await handle.Task;
            if (data == null) {
                Debug.LogError($"Could not find or load weapon of m_id `{m_id}`.");
                return;
            }
        }
        if (data.leftHandPrefab != null) {
            m_leftObj = Object.Instantiate(data.leftHandPrefab);
            WeaponAnchor[] handObjs = heroLink.GetComponentsInChildren<WeaponAnchor>();
            foreach (WeaponAnchor handObj in handObjs) {
                if (handObj.side == WeaponAnchor.Side.Left) {
                    m_leftObj.transform.SetParent(handObj.transform, worldPositionStays: false);
                    break;
                }
            }
        }
        if (data.rightHandPrefab != null) {
            m_rightObj = Object.Instantiate(data.rightHandPrefab);
            WeaponAnchor[] handObjs = heroLink.GetComponentsInChildren<WeaponAnchor>();
            foreach (WeaponAnchor handObj in handObjs) {
                if (handObj.side == WeaponAnchor.Side.Right) {
                    m_projectileSpawn = handObj.transform;
                    m_rightObj.transform.SetParent(m_projectileSpawn, worldPositionStays: false);
                    break;
                }
            }
        }

        m_projectiles = new();
        SaveManager.SetLevel(data, 1);
    }

    public void Update() {
        foreach (Projectile projectile in m_projectiles) {
            if (!projectile.toDestroy)
                projectile.Update();
        }
        for (int i = 0; i < m_projectiles.Count; i++) {
            if (m_projectiles[i].toDestroy)
                m_projectiles.RemoveAt(i);
        }
    }

    public void Show() {
        if (m_leftObj != null)
            m_leftObj.SetActive(true);
        if (m_rightObj != null)
            m_rightObj.SetActive(true);
    }
    public void Hide() {
        if (m_leftObj != null)
            m_leftObj.SetActive(false);
        if (m_rightObj != null)
            m_rightObj.SetActive(false);
    }

    public void FireProjectile(GameplayEntity target) {
        Projectile projectile = new Projectile(data, m_projectileSpawn, target);
        m_projectiles.Add(projectile);
    }
};