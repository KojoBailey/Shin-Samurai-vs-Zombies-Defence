using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        obj = Object.Instantiate(data.GetEquippedCostume().prefab);

        SaveManager.SetLevel(data, 1);
        SaveManager.SetLevel(data.meleeWeaponData, 1);
        SaveManager.SetLevel(data.rangedWeaponData, 1);
        Prepare();
        transform.position = new Vector3(spawnX, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, 90f * direction, 0f);
        ApplyTags(data.animationEvents);

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

        health = data.maxHealth;

        m_isTurning = false;
        ChangeAnimation("Idle");
        FinishInit();
    }

    protected override void HandleState() {
        if (GameplayManager.instance.waveComplete) {
            ChangeState(State.Victory);
            return;
        }

        // Health-related stuff
        if (health <= 0) {
            ChangeState(State.Die);
        }
        if (currentState == State.Die) return;
        m_healthRegenTimer -= Time.deltaTime;
        if (m_healthRegenTimer <= 0) {
            health += data.healthRegen * Time.deltaTime;
        }
        if (health > data.maxHealth)
            health = data.maxHealth;

        if (abilityStatus == AbilityStatus.CastForward) {
            ChangeState(State.CastForward);
            return;
        } if (abilityStatus == AbilityStatus.KatanaSlash) {
            ChangeState(State.PersonalAbility);
            return;
        }

        // Attack based on distance.
        attackStatus = AttackStatus.None;
        foreach (GameplayEntity enemy in GameplayManager.instance.entities.Values) {
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
            if (attackStatus == AttackStatus.Melee && !GameplayManager.heroDoNotAttack) {
                ChangeState(State.MeleeAttack);
            } else if (attackStatus == AttackStatus.Ranged && !GameplayManager.heroDoNotAttack) {
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

        if (transform.position.x <= m_leftBound || transform.position.x >= m_rightBound)
            ChangeState(State.Idle);
    }

    protected override void HandleMotion() {
        if (currentState != m_previousState) {
            m_previousState = currentState;
            switch (currentState) {
                case State.Idle:
                    SwitchToMelee();
                    ChangeAnimation("Idle", 0.1f);
                    break;
                case State.IdleRanged:
                    SwitchToRanged();
                    ChangeAnimation("IdleRanged", 0.1f);
                    break;
                case State.Forward:
                    SwitchToMelee();
                    ChangeAnimation("Forward", 0.1f);
                    break;
                case State.BackPedal:
                    SwitchToMelee();
                    ChangeAnimation("BackPedal", 0.1f);
                    break;
                case State.BackwardRun:
                    SwitchToMelee();
                    m_isTurning = true;
                    ChangeAnimation("BackPedalTurn");
                    break;
                case State.MeleeAttack:
                    SwitchToMelee();
                    break;
                case State.RangedAttack:
                    SwitchToRanged();
                    break;
                case State.CastForward:
                    SwitchToMelee();
                    ChangeAnimation("CastForward", 0.1f);
                    break;
                case State.PersonalAbility:
                    if (abilityStatus == AbilityStatus.KatanaSlash) {
                        SwitchToMelee();
                        ChangeAnimation("AbilityKatanaSlash", 0.1f);
                    }
                    break;
                case State.Die:
                    ChangeAnimation("Die", 0.1f);
                    data.GetEquippedCostume().audioData.Die();
                    break;
                case State.Victory:
                    ChangeAnimation("Victory", 0.1f);
                    break;
            }
        }
        if (currentState == State.Die) return;

        if (currentState == State.MeleeAttack) {
            if (m_meleeAttackTimer < 0f)
                m_meleeAttackTimer = meleeWeapon.data.attackFrequency;
            if (!animation.IsPlaying("Attack01")) {
                if (m_meleeAttackTimer == meleeWeapon.data.attackFrequency) {
                    ChangeAnimation("Attack01", 0.1f);
                } else {
                    ChangeAnimation("Idle", 0.1f);
                }
            }
        }
        m_meleeAttackTimer -= Time.deltaTime;

        if (currentState == State.RangedAttack && rangedWeapon != null) {
            if (m_rangedAttackTimer < 0f)
                m_rangedAttackTimer = rangedWeapon.data.attackFrequency;
            if (!animation.IsPlaying("AttackRanged")) {
                if (m_rangedAttackTimer == rangedWeapon.data.attackFrequency) {
                    ChangeAnimation("AttackRanged", 0.1f);
                } else {
                    ChangeAnimation("IdleRanged", 0.1f);
                }
            }
        }
        m_rangedAttackTimer -= Time.deltaTime;

        if (currentState == State.CastForward) {
            if (!animation.IsPlaying("CastForward")) {
                abilityStatus = AbilityStatus.None;
                ChangeState(State.Idle);
            }
        }

        if (currentState == State.PersonalAbility) {
            if (abilityStatus == AbilityStatus.KatanaSlash) {
                if (!animation.IsPlaying("AbilityKatanaSlash")) {
                    abilityStatus = AbilityStatus.None;
                    ChangeState(State.Idle);
                }
            }
        }

        if (m_isTurning == true && !animation.IsPlaying("BackPedalTurn") && currentState == State.BackwardRun) {
            ChangeAnimation("Backward");
            m_isTurning = false;
        }

        GameplayManager.instance.heroX = transform.position.x;
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