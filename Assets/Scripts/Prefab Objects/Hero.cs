using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Hero : GameplayEntity {
    private string heroId;
    public HeroData data;
    public override string typeId => data.id;

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

        // Attach weapons.
        if (data.meleeWeaponData != null)
            meleeWeapon = new MeleeWeapon(data.meleeWeaponData, obj);
        if (data.rangedWeaponData != null)
            rangedWeapon = new RangedWeapon(data.rangedWeaponData, obj);
        SwitchToMelee();

        health = data.maxHealth;
        onDeath += HandleDeath;

        FinishInit();
    }

    private void HandleDeath() {
        ChangeSpeed(1);
        animationHandler.Play("Die", false, 0.1f);
        data.GetEquippedCostume().audioData.Die();
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

    private void HandleAbilityCast() {
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
        if (animationName == animationHandler.currentAnimation) return;
        SwitchToMelee();
        animationHandler.Play(animationName, false, 0.1f);
        animationHandler.onAnimationEnd += () => {
            abilityState = AbilityState.None;
        };
    }

    private void HandleTravel() {
        if (isPerformingAbility)
            travelState = TravelState.None;
        else HandleTravelInput();
        
        // Import to always update due to acceleration physics.
        UpdatePosition();
    }
    private void HandleTravelInput() {
        if (Input.GetKey(KeyCode.D)) {
            ChangeTravelAnimation(TravelState.Forward);
            xVelocity = data.speed;
            backPedalTimer = 1;
        } else if (Input.GetKey(KeyCode.A)) {
            if (backPedalTimer <= 0) {
                ChangeTravelAnimation(TravelState.Backward);
                xVelocity = -data.speed * 1.2f;
            } else {
                ChangeTravelAnimation(TravelState.BackPedal);
                xVelocity = -data.speed;
                backPedalTimer -= Time.deltaTime;
            }
        } else {
            travelState = TravelState.None;
            backPedalTimer = 1;
        }
    }
    private void UpdatePosition() {
        if (travelState != TravelState.None)
            xPos += xVelocity * direction * Time.deltaTime;
        
        // Keep within the stage bounds.
        if (xPos < leftBound)
            xPos = leftBound;
        if (xPos > rightBound)
            xPos = rightBound;
        if (xPos <= leftBound || xPos >= rightBound)
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

    private void HandleAttack() {
        attackState = AttackState.None;
        if (GameplayManager.heroDoNotAttack
            || GameplayManager.instance.waveComplete
            || isTravelling
            || isPerformingAbility
        ) return;

        // Detect enemies and set attackState accordingly.
        foreach (GameplayEntity enemy in GameplayManager.instance.GetEntities()) {
            if (enemy == null || enemy.allegiance == allegiance || enemy.isDead)
                continue;

            float difference = enemy.xPos - xPos;
            if (allegiance == Side.Right)
                difference *= -1;
            if (difference > 0) {
                if (difference < meleeWeapon.data.range && !enemy.isFlying) {
                    attackState = AttackState.Melee;
                    break;
                } else if (difference < rangedWeapon.data.range) {
                    attackState = AttackState.Ranged;
                } else if (difference < rangedWeapon.data.range + 1 && attackState != AttackState.Ranged) {
                    attackState = AttackState.RangedHold;
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

    private void HandleIdle() {
        if (isTravelling) return;

        if (GameplayManager.instance.waveComplete) {
            animationHandler.Play("VictoryLoop", true, 0.1f);
            abilityState = AbilityState.None;
            attackState = AttackState.None;
            return;
        }
        
        if (attackState == AttackState.RangedHold) {
            SwitchToRanged();
            animationHandler.Play("IdleRanged", true, 0.1f);
        } else if (attackState == AttackState.None && !isPerformingAbility) {
            animationHandler.Play("Idle", true, 0.1f);
        }
    }

    private void HandleHealthRegen() {
        healthRegenTimer -= Time.deltaTime;
        if (healthRegenTimer <= 0)
            health += data.healthRegen * Time.deltaTime;
        if (health > data.maxHealth)
            health = data.maxHealth;
    }

    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - xPos;
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
        if (isDead) return;
        health += damage;
        healthRegenTimer = 0;
    }
};