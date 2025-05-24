using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Hero : GameplayEntity {
    private string m_heroId;
    public HeroData data;

    private float m_xVelocity;

    public enum AttackStatus { None, RangedHold, Melee, Ranged };
    public AttackStatus attackStatus;
    private float m_meleeAttackTimer;
    private float m_rangedAttackTimer;
    private float m_healthRegenTimer = 0;
    private float m_backPedalTimer;
    private bool m_isTurning;
    public enum AbilityStatus { None, CastForward, CastMid, KatanaSlash };
    public AbilityStatus abilityStatus = AbilityStatus.None;

    public Hero(string _heroId) {
        m_heroId = _heroId;
    }

    public async Task Init(float spawnX) {
        var handle = Addressables.LoadAssetAsync<HeroData>($"Data/Heroes/{m_heroId}");
        data = await handle.Task;
        if (data == null) {
            Debug.LogError($"Could not find or load Hero of ID \"{m_heroId}\".");
            return;
        }
        wrapperObject = Object.Instantiate(data.prefabWrapper);
        obj = Object.Instantiate(data.GetEquippedCostume().prefab, wrapperObject.transform);

        SaveManager.SetLevel(data, 1);
        SaveManager.SetLevel(data.meleeWeaponData, 1);
        SaveManager.SetLevel(data.rangedWeaponData, 1);
        Prepare();
        transform.position = new Vector3(spawnX, 0f, 0f);
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
        SwitchToMelee();

        health = data.health;

        m_isTurning = false;
        animation.Play(animationHandler.idle);
        FinishInit();
    }

    protected override void HandleState() {
        // Health-related stuff
        if (health <= 0) {
            ChangeState(State.Die);
        }
        if (currentState == State.Die) return;
        m_healthRegenTimer -= Time.deltaTime;
        if (m_healthRegenTimer <= 0) {
            health += data.healthRegen * Time.deltaTime;
        }
        if (health > data.health)
            health = data.health;

        if (abilityStatus == AbilityStatus.CastForward) {
            ChangeState(State.CastForward);
            return;
        } if (abilityStatus == AbilityStatus.KatanaSlash) {
            ChangeState(State.PersonalAbility);
            return;
        }

        // Attack based on distance.
        attackStatus = AttackStatus.None;
        foreach (GameplayEntity enemy in GameplayManager.entities.Values) {
            if (enemy == null || enemy.allegiance == allegiance || enemy.currentState == State.Die)
                continue;

            float difference = enemy.xPos - xPos;
            if (allegiance == Side.Right)
                difference *= -1;
            if (difference > 0) {
                if (difference < meleeRange) {
                    attackStatus = AttackStatus.Melee;
                    break;
                } else if (difference < rangedRange) {
                    attackStatus = AttackStatus.Ranged;
                    break;
                } else if (difference < rangedRange + 1) {
                    attackStatus = AttackStatus.RangedHold;
                    break;
                } else {
                    attackStatus = AttackStatus.None;
                }
            }
        }

        // Input-related stuff
        if (Input.GetKey(KeyCode.D)) {
                m_backPedalTimer = 0;
                ChangeState(State.Forward);
        } else if (Input.GetKey(KeyCode.A)) {
            if (m_backPedalTimer > 1.0) {
                ChangeState(State.BackwardRun);
            } else {
                ChangeState(State.BackPedal);
            }
        } else {
            m_backPedalTimer = 0;
            if (attackStatus == AttackStatus.Melee && !doNotAttack) {
                ChangeState(State.MeleeAttack);
            } else if (attackStatus == AttackStatus.Ranged && !doNotAttack) {
                ChangeState(State.RangedAttack);
            } else if (attackStatus == AttackStatus.RangedHold) {
                ChangeState(State.IdleRanged);
            } else {
                ChangeState(State.Idle);
            }
        }

        switch (currentState) {
            case State.Forward:
                m_xVelocity += data.acceleration * Time.deltaTime;
                break;
            case State.BackwardRun:
                if (m_isTurning == false) {
                    m_xVelocity -= data.acceleration * 1.2f * Time.deltaTime;
                    break;
                }
                goto case State.BackPedal;
            case State.BackPedal:
                m_xVelocity -= data.acceleration * Time.deltaTime;
                m_backPedalTimer += Time.deltaTime;
                break;
        }

        xPos += m_xVelocity * direction;
        m_xVelocity *= 0.90f;
    }

    protected override void HandleMotion() {
        if (currentState != m_previousState) {
            m_previousState = currentState;
            switch (currentState) {
                case State.Idle:
                    SwitchToMelee();
                    animation.CrossFade(animationHandler.idle, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.idle, 0.1f);
                    break;
                case State.IdleRanged:
                    SwitchToRanged();
                    animation.CrossFade(animationHandler.idleRanged, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.idleRanged, 0.1f);
                    break;
                case State.Forward:
                    SwitchToMelee();
                    animation.CrossFade(animationHandler.forward, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.forward, 0.1f);
                    break;
                case State.BackPedal:
                    SwitchToMelee();
                    animation.CrossFade(animationHandler.backpedal, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.backpedal, 0.1f);
                    break;
                case State.BackwardRun:
                    SwitchToMelee();
                    m_isTurning = true;
                    animation.Play(animationHandler.backpedalTurn);
                    wrapperAnimation.Play(animationHandler.backpedalTurn);
                    break;
                case State.MeleeAttack:
                    SwitchToMelee();
                    break;
                case State.RangedAttack:
                    SwitchToRanged();
                    break;
                case State.CastForward:
                    SwitchToMelee();
                    ChangeAnimation(animationHandler.castForward, 0.1f);
                    break;
                case State.PersonalAbility:
                    if (abilityStatus == AbilityStatus.KatanaSlash) {
                        SwitchToMelee();
                        ChangeAnimation(animationHandler.GetAbility("KatanaSlash"), 0.1f);
                    }
                    break;
                case State.Die:
                    animation.CrossFade(animationHandler.die, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.die, 0.1f);
                    data.GetEquippedCostume().audioData.Die();
                    break;
            }
        }
        if (currentState == State.Die) return;

        if (currentState == State.MeleeAttack) {
            if (m_meleeAttackTimer < 0f)
                m_meleeAttackTimer = meleeWeapon.data.attackFrequency;
            if (!animationHandler.attackIsPlaying) {
                if (m_meleeAttackTimer == meleeWeapon.data.attackFrequency) {
                    animation.CrossFade(animationHandler.attack, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.attack, 0.1f);
                } else {
                    animation.CrossFade(animationHandler.idle, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.idle, 0.1f);
                }
            }
        }
        m_meleeAttackTimer -= Time.deltaTime;

        if (currentState == State.RangedAttack && rangedWeapon != null) {
            if (m_rangedAttackTimer < 0f)
                m_rangedAttackTimer = rangedWeapon.data.attackFrequency;
            if (!animation.IsPlaying(animationHandler.attackRanged)) {
                if (m_rangedAttackTimer == rangedWeapon.data.attackFrequency) {
                    animation.CrossFade(animationHandler.attackRanged, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.attackRanged, 0.1f);
                } else {
                    animation.CrossFade(animationHandler.idleRanged, 0.1f);
                    wrapperAnimation.CrossFade(animationHandler.idleRanged, 0.1f);
                }
            }
        }
        m_rangedAttackTimer -= Time.deltaTime;

        if (currentState == State.CastForward) {
            if (!animationHandler.castForwardIsPlaying) {
                abilityStatus = AbilityStatus.None;
                ChangeState(State.Idle);
            }
        }

        if (currentState == State.PersonalAbility) {
            if (abilityStatus == AbilityStatus.KatanaSlash) {
                if (!animationHandler.IsAbilityPlaying("KatanaSlash")) {
                    abilityStatus = AbilityStatus.None;
                    ChangeState(State.Idle);
                }
            }
        }

        if (m_isTurning == true && !animationHandler.backpedalTurnIsPlaying && currentState == State.BackwardRun) {
            animation.Play(animationHandler.backward);
            wrapperAnimation.Play(animationHandler.backward);
            m_isTurning = false;
        }

        GameplayManager.heroX = transform.position.x;
    }

    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - transform.position.x;
        distance *= direction;
        return (distance < meleeWeapon.data.range) && (distance > 0);
    }
    public override void MeleeHit(GameplayEntity target) {
        MeleeHit(target, data.meleeWeaponData.damage);
    }
    public override void MeleeHit(GameplayEntity target, float damage) {
        target.Damage(damage);
        meleeWeapon.data.PlayHit();
    }

    public override void Damage(float damage) {
        health -= damage;
        m_healthRegenTimer = data.healthRegenDelay;
    }
    public override void Heal(float damage) {
        health += damage;
        m_healthRegenTimer = 0;
    }
};