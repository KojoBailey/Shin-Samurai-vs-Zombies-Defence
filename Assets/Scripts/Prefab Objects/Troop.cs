using UnityEngine;

public class Troop : GameplayEntity {
    public TroopData data;
    public override string typeId => data.id;

    protected HealthBar healthBar;

    public Troop(TroopData _data, Side _allegiance) {
        data = _data;
        allegiance = _allegiance;

        obj = Object.Instantiate(data.prefab);
        Prepare();
        transform.position = new Vector3(0f, 0f, Random.Range(-0.4f, 0.4f));
        transform.rotation = Quaternion.Euler(0f, 90f * direction, 0f);
        ApplyTags(data.animationEvents);

        // Attach weapon(s).
        if (data.meleeWeapon.prefab != null) {
            meleeWeapon = new MeleeWeapon(data.meleeWeapon, obj);
        }
        if (data.rangedWeapon.prefab != null) {
            rangedWeapon = new RangedWeapon(data.rangedWeapon, obj);
        }

        health = data.health;
        data.audioData.Spawn();
        healthBar = new HealthBar(GameplayManager.healthBarPrefab, this, data.health);
    }

    protected override void EntityUpdate() {
        /* State */
        if (HandleVictoryState() == true) return;
        if (HandleDeathState() == true) return;
        HandleKnockbackState();
        HandleTravelState();
        HandleAttackState();

        /* Motion */
        HandleTravelMotion();
        HandleKnockbackMotion();
        HandleStateChangeMotion();
        HandleAttackMotion();
        HandleDeathMotion();

        healthBar.Update();

        if (rangedWeapon != null)
            rangedWeapon.Update();
    }

    protected override void HandleKnockbackState() {
        for (int i = data.knockbackCount - m_knockedBackCount; i > 0; i--) {
            if (health <= data.health / (data.knockbackCount + 1) * i) {
                ChangeState(State.KnockedBack);
                isGettingKnockedBack = true;
                yVelocity = 3;
                m_knockedBackCount++;
                break;
            }
        }
        if (currentState == State.KnockedBack || (currentState == State.Landing && animation.IsPlaying("Land"))) return;
    }
    protected override void HandleTravelMotion() {
        if (currentState == State.Walk)
            xPos += data.speed * direction * Time.deltaTime;
        else if (isGettingKnockedBack)
            xPos -= direction * Time.deltaTime;

        // Prevent going past stage bounds.
        if (transform.position.x < m_leftBound)
            xPos = m_leftBound;
        if (transform.position.x > m_rightBound)
            xPos = m_rightBound;
    }
    protected override void HandleAttackMotion() {
        if (currentState == State.MeleeAttack) {
            if (m_attackTimer < 0f)
                m_attackTimer = data.attackFrequency;
            if (!animation.IsPlaying("Attack01")) {
                if (m_attackTimer == data.attackFrequency) {
                    ChangeAnimation("Attack01", 0.1f);
                } else {
                    ChangeAnimation("Idle", 0.1f);
                }
            }
            m_attackTimer -= Time.deltaTime;
        } else {
            m_attackTimer = data.attackFrequency;
        }
    }

    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - transform.position.x;
        if (allegiance == Side.Right) distance *= -1;
        return (distance < data.range) && (distance > 0);
    }
    public override void MeleeHit(GameplayEntity target) {
        target.Damage(data.damage);
        if (meleeWeapon != null)
            data.meleeWeapon.PlayHit();
    }
}