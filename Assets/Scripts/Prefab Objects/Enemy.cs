using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Enemy : GameplayEntity {
    private string id;
    private EnemyData data;
    private HealthBar healthBar;

    private float attackCooldown;

    private float leftBound, rightBound;

    public Vector3 position {
        get {
            return transform.position;
        }
    }

    public Enemy(string enemyId) {
        id = enemyId;
    }

    public async Task Init(float spawnX) {
        var handle = Addressables.LoadAssetAsync<EnemyData>($"Data/Enemies/Zombies/{id}");
        data = await handle.Task;
        if (data == null) {
            Debug.LogError($"Could not find or load Enemy of id `{id}`.");
            return;
        }
        obj = Object.Instantiate(data.prefab);
        Prepare();
        transform.position = new Vector3(spawnX, 0f, Random.Range(-0.4f, 0.4f));
        transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        // Attach weapon.
        meleeWeapon = new MeleeWeapon(data.meleeWeaponData);
        // await weapon.Init(obj);

        health = data.health;
        data.audioData.Spawn();
        healthBar = new HealthBar(this, data.health);
        await healthBar.Init();

        m_loaded = true;
    }

    public void Update() {
        if (m_loaded) {
            HandleLogic();
            HandleMotion();
            HandleAnimation();
            healthBar.Update();
            obj.SetActive(true);
        }
    }

    private void HandleLogic() {
        if (health <= 0) {
            ChangeState(State.Die);
            return;
        }

        for (int i = data.knockbackCount - m_knockedBackCount; i > 0; i--) {
            if (health <= data.health / (data.knockbackCount + 1) * i) {
                ChangeState(State.KnockedBack);
                isGettingKnockedBack = true;
                yVelocity = 3;
                m_knockedBackCount++;
                break;
            }
        }

        if (currentState == State.KnockedBack || (currentState == State.Landing && animation.IsPlaying("Land"))) return;

        if (InRangeOf(GameplayManager.heroX)) {
            ChangeState(State.MeleeAttack);
        } else {
            if (!animation.IsPlaying("Attack01"))
                ChangeState(State.Walk);
        }
    }

    private void HandleAnimation() {
        if (transform.position.x <= leftBound || transform.position.x >= rightBound) {
            ChangeState(State.Idle);
        }

        if (!isGettingKnockedBack && currentState == State.KnockedBack) {
            ChangeState(State.Landing);
        }

        if (currentState != m_previousState) {
            m_previousState = currentState;
            switch (currentState) {
                case State.Idle:
                    animation.CrossFade("Idle", 0.1f);
                    break;
                case State.Walk:
                    animation.CrossFade("Walk", 0.1f);
                    break;
                case State.KnockedBack:
                    animation.CrossFade("KnockedBack", 0.1f);
                    break;
                case State.Landing:
                    animation.CrossFade("Land", 0.1f);
                    break;
                case State.Die:
                    animation.CrossFade("Die", 0.1f);
                    break;
            }
        }

        if (currentState == State.MeleeAttack) {
            if (attackCooldown < 0f)
                attackCooldown = data.GetStat(EnemyData.Stat.AttackFrequency);
            if (!animation.IsPlaying("Attack01")) {
                if (attackCooldown == data.GetStat(EnemyData.Stat.AttackFrequency)) {
                    animation.CrossFade("Attack01", 0.1f);
                } else {
                    animation.CrossFade("Idle", 0.1f);
                }
            }
            attackCooldown -= Time.deltaTime;
        } else {
            attackCooldown = data.GetStat(EnemyData.Stat.AttackFrequency);
        }

        if (currentState == State.Die) {
            if (!animation.IsPlaying("Die")) {
                Object.Destroy(obj);
                toDestroy = true;
                return;
            }
        }
    }

    private void HandleMotion() {
        if (currentState == State.Walk) {
            ChangeX(-1 * data.GetStat(EnemyData.Stat.Speed) * Time.deltaTime);
        } else if (isGettingKnockedBack) {
            ChangeX(1 * Time.deltaTime);
        }
        if (transform.position.x < leftBound)
            SetX(leftBound);
        if (transform.position.x > rightBound)
            SetX(rightBound);
    }

    private void ChangeState(State newState) {
        currentState = newState;
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

    private bool InRangeOf(float target) {
        float distance = transform.position.x - target;
        return (distance < data.GetStat(EnemyData.Stat.Range)) && (distance > 0); // Includes checking if in front or behind.
    }

    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - transform.position.x;
        if (allegiance == Side.Right) distance *= -1;
        return (distance < data.GetStat(EnemyData.Stat.Range)) && (distance > 0);
    }
};