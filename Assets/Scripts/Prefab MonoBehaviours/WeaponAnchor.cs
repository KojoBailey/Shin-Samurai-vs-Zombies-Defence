using UnityEngine;

/* Attach to where weapons go in an entity prefab. */
public class WeaponAnchor : MonoBehaviour { // Weapon Anchor
    public enum Side {
        Left, Right
    }

    [SerializeField] public Side side;
}
