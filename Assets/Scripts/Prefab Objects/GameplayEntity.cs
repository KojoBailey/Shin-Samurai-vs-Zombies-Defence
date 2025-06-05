using UnityEngine;
using System.Collections.Generic;

/* Characters during gameplay, including heroes, allies, and enemies. */
public class GameplayEntity { // Gameplay Entity
    public string entityId;
    public virtual string typeId => "N/A";
    public enum Side { Left, Right }
    public Side allegiance;
    protected bool finishedLoading = false;

    public GameObject obj;
    public Transform transform;
    protected Animation animation;
    protected AnimationHandler animationHandler;
    protected float attackTimer;
    protected float leftBound, rightBound;

    public int direction;
    public float xPos {
        get => (transform != null) ? transform.position.x : 0;
        set => transform.position = new Vector3(value, transform.position.y, transform.position.z);
    }

    public float yVelocity;
    public bool isGettingKnockedBack = false;

    public MeleeWeapon meleeWeapon;
    public RangedWeapon rangedWeapon;

    public float health;
    public float speedModifier = 1.0f; // Affects both animation and movement.
    public float meleeRange;
    public float rangedRange;
    protected int knockedBackCount = 0;
    public bool toDestroy = false;

    public enum State {
        Idle,
        IdleRanged,
        Forward,
        BackPedal,
        BackwardRun,
        Walk,
        MeleeAttack,
        RangedAttack,
        CastMid,
        CastForward,
        PersonalAbility,
        KnockedBack,
        Landing,
        Stunned,
        Die,
        Revive,
        Victory,
    }
    public State currentState;
    protected State m_previousState;

    public bool isDead = false;

    public event System.Action onDeath;
    protected void TriggerOnDeath() => onDeath?.Invoke();

    protected void Prepare() {
        obj.SetActive(false);
        transform = obj.transform;
        animation = obj.GetComponent<Animation>();
        animationHandler = new AnimationHandler(animation);
        if (allegiance == Side.Left)
            direction = 1;
        else direction = -1;

        onDeath += () => isDead = true;
    }

    public void Spawn(float spawnX) {
        xPos = spawnX;
        obj.SetActive(true);
        finishedLoading = true;
    }
    protected void FinishInit() {
        obj.SetActive(true);
        finishedLoading = true;
    }

    public void Update() {
        if (finishedLoading) {
            EntityUpdate();
        }
    }
    protected virtual void EntityUpdate() {
        /* State */
        if (HandleVictoryState() == true) return;
        if (HandleDeathState() == true) return;
        HandleHealthRegen();
        HandleAbilityState();
        HandleKnockbackState();
        HandleTravelState();
        HandleAttackState();

        /* Motion */
        HandleTravelMotion();
        HandleKnockbackMotion();
        HandleStateChangeMotion();
        HandleAttackMotion();
        HandleDeathMotion();
    }

    /* Gameplay State Handling */
    protected virtual void HandleHealthRegen() {}
    protected virtual void HandleAbilityState() {}
    protected virtual void HandleKnockbackState() {}
    protected virtual void HandleTravelState() {
        if (!animation.IsPlaying("Attack01"))
            ChangeState(State.Walk);
        if (xPos <= leftBound || xPos >= rightBound)
            ChangeState(State.Idle);
    }
    protected virtual void HandleAttackState() {
        foreach (GameplayEntity enemy in GameplayManager.instance.entities.Values) {
            if (enemy == null || enemy.allegiance == allegiance || enemy.isDead)
                continue;

            if (IsInMeleeRange(enemy.xPos)) {
                ChangeState(State.MeleeAttack);
                break;
            }
        }
    }
    protected virtual bool HandleDeathState() {
        if (health <= 0)
            ChangeState(State.Die);
        return health <= 0;
    }
    protected virtual bool HandleVictoryState() {
        return false;
    }

    /* Animation and Motion Handling */
    protected virtual void HandleMotion() {}
    protected virtual void HandleTravelMotion() {}
    protected virtual void HandleKnockbackMotion() {
        if (!isGettingKnockedBack && currentState == State.KnockedBack)
            ChangeState(State.Landing);
    }
    protected virtual void HandleStateChangeMotion() {}
    protected virtual void HandleAttackMotion() {}
    protected virtual void HandleDeathMotion() {
        if (currentState == State.Die) {
            if (!animation.IsPlaying("Die")) {
                Object.Destroy(obj);
                toDestroy = true;
                return;
            }
        }
    }

    public void SetEntityId(string _entityId) {
        entityId = _entityId;
        obj.GetComponent<AnimEventAttack>().entityId = _entityId;
    }

    public void ApplyTags(AnimationEventData data) {
        if (GameplayManager.instance.entitiesWithTags.Contains(typeId))
            return;
        foreach (KeyValuePair<string, List<AnimationEventData.Tag>> tags in data.tags) {
            AnimationClip clip = animation[tags.Key].clip;
            foreach (AnimationEventData.Tag tag in tags.Value) {
                AnimationEvent animEvent = new AnimationEvent {
                    functionName = tag.action.ToString(),
                    time = tag.frame / clip.frameRate
                };
                clip.AddEvent(animEvent);
            }
        }
        GameplayManager.instance.entitiesWithTags.Add(typeId);
    }

    protected void ChangeState(State newState) {
        currentState = newState;
    }

    public void SetBounds(float left, float right) {
        leftBound = left;
        rightBound = right;
    }

    public virtual void Damage(float damage) {
        if (isDead) return;
        health -= damage;
        if (health <= 0)
            TriggerOnDeath();
    }
    public virtual void Heal(float damage) {
        health += damage;
    }

    public void SwitchToMelee() {
        meleeWeapon.Show();
        if (rangedWeapon != null)
            rangedWeapon.Hide();
    }
    public void SwitchToRanged() {
        meleeWeapon.Hide();
        if (rangedWeapon != null)
            rangedWeapon.Show();
    }

    public void FireProjectile(GameplayEntity target) {
        if (rangedWeapon != null)
            rangedWeapon.FireProjectile(target);
    }

    public virtual bool IsInMeleeRange(float _x) { return false; }
    public virtual void MeleeHit(GameplayEntity target) {}
    public virtual void MeleeHit(GameplayEntity target, float damage) {}

    protected void ChangeAnimation(string animationId) {
        animation.Play(animationId);
    }
    protected void ChangeAnimation(string animationId, float crossFade) {
        animation.CrossFade(animationId, crossFade);
    }

    public void ChangeSpeed(float _speedModifier) {
        speedModifier = _speedModifier;
        foreach (AnimationState state in animation) {
            state.speed = speedModifier;
        }
    }
};