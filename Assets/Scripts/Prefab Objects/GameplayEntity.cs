using UnityEngine;

/* Characters during gameplay, including heroes, allies, and enemies. */
public class GameplayEntity { // Gameplay Entity
    public string entityId;
    public enum Side { Left, Right }
    public Side allegiance;
    protected bool m_loaded = false;

    public GameObject obj;
    public Transform transform;
    public Animation animation;
    public GameObject wrapperObject;
    public Animation wrapperAnimation;
    protected float m_attackTimer;
    protected float m_leftBound, m_rightBound;

    public float xPos {
        get => transform.position.x;
        set => transform.position = new Vector3(value, transform.position.y, transform.position.z);
    }

    public float yVelocity;
    public float knockbackMeter;
    public bool isGettingKnockedBack = false;

    public MeleeWeapon meleeWeapon;
    public RangedWeapon rangedWeapon;

    public float health;
    public float meleeRange;
    public float rangedRange;
    protected int m_knockedBackCount = 0;
    public bool toDestroy = false;

    // Debug Tools
    protected const bool m_doNotAttack = false;

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
        KnockedBack,
        Landing,
        Stunned,
        Die,
        Revive,
        Victory
    }
    public State currentState;
    protected State m_previousState;

    public void SetEntityId(string newId) {
        entityId = newId;
        if (wrapperAnimation != null)
            wrapperObject.GetComponent<AnimEventAttack>().entityId = entityId;
        else
            obj.GetComponent<AnimEventAttack>().entityId = entityId;
    }

    protected void Prepare() {
        obj.SetActive(false);
        transform = obj.transform;
        animation = obj.GetComponent<Animation>();
        knockbackMeter = 20;
    }

    public virtual void Damage(float damage) {
        health -= damage;
    }
    public virtual void Heal(float damage) {
        health += damage;
    }

    public void FireProjectile(GameplayEntity target) {
        if (currentState == State.RangedAttack)
            rangedWeapon.FireProjectile(target);
    }

    public virtual bool IsInMeleeRange(float _x) { return false; }
    public virtual void MeleeHitSFX() {}
};