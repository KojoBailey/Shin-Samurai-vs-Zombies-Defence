using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class Troop : GameplayEntity {
    private string id;
    private EnemyData data;

    private Weapon weapon;

    public enum State {
        Idle,
        Walk,
        Attack,
        KnockedBack,
        Stunned,
        Die,
        Victory
    }
    public State CurrentState;
    private State previousState;

    private float attackCooldown;

    private float leftBound, rightBound;

    public Vector3 position {
        get {
            return transform.position;
        }
    }

    public Troop(string enemyId) {
        id = enemyId;
    }

    public async Task Init(float spawnX) {
        var handle = Addressables.LoadAssetAsync<EnemyData>($"Data/Enemies/Zombies/{id}");
        data = await handle.Task;
        if (data == null) {
            Debug.LogError($"Could not find or load Enemy of id `{id}`.");
            return;
        }
        obj = Object.Instantiate(data.Prefab);
        Prepare();
        transform.position = new Vector3(spawnX, 0f, Random.Range(-0.4f, 0.4f));
        transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        // Attach weapon.
        weapon = new Weapon(data.WeaponData);
        // await weapon.Init(obj);

        data.AudioData.Spawn();
        loaded = true;
    }

    public void Update() {
        if (loaded) {
            HandleLogic();
            HandleMotion();
            HandleAnimation();
        }
    }

    private void HandleLogic() {
        if (InRangeOf(GameplayManager.HeroX)) {
            ChangeState(State.Attack);
        } else {
            if (!animation.IsPlaying("Attack01"))
                ChangeState(State.Walk);
        }
    }

    private void HandleAnimation() {
        if (transform.position.x <= leftBound || transform.position.x >= rightBound) {
            ChangeState(State.Idle);
        }

        if (CurrentState != previousState) {
            previousState = CurrentState;
            switch (CurrentState) {
                case State.Idle:
                    animation.CrossFade("Idle", 0.1f);
                    break;
                case State.Walk:
                    animation.CrossFade("Walk", 0.1f);
                    break;
            }
        }

        if (CurrentState == State.Attack) {
            if (attackCooldown < 0f)
                attackCooldown = data.AttackFrequency;
            if (!animation.IsPlaying("Attack01")) {
                if (attackCooldown == data.AttackFrequency) {
                    animation.CrossFade("Attack01", 0.1f);
                } else {
                    animation.CrossFade("Idle", 0.1f);
                }
            }
            attackCooldown -= Time.deltaTime;
        } else {
            attackCooldown = data.AttackFrequency;
        }

        obj.SetActive(true);
    }

    private void HandleMotion() {
        if (CurrentState == State.Walk)
            ChangeX(-1 * data.Speed * Time.deltaTime);
        if (transform.position.x < leftBound)
            SetX(leftBound);
        if (transform.position.x > rightBound)
            SetX(rightBound);
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

    private bool InRangeOf(float target) {
        float distance = transform.position.x - target;
        return (distance < data.MeleeRange) && (distance > 0); // Includes checking if in front or behind.
    }

    public override float GetMeleeDamage() {
        return 10;
    }
    public override bool IsInMeleeRange(float targetX) {
        float distance = targetX - transform.position.x;
        if (Allegiance == Side.Right) distance *= -1;
        return (distance < data.MeleeRange) && (distance > 0);
    }
};