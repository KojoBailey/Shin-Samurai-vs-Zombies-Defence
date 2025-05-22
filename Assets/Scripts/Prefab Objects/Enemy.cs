using UnityEngine;

public class Enemy : GameplayEntity {
    private string m_id;
    public EnemyData data;
    private HealthBar m_healthBar;

    public Enemy(EnemyData _data, Side _allegiance) {
        data = _data;
        allegiance = _allegiance;

        wrapperObject = Object.Instantiate(data.prefabWrapper);
        obj = Object.Instantiate(data.prefab, wrapperObject.transform);
        Prepare();
        transform.position = new Vector3(0f, 0f, Random.Range(-0.4f, 0.4f));
        transform.rotation = Quaternion.Euler(0f, 90f * direction, 0f);

        // Attach weapon.
        if (data.meleeWeaponData != null) {
            meleeWeapon = new MeleeWeapon(data.meleeWeaponData, obj);
            meleeRange = meleeWeapon.data.range;
        }
        if (data.rangedWeaponData != null) {
            rangedWeapon = new RangedWeapon(data.rangedWeaponData, obj);
            rangedRange = rangedWeapon.data.range;
        }

        health = data.health;
        data.audioData.Spawn();
        m_healthBar = new HealthBar(GameplayManager.healthBarPrefab, this, data.health);
    }

    protected override void HandleState() {
        // Handle death.
        if (health <= 0) {
            ChangeState(State.Die);
            return;
        }

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
        if (!animationHandler.attackIsPlaying)
            ChangeState(State.Walk);
        foreach (GameplayEntity enemy in GameplayManager.entities.Values) {
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
                    ChangeAnimation(animationHandler.idle, 0.1f);
                    break;
                case State.Walk:
                    animation.CrossFade(animationHandler.forward, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.forward, 0.1f);
                    break;
                case State.KnockedBack:
                    animation.CrossFade(animationHandler.knockedBack, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.knockedBack, 0.1f);
                    break;
                case State.Landing:
                    animation.CrossFade(animationHandler.land, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.land, 0.1f);
                    break;
                case State.Die:
                    animation.CrossFade(animationHandler.die, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.die, 0.1f);
                    break;
            }
        }

        if (currentState == State.MeleeAttack) {
            if (m_attackTimer < 0f)
                m_attackTimer = data.attackFrequency;
            if (!animationHandler.attackIsPlaying) {
                if (m_attackTimer == data.attackFrequency) {
                    animation.CrossFade(animationHandler.attack, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.attack, 0.1f);
                } else {
                    animation.CrossFade(animationHandler.idle, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.idle, 0.1f);
                }
            }
            m_attackTimer -= Time.deltaTime;
        } else {
            m_attackTimer = data.attackFrequency;
        }

        if (currentState == State.Die) {
            if (!animationHandler.dieIsPlaying) {
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
            meleeWeapon.data.PlayHit();
    }
};