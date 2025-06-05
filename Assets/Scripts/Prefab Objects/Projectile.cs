using UnityEngine;

public class Projectile {
    private GameObject gameObject;
    private Transform transform;
    private GameplayEntity targetEntity;
    private Transform targetTransform;
    private RangedWeaponData data;
    private float initialDistance;
    private const float speed = 7;

    public bool toDestroy = false;
    private bool initialised = false;

    public Projectile(RangedWeaponData _data, Transform spawnPos, GameplayEntity _targetEntity) {
        if (_targetEntity != null) {
            data = _data;
            gameObject = Object.Instantiate(data.projectile);
            transform = gameObject.GetComponent<Transform>();
            transform.position = spawnPos.position;
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            targetEntity = _targetEntity;
            targetTransform = targetEntity.transform;
            initialDistance = targetTransform.position.x - transform.position.x;
            initialised = true;
        } else {
            toDestroy = true;
        }
    }

    public void Update() {
        if (initialised || !toDestroy) {
            if (targetTransform == null || targetEntity.isDead) {
                Object.Destroy(gameObject);
                toDestroy = true;
                return;
            }
            transform.position += new Vector3(
                speed * Time.deltaTime,
                (targetTransform.position.y - transform.position.y + 0.7f) / initialDistance * speed  * Time.deltaTime,
                0
            );
            if (targetTransform.position.x - transform.position.x < 0) {
                targetEntity.Damage(data.GetStat(RangedWeaponData.Stat.Damage));
                data.PlayHit();
                Object.Destroy(gameObject);
                toDestroy = true;
            }
        }
    }
}