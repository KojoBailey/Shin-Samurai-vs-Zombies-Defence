using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;

public class RangedWeapon { // Ranged Weapon
    public RangedWeaponData data;
    private GameObject leftObj, rightObj;
    private Transform projectileSpawn;

    private List<Projectile> projectiles;

    public RangedWeapon(RangedWeaponData _data, GameObject heroLink) {
        data = _data;

        if (data.leftHandPrefab != null) {
            leftObj = Object.Instantiate(data.leftHandPrefab);
            WeaponAnchor[] handObjs = heroLink.GetComponentsInChildren<WeaponAnchor>();
            foreach (WeaponAnchor handObj in handObjs) {
                if (handObj.side == WeaponAnchor.Side.Left) {
                    leftObj.transform.SetParent(handObj.transform, worldPositionStays: false);
                    break;
                }
            }
        }
        if (data.rightHandPrefab != null) {
            rightObj = Object.Instantiate(data.rightHandPrefab);
            WeaponAnchor[] handObjs = heroLink.GetComponentsInChildren<WeaponAnchor>();
            foreach (WeaponAnchor handObj in handObjs) {
                if (handObj.side == WeaponAnchor.Side.Right) {
                    projectileSpawn = handObj.transform;
                    rightObj.transform.SetParent(projectileSpawn, worldPositionStays: false);
                    break;
                }
            }
        }

        projectiles = new();
        SaveManager.SetLevel(data, 1);
    }
    public RangedWeapon(TroopData.RangedWeapon troopWeapon, GameObject troopLink) {
        if (troopWeapon.hand == WeaponAnchor.Side.Left) {
            leftObj = Object.Instantiate(troopWeapon.prefab);
            WeaponAnchor[] handObjs = troopLink.GetComponentsInChildren<WeaponAnchor>();
            foreach (WeaponAnchor handObj in handObjs) {
                if (handObj.side == WeaponAnchor.Side.Left) {
                    leftObj.transform.SetParent(handObj.transform, worldPositionStays: false);
                    break;
                }
            }
        } else if (troopWeapon.hand == WeaponAnchor.Side.Right) {
            rightObj = Object.Instantiate(troopWeapon.prefab);
            WeaponAnchor[] handObjs = troopLink.GetComponentsInChildren<WeaponAnchor>();
            foreach (WeaponAnchor handObj in handObjs) {
                if (handObj.side == WeaponAnchor.Side.Right) {
                    rightObj.transform.SetParent(handObj.transform, worldPositionStays: false);
                    break;
                }
            }
        }

        projectiles = new();
    }

    public void Update() {
        foreach (Projectile projectile in projectiles) {
            if (!projectile.toDestroy)
                projectile.Update();
        }
        for (int i = 0; i < projectiles.Count; i++) {
            if (projectiles[i].toDestroy)
                projectiles.RemoveAt(i);
        }
    }

    public void Show() {
        if (leftObj != null)
            leftObj.SetActive(true);
        if (rightObj != null)
            rightObj.SetActive(true);
    }
    public void Hide() {
        if (leftObj != null)
            leftObj.SetActive(false);
        if (rightObj != null)
            rightObj.SetActive(false);
    }

    public void FireProjectile(GameplayEntity target) {
        Projectile projectile = new Projectile(data, projectileSpawn, target);
        projectiles.Add(projectile);
    }
};