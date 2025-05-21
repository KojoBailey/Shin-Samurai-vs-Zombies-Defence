using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Enemy : GameplayEntity {
    private string m_id;
    public EnemyData data;
    private HealthBar m_healthBar;

    public Enemy(string enemyId) {
        m_id = enemyId;
    }

    public async Task Init(float spawnX) {
        var handle = Addressables.LoadAssetAsync<EnemyData>($"Data/Enemies/Zombies/{m_id}");
        data = await handle.Task;
        if (data == null) {
            Debug.LogError($"Could not find or load Enemy of ID \"{m_id}\".");
            return;
        }
        wrapperObject = Object.Instantiate(data.prefabWrapper);
        obj = Object.Instantiate(data.prefab, wrapperObject.transform);
        Prepare();
        transform.position = new Vector3(spawnX, 0f, Random.Range(-0.4f, 0.4f));
        transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        // Attach weapon.
        if (data.meleeWeaponData != null) {
            meleeWeapon = new MeleeWeapon(data.meleeWeaponData);
            await meleeWeapon.Init(obj);
            meleeRange = meleeWeapon.data.range;
        }
        if (data.rangedWeaponData != null) {
            rangedWeapon = new RangedWeapon(data.rangedWeaponData);
            await rangedWeapon.Init(obj);
            rangedRange = rangedWeapon.data.range;
        }

        health = data.health;
        data.audioData.Spawn();
        m_healthBar = new HealthBar(this, data.health);
        await m_healthBar.Init();

        FinishInit();
    }

    protected override void HandleState() {
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
            if (!animationHandler.attackIsPlaying)
                ChangeState(State.Walk);
        }
    }

    protected override void HandleMotion() {
        if (currentState == State.Walk) {
            ChangeX(-1 * data.speed * Time.deltaTime);
        } else if (isGettingKnockedBack) {
            ChangeX(1 * Time.deltaTime);
        }

        if (!isGettingKnockedBack && currentState == State.KnockedBack) {
            ChangeState(State.Landing);
        }
        
        if (currentState != m_previousState) {
            m_previousState = currentState;
            switch (currentState) {
                case State.Idle:
                    animation.CrossFade(animationHandler.idle, 0.1f);
                    break;
                case State.Walk:
                    animation.CrossFade(animationHandler.forward, 0.1f);
                    break;
                case State.KnockedBack:
                    animation.CrossFade(animationHandler.knockedBack, 0.1f);
                    break;
                case State.Landing:
                    animation.CrossFade(animationHandler.land, 0.1f);
                    break;
                case State.Die:
                    animation.CrossFade(animationHandler.die, 0.1f);
                    break;
            }
        }

        if (currentState == State.MeleeAttack) {
            if (m_attackTimer < 0f)
                m_attackTimer = data.attackFrequency;
            if (!animationHandler.attackIsPlaying) {
                if (m_attackTimer == data.attackFrequency) {
                    animation.CrossFade(animationHandler.attack, 0.1f);
                } else {
                    animation.CrossFade(animationHandler.idle, 0.1f);
                }
            }
            m_attackTimer -= Time.deltaTime;
        } else {
            m_attackTimer = data.attackFrequency;
        }

        if (currentState == State.Die) {
            if (!animationHandler.dieIsPlaying) {
                Object.Destroy(obj);
                toDestroy = true;
                return;
            }
        }

        m_healthBar.Update();
    }

    private bool InRangeOf(float target) {
        float distance = transform.position.x - target;
        return (distance < data.range) && (distance > 0); // Includes checking if in front or behind.
    }

    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - transform.position.x;
        if (allegiance == Side.Right) distance *= -1;
        return (distance < data.range) && (distance > 0);
    }
};