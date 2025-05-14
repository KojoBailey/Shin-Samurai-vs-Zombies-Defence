using UnityEngine;

public class WeaponAnchor : MonoBehaviour {
    public enum Hand {
        Left, Right
    }

    [SerializeField] public Hand Side;
}
