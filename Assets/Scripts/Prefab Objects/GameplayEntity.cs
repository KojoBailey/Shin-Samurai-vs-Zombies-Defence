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
    public AnimationHandler animationHandler;
    public Animation wrapperAnimation;
    protected float m_attackTimer;
    protected float m_leftBound, m_rightBound;

    public int direction;
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
    public float speedModifier = 1.0f; // Affects both animation and movement.
    public float meleeRange;
    public float rangedRange;
    protected int m_knockedBackCount = 0;
    public bool toDestroy = false;

    // Debug Tools
    public bool doNotAttack = false;

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

    protected void Prepare() {
        obj.SetActive(false);
        transform = obj.transform;
        animation = obj.GetComponent<Animation>();
        wrapperAnimation = wrapperObject.GetComponent<Animation>();
        animationHandler = obj.GetComponent<AnimationHandler>();
        animationHandler.LoadClips();
        knockbackMeter = 20;
        if (allegiance == Side.Left)
            direction = 1;
        else direction = -1;
    }

    public void Spawn(float spawnX) {
        xPos = spawnX;
        obj.SetActive(true);
        m_loaded = true;
    }
    protected void FinishInit() {
        obj.SetActive(true);
        m_loaded = true;
    }

    public virtual void Update() {
        if (m_loaded) {
            HandleState();
            if (transform.position.x <= m_leftBound || transform.position.x >= m_rightBound) {
                SwitchToMelee();
                ChangeState(State.Idle);
            }
            HandleMotion();
            if (transform.position.x < m_leftBound)
                xPos = m_leftBound;
            if (transform.position.x > m_rightBound)
                xPos = m_rightBound;
            if (rangedWeapon != null)
                rangedWeapon.Update();
        }
    }

    protected virtual void HandleState() {}
    protected virtual void HandleMotion() {}

    public void SetEntityId(string newId) {
        entityId = newId;
        wrapperObject.GetComponent<AnimEventAttack>().entityId = entityId;
    }

    protected void ChangeState(State newState) {
        currentState = newState;
    }

    public void SetBounds(float left, float right) {
        m_leftBound = left;
        m_rightBound = right;
    }

    public virtual void Damage(float damage) {
        health -= damage;
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
        if (currentState == State.RangedAttack)
            rangedWeapon.FireProjectile(target);
    }

    public virtual bool IsInMeleeRange(float _x) { return false; }
    public virtual void MeleeHit(GameplayEntity target) {}

    protected void ChangeAnimation(string animationId) {
        animation.Play(animationId);
        wrapperAnimation.Play(animationId);
    }
    protected void ChangeAnimation(string animationId, float crossFade) {
        animation.CrossFade(animationId, crossFade);
        wrapperAnimation.CrossFade(animationId, crossFade);
    }

    public void ChangeSpeed(float _speedModifier) {
        speedModifier = _speedModifier;
        foreach (AnimationState state in animation) {
            state.speed = speedModifier;
        }
        foreach (AnimationState state in wrapperAnimation) {
            state.speed = speedModifier;
        }
    }
};