using UnityEngine;

public class Enemy : GameplayEntity {
    private string m_id;
    public EnemyData data;
    private HealthBar m_healthBar;

    public Enemy(EnemyData _data, Side _allegiance) {
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
        m_healthBar = new HealthBar(GameplayManager.healthBarPrefab, this, data.health);
    }

    protected override void HandleState() {
        if (health <= 0) {
            ChangeState(State.Die);
        }
        if (currentState == State.Die) return;
        
        // Handle knockback.
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

        // Handle attacking.
        if (!animation.IsPlaying("Attack01"))
            ChangeState(State.Walk);
        if (transform.position.x <= m_leftBound || transform.position.x >= m_rightBound)
            ChangeState(State.Idle);
        foreach (GameplayEntity enemy in GameplayManager.instance.entities.Values) {
            if (enemy == null || enemy.allegiance == allegiance || enemy.currentState == State.Die)
                continue;

            if (IsInMeleeRange(enemy.xPos)) {
                ChangeState(State.MeleeAttack);
                break;
            }
        }
    }

    protected override void HandleMotion() {
        if (currentState == State.Walk) {
            xPos += data.speed * speedModifier * direction * Time.deltaTime;
        } else if (isGettingKnockedBack) {
            xPos -=  direction * Time.deltaTime;
        }
        

        if (!isGettingKnockedBack && currentState == State.KnockedBack) {
            ChangeState(State.Landing);
        }
        
        if (currentState != m_previousState) {
            m_previousState = currentState;
            switch (currentState) {
                case State.Idle:
                    ChangeAnimation("Idle", 0.1f);
                    break;
                case State.Walk:
                    ChangeAnimation("Forward", 0.1f);
                    break;
                case State.KnockedBack:
                    ChangeAnimation("KnockedBack", 0.1f);
                    break;
                case State.Landing:
                    ChangeAnimation("Land", 0.1f);
                    break;
                case State.Die:
                    ChangeSpeed(1);
                    ChangeAnimation("Die", 0.1f);
                    GameplayManager.instance.enemiesRemaining -= 1;
                    break;
            }
        }

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

        if (currentState == State.Die) {
            if (!animation.IsPlaying("Die")) {
                Object.Destroy(obj);
                toDestroy = true;
                return;
            }
        }

        m_healthBar.Update();
    }

    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - transform.position.x;
        if (allegiance == Side.Right) distance *= -1;
        return (distance <= data.range) && (distance > 0);
    }
    public override void MeleeHit(GameplayEntity target) {
        target.Damage(data.damage);
        if (meleeWeapon != null)
            data.meleeWeapon.PlayHit();
    }
};