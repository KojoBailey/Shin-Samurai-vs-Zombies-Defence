using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Hero : GameplayEntity {
    private string m_heroId;
    public HeroData data;

    private float m_xVelocity;

    public GameplayManager.AttackStatus attackStatus;
    private float m_meleeAttackTimer;
    private float m_rangedAttackTimer;
    private float m_healthRegenTimer = 0;
    private float m_backPedalTimer;
    private bool m_isTurning;

    public Hero(string _heroId) {
        m_heroId = _heroId;
    }

    public async Task Init(float spawnX) {
        var handle = Addressables.LoadAssetAsync<HeroData>($"Data/Heroes/{m_heroId}");
        data = await handle.Task;
        if (data == null) {
            Debug.LogError($"Could not find or load Hero of ID \"{m_heroId}\".");
            return;
        }
        wrapperObject = Object.Instantiate(data.prefabWrapper);
        obj = Object.Instantiate(data.GetEquippedCostume().prefab, wrapperObject.transform);

        SaveManager.SetLevel(data, 1);
        SaveManager.SetLevel(data.meleeWeaponData, 1);
        SaveManager.SetLevel(data.rangedWeaponData, 1);
        Prepare();
        wrapperAnimation = wrapperObject.GetComponent<Animation>();
        transform.position = new Vector3(spawnX, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);

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
        SwitchToMelee();

        health = data.health;

        m_isTurning = false;
        m_loaded = true;
    }

    public void Update() {
        if (m_loaded) {
            HandleLogic();
            HandleMotion();
            HandleAnimation();
            if (rangedWeapon != null)
                rangedWeapon.Update();
        }
    }

    private void HandleLogic() {
        // Health-related stuff
        if (health <= 0) {
            ChangeState(State.Die);
        }
        if (currentState == State.Die) return;
        m_healthRegenTimer -= Time.deltaTime;
        if (m_healthRegenTimer <= 0) {
            health += data.healthRegen * Time.deltaTime;
        }
        if (health > data.health)
            health = data.health;

        // Input-related stuff
        if (Input.GetKey(KeyCode.D)) {
                m_backPedalTimer = 0;
                ChangeState(State.Forward);
        } else if (Input.GetKey(KeyCode.A)) {
            if (m_backPedalTimer > 1.0) {
                ChangeState(State.BackwardRun);
            } else {
                ChangeState(State.BackPedal);
            }
        } else {
            m_backPedalTimer = 0;
            if (attackStatus == GameplayManager.AttackStatus.Melee && !m_doNotAttack) {
                ChangeState(State.MeleeAttack);
            } else if (attackStatus == GameplayManager.AttackStatus.Ranged && !m_doNotAttack) {
                ChangeState(State.RangedAttack);
            } else if (attackStatus == GameplayManager.AttackStatus.RangedHold) {
                ChangeState(State.IdleRanged);
            } else {
                ChangeState(State.Idle);
            }
        }
    }

    private void HandleAnimation() {
        if (transform.position.x <= m_leftBound || transform.position.x >= m_rightBound) {
            SwitchToMelee();
            ChangeState(State.Idle);
        }

        if (currentState != m_previousState) {
            m_previousState = currentState;
            switch (currentState) {
                case State.Idle:
                    SwitchToMelee();
                    animation.CrossFade("Idle", 0.1f);
                    wrapperAnimation.CrossFade("Idle", 0.1f);
                    break;
                case State.IdleRanged:
                    SwitchToRanged();
                    animation.CrossFade("IdleRanged", 0.1f);
                    wrapperAnimation.CrossFade("IdleRanged", 0.1f);
                    break;
                case State.Forward:
                    SwitchToMelee();
                    animation.CrossFade("RunForward", 0.1f);
                    wrapperAnimation.CrossFade("RunForward", 0.1f);
                    break;
                case State.BackPedal:
                    SwitchToMelee();
                    animation.CrossFade("Backpedal", 0.1f);
                    wrapperAnimation.CrossFade("Backpedal", 0.1f);
                    break;
                case State.BackwardRun:
                    SwitchToMelee();
                    m_isTurning = true;
                    animation.Play("BackpedalTurn");
                    wrapperAnimation.Play("BackpedalTurn");
                    break;
                case State.MeleeAttack:
                    SwitchToMelee();
                    break;
                case State.RangedAttack:
                    SwitchToRanged();
                    break;
                case State.Die:
                    animation.CrossFade("Die", 0.1f);
                    wrapperAnimation.CrossFade("Die", 0.1f);
                    data.GetEquippedCostume().audioData.Die();
                    break;
            }
        }
        if (currentState == State.Die) return;

        if (currentState == State.MeleeAttack) {
            if (m_meleeAttackTimer < 0f)
                m_meleeAttackTimer = meleeWeapon.data.attackFrequency;
            if (!animation.IsPlaying("Attack01") && !animation.IsPlaying("Attack02") && !animation.IsPlaying("Attack03")) {
                if (m_meleeAttackTimer == meleeWeapon.data.attackFrequency) {
                    int rand = Random.Range(1, 4);
                    animation.CrossFade($"Attack0{rand}", 0.1f);
                    wrapperAnimation.CrossFade($"Attack0{rand}", 0.1f);
                } else {
                    animation.CrossFade("Idle", 0.1f);
                    wrapperAnimation.CrossFade("Idle", 0.1f);
                }
            }
        }
        m_meleeAttackTimer -= Time.deltaTime;

        if (currentState == State.RangedAttack && rangedWeapon != null) {
            if (m_rangedAttackTimer < 0f)
                m_rangedAttackTimer = rangedWeapon.data.attackFrequency;
            if (!animation.IsPlaying("AttackRanged")) {
                if (m_rangedAttackTimer == rangedWeapon.data.attackFrequency) {
                    animation.CrossFade("AttackRanged", 0.1f);
                    wrapperAnimation.CrossFade("AttackRanged", 0.1f);
                } else {
                    animation.CrossFade("IdleRanged", 0.1f);
                    wrapperAnimation.CrossFade("IdleRanged", 0.1f);
                }
            }
        }
        m_rangedAttackTimer -= Time.deltaTime;

        if (m_isTurning == true && !animation.IsPlaying("BackpedalTurn") && currentState == State.BackwardRun) {
            animation.Play("RunBackward");
            wrapperAnimation.Play("RunBackward");
            m_isTurning = false;
        }

        obj.SetActive(true);
    }

    private void HandleMotion() {
        switch (currentState) {
            case State.Forward:
                m_xVelocity += data.acceleration * Time.deltaTime;
                break;
            case State.BackwardRun:
                if (m_isTurning == false) {
                    m_xVelocity -= data.acceleration * 1.2f * Time.deltaTime;
                    break;
                }
                goto case State.BackPedal;
            case State.BackPedal:
                m_xVelocity -= data.acceleration * Time.deltaTime;
                m_backPedalTimer += Time.deltaTime;
                break;
        }

        xPos += m_xVelocity;
        m_xVelocity *= 0.90f;
        if (transform.position.x < m_leftBound)
            xPos = m_leftBound;
        if (transform.position.x > m_rightBound)
            xPos = m_rightBound;

        GameplayManager.heroX = transform.position.x;
    }

    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - transform.position.x;
        if (allegiance == Side.Right) distance *= -1;
        return (distance < meleeWeapon.data.range) && (distance > 0);
    }
    public override void MeleeHitSFX() {
        meleeWeapon.data.PlayHit();
    }

    public override void Damage(float damage) {
        health -= damage;
        m_healthRegenTimer = data.healthRegenDelay;
    }
    public override void Heal(float damage) {
        health += damage;
        m_healthRegenTimer = 0;
    }
};