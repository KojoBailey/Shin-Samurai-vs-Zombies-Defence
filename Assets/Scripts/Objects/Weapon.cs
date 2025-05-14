using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Weapon {
    private string id;
    public WeaponData data;
    private GameObject leftObj, rightObj;

    public Weapon(string weaponId) {
        id = weaponId;
    }
    public Weapon(WeaponData inputData) {
        data = inputData;
    }

    public async Task Init(GameObject heroLink) {
        if (data == null) {
            var handle = Addressables.LoadAssetAsync<WeaponData>($"Data/Weapons/{id}");
            data = await handle.Task;
            if (data == null) {
                Debug.LogError($"Could not find or load weapon of id `{id}`.");
                return;
            }
        }
        if (data.LeftHandPrefab != null) {
            leftObj = Object.Instantiate(data.LeftHandPrefab);
            WeaponAnchor[] handObjs = heroLink.GetComponentsInChildren<WeaponAnchor>();
            foreach (WeaponAnchor handObj in handObjs) {
                if (handObj.Side == WeaponAnchor.Hand.Left) {
                    leftObj.transform.SetParent(handObj.transform, worldPositionStays: false);
                    break;
                }
            }
        }
        if (data.RightHandPrefab != null) {
            rightObj = Object.Instantiate(data.RightHandPrefab);
            WeaponAnchor[] handObjs = heroLink.GetComponentsInChildren<WeaponAnchor>();
            foreach (WeaponAnchor handObj in handObjs) {
                if (handObj.Side == WeaponAnchor.Hand.Right) {
                    rightObj.transform.SetParent(handObj.transform, worldPositionStays: false);
                    break;
                }
            }
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
};