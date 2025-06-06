using UnityEngine;

public class Projectile {
    private GameObject gameObject;
    private Transform transform;
    private GameplayEntity targetEntity;
    private Transform targetTransform;
    private float initialDistance;
    private const float speed = 7;

    private AudioBundle hitAudio;
    private float damage;
    private int direction;

    public bool toDestroy = false;
    private bool initialised = false;

    public Projectile(RangedWeaponData _data, Transform spawnPos, GameplayEntity _targetEntity) {
        if (_targetEntity != null) {
            damage = _data.damage;
            hitAudio = _data.hitAudio;
            gameObject = Object.Instantiate(_data.projectile);
            transform = gameObject.GetComponent<Transform>();
            transform.position = spawnPos.position;
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            targetEntity = _targetEntity;
            targetTransform = targetEntity.transform;
            initialDistance = targetTransform.position.x - transform.position.x;
            direction = -targetEntity.direction;
            initialised = true;
        } else {
            toDestroy = true;
        }
    }
    public Projectile(GameObject prefab, float _damage, AudioBundle _hitAudio, Transform spawnPos, GameplayEntity _targetEntity) {
        if (_targetEntity != null) {
            damage = _damage;
            hitAudio = _hitAudio;
            gameObject = Object.Instantiate(prefab);
            targetEntity = _targetEntity;
            targetTransform = targetEntity.transform;
            direction = -targetEntity.direction;
            transform = gameObject.GetComponent<Transform>();
            transform.position = spawnPos.position;
            transform.rotation = Quaternion.Euler(0f, 90f * direction, 0f);
            initialDistance = (targetTransform.position.x - transform.position.x) * direction;
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
                speed * Time.deltaTime * direction,
                (targetTransform.position.y - transform.position.y + 0.7f) / initialDistance * speed  * Time.deltaTime,
                0
            );
            if ((targetTransform.position.x - transform.position.x) * direction < 0) {
                targetEntity.Damage(damage);
                if (hitAudio != null)
                    SFXManager.Play(hitAudio.GetRandom());
                Object.Destroy(gameObject);
                toDestroy = true;
            }
        }
    }
}