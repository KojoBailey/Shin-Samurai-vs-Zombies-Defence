using UnityEngine;

public class Projectile {
    private GameObject m_object;
    private Transform m_transform;
    private GameplayEntity m_target;
    private Transform m_targetTransform;
    private RangedWeaponData m_data;
    private float m_initialDistance;
    private const float m_speed = 7;

    public bool toDestroy = false;
    private bool m_initialised = false;

    public Projectile(RangedWeaponData _data, Transform spawnPos, GameplayEntity targetEntity) {
        m_data = _data;
        m_object = Object.Instantiate(m_data.projectile);
        m_transform = m_object.GetComponent<Transform>();
        m_transform.position = spawnPos.position;
        m_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        m_target = targetEntity;
        m_targetTransform = targetEntity.transform;
        m_initialDistance = m_targetTransform.position.x - m_transform.position.x;
        m_initialised = true;
    }

    public void Update() {
        if (m_initialised || !toDestroy) {
            if (m_targetTransform == null || m_target.currentState == GameplayEntity.State.Die) {
                Object.Destroy(m_object);
                toDestroy = true;
                return;
            }
            m_transform.position += new Vector3(
                m_speed * Time.deltaTime,
                (m_targetTransform.position.y - m_transform.position.y + 0.7f) / m_initialDistance * m_speed  * Time.deltaTime,
                0
            );
            if (m_targetTransform.position.x - m_transform.position.x < 0) {
                m_target.Damage(m_data.GetStat(RangedWeaponData.Stat.Damage));
                m_data.PlayHit();
                Object.Destroy(m_object);
                toDestroy = true;
            }
        }
    }
}