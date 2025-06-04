using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Hero : GameplayEntity {
    private string heroId;
    public HeroData data;
    public override string typeId => data.id;

    private AnimationHandler animationHandler;

    public bool lockControls;

    private float xVelocity;

    public enum TravelState { None, Forward, Backward, BackPedal };
    public TravelState travelState;
    private TravelState previousTravelStatus;
    public enum AttackStatus { None, RangedHold, Melee, Ranged };
    public AttackStatus attackStatus;
    private float m_meleeAttackTimer;
    private float m_rangedAttackTimer;
    private float healthRegenTimer = 0;
    private float backPedalTimer;
    public enum AbilityStatus { None, CastForward, CastMid, KatanaSlash };
    public AbilityStatus abilityStatus = AbilityStatus.None;
    private enum AbilityProgress {
        NotPlaying,
        Started,
        InProgress
    }
    private AbilityProgress abilityProgress = AbilityProgress.NotPlaying;

    private bool isTravelling;
    private bool isPerformingAbility;
    public bool isDead = false;

    public Hero(string _heroId) {
        heroId = _heroId;
    }

    public async Task Init(float spawnX) {
        var handle = Addressables.LoadAssetAsync<HeroData>($"Data/Heroes/{heroId}");
        data = await handle.Task;
        if (data == null) {
            Debug.LogError($"Could not find or load Hero of ID \"{heroId}\".");
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

        animationHandler = new AnimationHandler(animation);
        animationHandler.Reset("Idle", true);

        onDeath += HandleDeath;

        FinishInit();
    }

    protected void HandleDeath() {
        isDead = true;
        ChangeState(State.Die);
        lockControls = true;
        animationHandler.Reset("Die", false, 0.1f);
        data.GetEquippedCostume().audioData.Die();
    }
    
    protected void HandleTravel() {
        if (!lockControls) HandleTravelInput();
        UpdatePosition();
    }
    private void HandleTravelInput() {
        if (Input.GetKey(KeyCode.D)) {
            isTravelling = true;
            xVelocity += data.acceleration * Time.deltaTime;
            animationHandler.Reset("Forward", true, 0.1f);
            backPedalTimer = 1;
        } else if (Input.GetKey(KeyCode.A)) {
            isTravelling = true;
            if (backPedalTimer <= 0) {
                xVelocity -= data.acceleration * 1.2f * Time.deltaTime;
                    animationHandler.Reset("BackPedalTurn", false, 0.1f, "BackwardRun");
                    animationHandler.Queue("Backward", true, 0, "BackwardRun");
            } else {
                xVelocity -= data.acceleration * Time.deltaTime;
                backPedalTimer -= Time.deltaTime;
                animationHandler.Reset("BackPedal", true, 0.1f);
            }
        } else {
            isTravelling = false;
            backPedalTimer = 1;
        }
    }
    private void UpdatePosition() {
        xPos += xVelocity * direction;
        xVelocity *= 0.90f;

        // Keep within the stage bounds.
        if (transform.position.x < m_leftBound)
            xPos = m_leftBound;
        if (transform.position.x > m_rightBound)
            xPos = m_rightBound;
        if (transform.position.x <= m_leftBound || transform.position.x >= m_rightBound)
            isTravelling = false;
    }


    protected void HandleAttack() {
        attackStatus = AttackStatus.None;
        if (GameplayManager.heroDoNotAttack) return;

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

        if (attackStatus == AttackStatus.Melee) {
            SwitchToMelee();
            if (m_meleeAttackTimer < 0f)
                m_meleeAttackTimer = meleeWeapon.data.attackFrequency;
            if (!animation.IsPlaying("Attack01")) {
                if (m_meleeAttackTimer == meleeWeapon.data.attackFrequency) {
                    ChangeAnimation("Attack01", 0.1f);
                } else {
                    ChangeAnimation("Idle", 0.1f);
                }
            }
        } else if (attackStatus == AttackStatus.Ranged && rangedWeapon != null) {
            SwitchToRanged();
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
        m_meleeAttackTimer -= Time.deltaTime;
        m_rangedAttackTimer -= Time.deltaTime;
    }
    protected override void HandleHealthRegen() {
        healthRegenTimer -= Time.deltaTime;
        if (healthRegenTimer <= 0)
            health += data.healthRegen * Time.deltaTime;
        if (health > data.maxHealth)
            health = data.maxHealth;
    }

    protected void HandleIdle() {
        if (GameplayManager.instance.waveComplete) {
            animationHandler.Reset("VictoryLoop", true, 0.1f);
            return;
        }
        if (attackStatus == AttackStatus.RangedHold) {
            SwitchToRanged();
            animationHandler.Reset("IdleRanged", true, 0.1f);
        } else if (!isTravelling && attackStatus == AttackStatus.None && !isPerformingAbility) {
            animationHandler.Reset("Idle", true, 0.1f);
        }
    }

    protected override void EntityUpdate() {
        if (!isDead) {
            if (!GameplayManager.instance.waveComplete) {
                HandleAttack();
            }
            HandleAbilityCast();
            HandleTravel();
            HandleIdle();
            HandleHealthRegen();
        }
        animationHandler.Update();
        if (rangedWeapon != null) rangedWeapon.Update();
    }

    protected void HandleAbilityCast() {
        switch (abilityStatus) {
            case AbilityStatus.CastForward:
                PlayAbilityAnimation("CastForward");
                break;
            case AbilityStatus.KatanaSlash:
                PlayAbilityAnimation("AbilityKatanaSlash");
                break;
        }
    }

    private void PlayAbilityAnimation(string animationName) {
        SwitchToMelee();
        isPerformingAbility = true;
        lockControls = true;
        animationHandler.Reset(animationName, false, 0.1f);
        animationHandler.onAnimationEnd += () => {
            isPerformingAbility = false;
            lockControls = false;
            abilityStatus = AbilityStatus.None;
        };
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
        if (isDead) return;
        health -= damage;
        healthRegenTimer = data.healthRegenDelay;
        if (health <= 0)
            TriggerOnDeath();
    }
    public override void Heal(float damage) {
        health += damage;
        healthRegenTimer = 0;
    }
};