using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Troop Weapon", menuName = "Game Data/Troop Weapon")]
public class TroopWeaponData : ScriptableObject, IUpgradable {
    public string id;
    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;
    public AudioClip[] hitAudio;

    public void PlayHit() {
        if (hitAudio.Length > 0) {
            SFXManager.Play(hitAudio[Random.Range(0, hitAudio.Length)]);
        }
    }
}
