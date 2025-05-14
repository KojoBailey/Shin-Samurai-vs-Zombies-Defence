using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Hero : GameplayEntity {
    private string heroId;
    private HeroData data;

    private Weapon meleeWeapon;
    private Weapon rangedWeapon;
    private bool rangedAttacking;

    public enum State {
        Idle,
        Forward,
        BackPedal,
        BackwardRun,
        MeleeAttack,
        RangedAttack,
        CastMid,
        CastForward,
        KnockedBack,
        Stunned,
        Die,
        Revive,
        Victory
    }
    public State CurrentState;
    private State previousState;
    private bool isTurning;

    private const float attackFrequency = 0.7f;
    private float attackCooldown;

    private float velocity;
    private const float acceleration = 0.35f;
    private float backPedalTimer;

    private float leftBound, rightBound;

    public Vector3 position {
        get {
            return transform.position;
        }
    }

    public Hero(string _heroId) {
        heroId = _heroId;
    }

    public async Task Init(float spawnX) {
        var handle = Addressables.LoadAssetAsync<HeroData>($"Data/Heroes/{heroId}");
        data = await handle.Task;
        if (data == null) {
            Debug.LogError($"Could not find or load Hero of ID `{heroId}`.");
            return;
        }
        wrapperObj = Object.Instantiate(data.Wrapper);
        obj = Object.Instantiate(data.GetEquippedCostume().Prefab, wrapperObj.transform);

        Prepare();
        wrapperAnimation = wrapperObj.GetComponent<Animation>();
        transform.position = new Vector3(spawnX, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        // Attach weapon.
        rangedWeapon = new Weapon(data.RangedWeaponData);
        await rangedWeapon.Init(obj);
        meleeWeapon = new Weapon(data.MeleeWeaponData);
        await meleeWeapon.Init(obj);
        SwitchWeapon(WeaponData.Style.Melee);

        Health = 100;
        isTurning = false;
        loaded = true;
    }

    public void Update() {
        if (loaded) {
            HandleInput();
            HandleMotion();
            HandleAnimation();
        }
    }

    private void HandleInput() {
        if (!Input.GetKey(KeyCode.A)) {
            backPedalTimer = 0;
        }
        if (Input.GetKey(KeyCode.D)) {
            ChangeState(State.Forward);
        } else if (Input.GetKey(KeyCode.A)) {
            if (backPedalTimer > 1.0) {
                ChangeState(State.BackwardRun);
            } else {
                ChangeState(State.BackPedal);
            }
        } else {
            ChangeState(State.Idle);
        }

        if (Input.GetKey(KeyCode.K)) {
            ChangeState(State.MeleeAttack);
        }
    }

    private void HandleAnimation() {
        if (transform.position.x <= leftBound || transform.position.x >= rightBound) {
            SwitchWeapon(WeaponData.Style.Melee);
            ChangeState(State.Idle);
        }

        if (CurrentState != previousState) {
            previousState = CurrentState;
            switch (CurrentState) {
                case State.Idle:
                    if (rangedAttacking)
                        animation.CrossFade("IdleRanged", 0.1f);
                    else
                        animation.CrossFade("Idle", 0.1f);
                    break;
                case State.Forward:
                    SwitchWeapon(WeaponData.Style.Melee);
                    animation.CrossFade("RunForward", 0.1f);
                    break;
                case State.BackPedal:
                    SwitchWeapon(WeaponData.Style.Melee);
                    animation.CrossFade("BackPedal", 0.1f);
                    break;
                case State.BackwardRun:
                    SwitchWeapon(WeaponData.Style.Melee);
                    isTurning = true;
                    animation.Play("BackPedalTurn");
                    break;
                case State.MeleeAttack:
                    SwitchWeapon(WeaponData.Style.Melee);
                    break;
            }
        }

        if (CurrentState == State.MeleeAttack) {
            if (attackCooldown < 0f)
                attackCooldown = attackFrequency;
            if (!animation.IsPlaying("Attack01") && !animation.IsPlaying("Attack02") && !animation.IsPlaying("Attack03")) {
                if (attackCooldown == attackFrequency) {
                    int rand = Random.Range(1, 4);
                    animation.CrossFade($"Attack0{rand}", 0.1f);
                    wrapperAnimation.Play($"Attack0{rand}");
                } else {
                    animation.CrossFade("Idle", 0.1f);
                    wrapperAnimation.Play("Idle");
                }
            }
            attackCooldown -= Time.deltaTime;
        } else {
            attackCooldown = attackFrequency;
        }

        if (isTurning == true && !animation.IsPlaying("BackPedalTurn")) {
            animation.Play("RunBackward");
            isTurning = false;
        }

        obj.SetActive(true);
    }

    private void HandleMotion() {
        switch (CurrentState) {
            case State.Forward:
                velocity += acceleration * Time.deltaTime;
                break;
            case State.BackwardRun:
                if (isTurning == false) {
                    velocity -= acceleration * 1.2f * Time.deltaTime;
                    break;
                }
                goto case State.BackPedal;
            case State.BackPedal:
                velocity -= acceleration * Time.deltaTime;
                backPedalTimer += Time.deltaTime;
                break;
        }

        ChangeX(velocity);
        velocity *= 0.90f;
        if (transform.position.x < leftBound)
            SetX(leftBound);
        if (transform.position.x > rightBound)
            SetX(rightBound);

        GameplayManager.HeroX = transform.position.x;
    }

    private void ChangeState(State newState) {
        CurrentState = newState;
    }

    public void SetBounds(float left, float right) {
        leftBound = left;
        rightBound = right;
    }

    public void ChangeX(float x) {
        transform.position += new Vector3(x, 0, 0);
    }
    public void SetX(float x) {
        transform.position = new Vector3(x, 0, 0);
    }

    public void SwitchWeapon(WeaponData.Style type) {
        if (type == WeaponData.Style.Melee) {
            meleeWeapon.Show();
            rangedWeapon.Hide();
        } else if (type == WeaponData.Style.Ranged) {
            meleeWeapon.Hide();
            rangedWeapon.Show();
        }
    }

    public override float GetMeleeDamage() {
        return 10;
    }
    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - transform.position.x;
        if (Allegiance == Side.Right) distance *= -1;
        return (distance < meleeWeapon.data.Range) && (distance > 0);
    }
    public override void MeleeAttackSFX() {
        meleeWeapon.data.PlayHit();
    }
};