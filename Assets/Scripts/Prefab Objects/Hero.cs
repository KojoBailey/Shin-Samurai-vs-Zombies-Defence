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

    public bool isDead = false;
    private float healthRegenTimer = 0;

    public enum TravelState { None, Forward, Backward, BackPedal };
    public TravelState travelState;
    private bool isTravelling => travelState != TravelState.None;
    private float backPedalTimer;
    private float xVelocity;

    public enum AttackState { None, RangedHold, Melee, Ranged };
    public AttackState attackState;
    private float meleeAttackTimer;
    private float rangedAttackTimer;

    public enum AbilityState { None, CastForward, CastMid, KatanaSlash };
    public AbilityState abilityState = AbilityState.None;
    private bool isPerformingAbility => abilityState != AbilityState.None;

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
        animationHandler.Play("Idle", true);

        onDeath += HandleDeath;

        FinishInit();
    }

    protected override void EntityUpdate() {
        if (!isDead) {
            HandleAbilityCast();
            HandleTravel();
            HandleAttack();
            HandleIdle();
            HandleHealthRegen();
        }
        animationHandler.Update();
        if (rangedWeapon != null) rangedWeapon.Update();
    }

    protected void HandleAbilityCast() {
        switch (abilityState) {
            case AbilityState.CastForward:
                PlayAbilityAnimation("CastForward");
                break;
            case AbilityState.KatanaSlash:
                PlayAbilityAnimation("AbilityKatanaSlash");
                break;
        }
    }
    private void PlayAbilityAnimation(string animationName) {
        SwitchToMelee();
        animationHandler.Play(animationName, false, 0.1f);
        animationHandler.onAnimationEnd += () => {
            abilityState = AbilityState.None;
        };
    }

    protected void HandleDeath() {
        isDead = true;
        ChangeState(State.Die);
        animationHandler.Play("Die", false, 0.1f);
        data.GetEquippedCostume().audioData.Die();
    }

    protected void HandleTravel() {
        if (isPerformingAbility) {
            travelState = TravelState.None;
        } else {
            HandleTravelInput();
        }
        UpdatePosition();
    }
    private void HandleTravelInput() {
        if (Input.GetKey(KeyCode.D)) {
            ChangeTravelAnimation(TravelState.Forward);
            xVelocity += data.acceleration * Time.deltaTime;
            backPedalTimer = 1;
        } else if (Input.GetKey(KeyCode.A)) {
            if (backPedalTimer <= 0) {
                ChangeTravelAnimation(TravelState.Backward);
                xVelocity -= data.acceleration * 1.2f * Time.deltaTime;
            } else {
                ChangeTravelAnimation(TravelState.BackPedal);
                xVelocity -= data.acceleration * Time.deltaTime;
                backPedalTimer -= Time.deltaTime;
            }
        } else {
            travelState = TravelState.None;
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
            travelState = TravelState.None;
    }
    private void ChangeTravelAnimation(TravelState _travelState) {
        if (travelState != _travelState)
            switch (_travelState) {
                case TravelState.Forward:
                    animationHandler.Play("Forward", true, 0.1f);
                    break;
                case TravelState.BackPedal:
                    animationHandler.Play("BackPedal", true, 0.1f);
                    break;
                case TravelState.Backward:
                    animationHandler.PlaySequence(
                        ("BackPedalTurn", false, 0.1f),
                        ("Backward", true, 0)
                    );
                    break;
            }
        travelState = _travelState;
    }

    protected void HandleAttack() {
        attackState = AttackState.None;
        if (GameplayManager.heroDoNotAttack
            || GameplayManager.instance.waveComplete
            || isTravelling
            || isPerformingAbility
        ) return;

        // Detect enemies and set attackState accordingly.
        foreach (GameplayEntity enemy in GameplayManager.instance.entities.Values) {
            if (enemy == null || enemy.allegiance == allegiance || enemy.currentState == State.Die)
                continue;

            float difference = enemy.xPos - xPos;
            if (allegiance == Side.Right)
                difference *= -1;
            if (difference > 0) {
                if (difference < meleeRange) {
                    attackState = AttackState.Melee;
                    break;
                } else if (difference < rangedRange) {
                    attackState = AttackState.Ranged;
                    break;
                } else if (difference < rangedRange + 1) {
                    attackState = AttackState.RangedHold;
                    break;
                }
            }
        }

        if (attackState == AttackState.Melee) {
            SwitchToMelee();
            if (meleeAttackTimer < 0f)
                meleeAttackTimer = meleeWeapon.data.attackFrequency;
            if (!animation.IsPlaying("Attack01")) {
                if (meleeAttackTimer == meleeWeapon.data.attackFrequency) {
                    animationHandler.Play("Attack01", false, 0.1f);
                } else {
                    animationHandler.Play("Idle", true, 0.1f);
                }
            }
        } else if (attackState == AttackState.Ranged && rangedWeapon != null) {
            SwitchToRanged();
            if (rangedAttackTimer < 0f)
                rangedAttackTimer = rangedWeapon.data.attackFrequency;
            if (!animation.IsPlaying("AttackRanged")) {
                if (rangedAttackTimer == rangedWeapon.data.attackFrequency) {
                    animationHandler.Play("AttackRanged", false, 0.1f);
                } else {
                    animationHandler.Play("IdleRanged", true, 0.1f);
                }
            }
        }
        meleeAttackTimer -= Time.deltaTime;
        rangedAttackTimer -= Time.deltaTime;
    }

    protected void HandleIdle() {
        if (GameplayManager.instance.waveComplete) {
            animationHandler.Play("VictoryLoop", true, 0.1f);
            return;
        }
        if (attackState == AttackState.RangedHold) {
            SwitchToRanged();
            animationHandler.Play("IdleRanged", true, 0.1f);
        } else if (!isTravelling && attackState == AttackState.None && !isPerformingAbility) {
            animationHandler.Play("Idle", true, 0.1f);
        }
    }

    protected override void HandleHealthRegen() {
        healthRegenTimer -= Time.deltaTime;
        if (healthRegenTimer <= 0)
            health += data.healthRegen * Time.deltaTime;
        if (health > data.maxHealth)
            health = data.maxHealth;
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