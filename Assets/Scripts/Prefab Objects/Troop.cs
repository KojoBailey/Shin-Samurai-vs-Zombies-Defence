using UnityEngine;

public class Troop : GameplayEntity {
    public TroopData data;
    public override string typeId => data.id;
    protected virtual bool isAlly => false;

    protected HealthBar healthBar;

    private bool isAttacking;

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
        onDeath += HandleDeath;
        if (isAlly)
            GameplayManager.instance.onWaveComplete += HandleVictory;
        else
            GameplayManager.instance.hero.onDeath += HandleVictory;

        data.audioData.Spawn();
        healthBar = new HealthBar(GameplayManager.healthBarPrefab, this, data.health);
    }

    protected virtual void HandleDeath() {
        ChangeSpeed(1);
        animationHandler.Play("Die", false, 0.1f);
        animationHandler.onAnimationEnd += () => {
            Object.Destroy(obj);
            toDestroy = true;
        };
        data.audioData.Die();
    }

    private void HandleVictory() {
        if (animation["VictoryLoop"] != null)
            animationHandler.Play("VictoryLoop", true, 0.1f);
        else
            animationHandler.Play("Idle", true, 0.1f);
    }

    protected override void EntityUpdate() {
        if (!isDead && !GameplayManager.instance.waveComplete && !GameplayManager.instance.defeated) {
            HandleAttack();
            HandleTravel();
        }
        animationHandler.Update();
        healthBar.Update();
        if (rangedWeapon != null) rangedWeapon.Update();
    }

    private void HandleAttack() {
        if (animation.IsPlaying("Attack01") || animation.IsPlaying("AttackRanged")) {
            isAttacking = true;
        } else {
            isAttacking = false;
            foreach (GameplayEntity enemy in GameplayManager.instance.entities.Values) {
                if (enemy == null || enemy.allegiance == allegiance || enemy.isDead)
                    continue;

                if (IsInMeleeRange(enemy.xPos)) {
                    isAttacking = true;
                    break;
                }
            }
        }

        if (isAttacking) {
            if (attackTimer < 0f)
                attackTimer = data.attackFrequency;
            if (data.isMeleeAttacker) {
                if (!animation.IsPlaying("Attack01")) {
                    if (attackTimer == data.attackFrequency) {
                        animationHandler.Play("Attack01", false, 0.1f);
                    } else {
                        animationHandler.Play("Idle", true, 0.1f);
                    }
                }
            } else if (data.isRangedAttacker) {
                if (!animation.IsPlaying("AttackRanged")) {
                    if (attackTimer == data.attackFrequency) {
                        animationHandler.Play("AttackRanged", false, 0.1f);
                    } else {
                        animationHandler.Play("Idle", true, 0.1f);
                    }
                }
            }
            attackTimer -= Time.deltaTime;
        } else {
            attackTimer = data.attackFrequency;
        }
    }

    private void HandleTravel() {
        if (isGettingKnockedBack) {
            xPos -= direction * Time.deltaTime;
        } else if (!isAttacking) {
            xPos += data.speed * speedModifier * direction * Time.deltaTime;
            animationHandler.Play("Forward", true, 0.1f);
        }

        // Prevent going past stage bounds.
        if (xPos < leftBound)
            xPos = leftBound;
        if (xPos > rightBound)
            xPos = rightBound;
        if (isGettingKnockedBack) return;
        if ((xPos <= leftBound || xPos >= rightBound) && !isGettingKnockedBack)
            animationHandler.Play("Idle", true, 0.1f);
    }

    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - xPos;
        if (allegiance == Side.Right) distance *= -1;
        return (distance < data.range) && (distance > 0);
    }
    public override void MeleeHit(GameplayEntity target) {
        target.Damage(data.damage);
        if (meleeWeapon != null)
            data.meleeWeapon.PlayHit();
    }

    public override void Damage(float damage) {
        if (isDead) return;
        health -= damage;
        if (health <= 0) {
            TriggerOnDeath();
            return;
        }
        CheckKnockback();
    }
    private void CheckKnockback() {
        for (int i = data.knockbackCount - knockedBackCount; i > 0; i--) {
            if (health <= data.health / (data.knockbackCount + 1) * i) {
                isGettingKnockedBack = true;
                yVelocity = 3;
                knockedBackCount++;
                animationHandler.PlaySequence(
                    ("KnockedBack", false, 0.1f),
                    ("Land", false, 0.1f)
                );
                break;
            }
        }
    }

    public override void FireProjectile(GameplayEntity target) {
        rangedWeapon.FireProjectile(
            data.rangedWeapon.prefab,
            data.damage,
            data.rangedWeapon.hitAudio,
            target
        );
    }
}