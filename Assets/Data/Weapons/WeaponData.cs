using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game Data/Weapon")]
public class WeaponData : ScriptableObject {
    public enum Style {
        Melee, Ranged
    }

    public string DisplayName;
    public string Desc;
    public Sprite Icon;
    public Style Type;
    public GameObject LeftHandPrefab;
    public GameObject RightHandPrefab;
    public GameObject Projectile;
    public AudioClip[] HitSFX;
    public float Range;

    public void PlayHit() {
        if (HitSFX.Length > 0) {
            SFXManager.Play(HitSFX[Random.Range(0, HitSFX.Length)]);
        }
    }
}
