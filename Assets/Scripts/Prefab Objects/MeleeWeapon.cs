using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class MeleeWeapon { // Melee Weapon
    private string m_id;
    public MeleeWeaponData data;
    private GameObject m_leftObj, m_rightObj;

    public MeleeWeapon(MeleeWeaponData inputData, GameObject heroLink) {
        data = inputData;

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
                    m_rightObj.transform.SetParent(handObj.transform, worldPositionStays: false);
                    break;
                }
            }
        }

        SaveManager.SetLevel(data, 1);
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
};