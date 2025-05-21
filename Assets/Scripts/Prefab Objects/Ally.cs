using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Ally : GameplayEntity {
    private string m_id;
    private AllyData data;
    private HealthBar m_healthBar;

    public Ally(string enemyId) {
        m_id = enemyId;
    }

    public async Task Init(float spawnX) {
        var handle = Addressables.LoadAssetAsync<AllyData>($"Data/Allies/{m_id}");
        data = await handle.Task;
        if (data == null) {
            Debug.LogError($"Could not find or load Ally of ID `{m_id}`.");
            return;
        }
        obj = Object.Instantiate(data.prefabWrapper);
        Prepare();
        animationHandler = obj.GetComponent<AnimationHandler>();
        animationHandler.LoadClips();
        transform.position = new Vector3(spawnX, 0f, Random.Range(-0.4f, 0.4f));
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        SaveManager.SetLevel(data, 1);

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

        obj.SetActive(true);
        m_loaded = true;
    }

    public void Update() {
        if (m_loaded) {
            HandleState();
            HandleMotion();
            if (rangedWeapon != null)
                rangedWeapon.Update();
        }
    }

    public virtual void HandleState() {
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

        if (currentState == State.KnockedBack || (currentState == State.Landing && animationHandler.landIsPlaying)) return;

        if (!animationHandler.attackIsPlaying)
            ChangeState(State.Walk);
    }
    public virtual void HandleMotion() {
        if (transform.position.x <= m_leftBound || transform.position.x >= m_rightBound) {
            ChangeState(State.Idle);
        }

        if (!isGettingKnockedBack && currentState == State.KnockedBack) {
            ChangeState(State.Landing);
        }

        if (currentState != m_previousState) {
            m_previousState = currentState;
            switch (currentState) {
                case State.Idle:
                    animation.CrossFade(animationHandler.idle.name, 0.1f);
                    break;
                case State.Walk:
                    animation.CrossFade(animationHandler.forward.name, 0.1f);
                    break;
                case State.KnockedBack:
                    animation.CrossFade(animationHandler.knockedBack.name, 0.1f);
                    break;
                case State.Landing:
                    animation.CrossFade(animationHandler.land.name, 0.1f);
                    break;
                case State.Die:
                    animation.CrossFade(animationHandler.die.name, 0.1f);
                    break;
            }
        }

        if (currentState == State.MeleeAttack) {
            if (m_attackTimer < 0f)
                m_attackTimer = data.meleeWeaponData.attackFrequency;
            if (!animationHandler.attackIsPlaying) {
                if (m_attackTimer == data.meleeWeaponData.attackFrequency) {
                    animation.CrossFade(animationHandler.attack.name, 0.1f);
                } else {
                    animation.CrossFade(animationHandler.idle.name, 0.1f);
                }
            }
            m_attackTimer -= Time.deltaTime;
        } else {
            m_attackTimer = data.meleeWeaponData.attackFrequency;
        }

        if (currentState == State.Die) {
            if (!animationHandler.dieIsPlaying) {
                Object.Destroy(obj);
                toDestroy = true;
                return;
            }
        }

        if (currentState == State.Walk) {
            if (allegiance == Side.Left)
                ChangeX(1 * data.speed * Time.deltaTime);
            else ChangeX(-1 * data.speed * Time.deltaTime);
        } else if (isGettingKnockedBack) {
            ChangeX(1 * Time.deltaTime);
        }
        if (transform.position.x < m_leftBound)
            SetX(m_leftBound);
        if (transform.position.x > m_rightBound)
            SetX(m_rightBound);
    }
}