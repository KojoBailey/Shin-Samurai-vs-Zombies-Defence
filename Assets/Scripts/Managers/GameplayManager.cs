using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Threading.Tasks;

/* Manages the main, non-menu gameplay. */
public class GameplayManager { // Gameplay Manager
    /* Singleton Instance */
    public static GameplayManager instance;

    /* Debug Tools */
    public static string className = typeof(GameplayManager).Name;
    public const bool fastLoad = false;
    public const bool heroDoNotAttack = false;

    /* Sub-managers */
    // !! Later these will need separating from GameplayManager for use in menus.
    public AbilityManager abilityManager = new AbilityManager();

    /* Assets & Data */
    private List<object> addressableAssets = new List<object>();
    public static GameObject healthBarPrefab;
    public WaveData wave;
    public List<AbilityData> equippedAbilities = new List<AbilityData>();
    public List<AllyData> equippedAllies = new List<AllyData>();
    private List<EnemyData> enemySpawnQueue = new List<EnemyData>();

    /* Game Objects */
    public Stage stage;
    public Gate gate;
    private GameObject hud;
    private BGM bgm;
    public Dictionary<string, GameplayEntity> entities = new Dictionary<string, GameplayEntity>();
    public Hero hero;

    /* Time Trackers */
    public float waveStopwatch = 0; // Time since wave began.
    public List<float> allyCooldowns = new List<float>();
    public List<float> abilityCooldowns = new List<float>();
    private float enemySpawnTimer; // Time between groups of enemy spawns.
    private const float enemySpacingDuration = 0.3f; // Time between enemy spawns of the same group.
    private float enemySpacingTimer;
    private const float slowMoDuration = 0.4f; // Duration of the victory slow-mo.
    private float slowMoTimer;
    private float victoryDuration; // Equal to the victory music duration.
    private float victoryTimer;

    /* Completion Flags */
    public bool waveStarted = false;
    public bool waveComplete = false;
    public bool startSlowMo = false;

    private Transform camera;
    public float heroX;

    public int smithy = 0;
    private float smithySave = 0;
    public const float smithyRate = 1;

    public int totalEnemies;
    public int enemiesRemaining;
    private int waveEntryIndex = 0; // Which sub-wave of enemies is currently being called.

    public Dictionary<string, GameplayEntity> closestTargets = new Dictionary<string, GameplayEntity>();

    // Initialises and loads the data but does not start the wave.
    public static async Task Init(Transform _cameraTransform) {
        // Initialise new instance.
        instance = new GameplayManager();
        instance.camera = _cameraTransform;

        // Load wave data.
        var waveHandle = Addressables.LoadAssetAsync<WaveData>("Data/Waves/1");
        instance.wave =  await waveHandle.Task;
        instance.addressableAssets.Add(instance.wave);
        instance.totalEnemies = 0;
        foreach (WaveData.Entry entry in instance.wave.entries) {
            instance.totalEnemies += entry.enemyQuanitity;
        }
        instance.enemiesRemaining = instance.totalEnemies;

        // Load health bar prefab.
        var healthBarHandle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Entity Health Bar");
        healthBarPrefab = await healthBarHandle.Task;
        instance.addressableAssets.Add(healthBarPrefab);

        // Load ally data.
        var ashigaruDataHandle = Addressables.LoadAssetAsync<AllyData>($"Data/Allies/Humans/Ashigaru");
        AllyData ashigaruData = await ashigaruDataHandle.Task;
        instance.addressableAssets.Add(ashigaruData);
        if (ashigaruData == null) {
            Debug.LogError($"Could not find or load Ally of ID \"{"Humans/Ashigaru"}\".");
            return;
        }
        instance.equippedAllies.Add(ashigaruData);
        instance.allyCooldowns.Add(0); // !! Replace when Allies are loaded properly
        SaveManager.SetLevel(ashigaruData, 1); // !! Remove once save system implemented

        // Pre-load audio clips.
        await SFXManager.Load(className, "Wave Victory");
        if (!fastLoad) {
            await SFXManager.Load(className, "Combat/Swoosh Small");
            await SFXManager.Load(className, "Combat/Swoosh Medium");
            await SFXManager.Load(className, "Combat/Arrow Fire");
            await SFXManager.Load(className, "Combat/Footstep");
            await SFXManager.Load(className, "Combat/Footstep Large");
        }

        // Load abilities.
        await instance.abilityManager.Init();

        // Load stage.
        instance.stage = new Stage(instance.wave.stage);
        await instance.stage.Init();

        // Load gate.
        SaveManager.EquipCostume("AlliesGate", 0);
        instance.gate = new Gate(GameplayEntity.Side.Left);
        await instance.gate.Init();
        instance.AddEntity("Gate", instance.gate);

        // Load HUD.
        var hudHandle = Addressables.InstantiateAsync("Prefabs/Gameplay HUD");
        instance.hud = await hudHandle.Task;

        // Load hero.
        SaveManager.EquipCostume("Samurai", 0);
        SaveManager.EquipCostume("Kunoichi", 0);
        SaveManager.EquipCostume("Ronin", 0);
        SaveManager.EquipCostume("Ashigaru", 0);
        instance.hero = new Hero(SaveManager.selectedHero);
        instance.hero.SetBounds(instance.stage.leftBound, instance.stage.rightBound);
        instance.hero.allegiance = GameplayEntity.Side.Left;
        await instance.hero.Init(instance.stage.heroSpawn);
        instance.AddEntity("Hero", instance.hero);

        // Load BGM.
        instance.bgm = new BGM("Zen Garden Day");
        await instance.bgm.Init();
    }

    public void StartWave() {
        bgm.Play();
        waveStarted = true;
    }

    private void AddEntity(string id, GameplayEntity entity) {
        entity.SetEntityId(id);
        entities.Add(id, entity);
    }

    public void SpawnEnemy(EnemyData _data) {
        Enemy enemy = new Enemy(_data, GameplayEntity.Side.Right);
        enemy.SetBounds(stage.leftBound, float.MaxValue);
        AddEntity($"Enemy{entities.Count - 1}", enemy);
        enemy.Spawn(stage.zombieSpawn);
    }
     public void SpawnAlly(AllyData _data) {
        Ally ally = new Ally(_data, GameplayEntity.Side.Left);
        ally.SetBounds(float.MinValue, stage.rightBound);
        AddEntity($"Ally{entities.Count - 1}", ally);
        ally.Spawn(stage.allySpawn);
    }

    public void Update() {
        if (waveStarted) {
            abilityManager.Update();

            // Update each entity and destroy finished ones.
            foreach (var entity in entities) {
                if (entity.Value != null) {
                    if (entity.Value.toDestroy) {
                        DestroyEntity(entity.Value.entityId);
                        break;
                    }
                    entity.Value.Update();
                }
            }

            foreach (GameplayEntity entity in entities.Values) {
                if (entity != null) {
                    stage.ApplyGravity(entity);

                    // Get closest targets to each entity.
                    float closestDistance = float.MaxValue;
                    foreach (GameplayEntity target in entities.Values) {
                        if (target != null && target.allegiance != entity.allegiance && target.currentState != GameplayEntity.State.Die) {
                            float distance = target.xPos - entity.xPos;
                            if (entity.allegiance == GameplayEntity.Side.Right)
                                distance *= -1;
                            if (distance > 0 && distance < closestDistance) {
                                closestDistance = distance;
                                closestTargets[entity.entityId] = target;
                            }
                        }
                    }
                }
            }

            allyCooldowns[0] -= Time.deltaTime;
            for (int i = 0; i < abilityCooldowns.Count; i++)
                abilityCooldowns[i] -= Time.deltaTime;

            if (waveStopwatch - smithySave > smithyRate) {
                smithySave = waveStopwatch;
                smithy += 1;
            }

            if (enemiesRemaining == 0) {
                if (!startSlowMo) {
                    slowMoTimer = waveStopwatch;
                    bgm.Stop();
                    camera.position = new Vector3(camera.position.x, 1.12f, -3.3f);
                    Time.timeScale = 0.2f;
                    startSlowMo = true;
                } else {
                    if (slowMoTimer == 0) {
                        camera.localPosition += new Vector3(0, 0.01f, -0.1f) * Time.deltaTime;
                    } else if (waveStopwatch - slowMoTimer > slowMoDuration) {
                        slowMoTimer = 0;
                        victoryDuration = SFXManager.PlayFromBundle("Wave Victory");
                        victoryTimer = waveStopwatch;
                        Time.timeScale = 1;
                        waveComplete = true;
                    }
                    if (waveComplete && waveStopwatch - victoryTimer > victoryDuration) {
                        Terminate();
                        SceneLoadManager.LoadScene("TitleScreen");
                    }
                }
            } else {
                if (!(waveEntryIndex > wave.entries.Length - 1) && waveStopwatch - enemySpawnTimer > wave.entries[waveEntryIndex].delay) {
                    enemySpawnTimer = waveStopwatch;
                    for (int i = 0; i < wave.entries[waveEntryIndex].enemyQuanitity; i++)
                        enemySpawnQueue.Add(wave.entries[waveEntryIndex].enemy);
                    waveEntryIndex++;
                }

                if (waveStopwatch - enemySpacingTimer > enemySpacingDuration && enemySpawnQueue.Count > 0) {
                    enemySpacingTimer = waveStopwatch;
                    SpawnEnemy(enemySpawnQueue[0]);
                    enemySpawnQueue.RemoveAt(0);
                }
            }

            instance.waveStopwatch += Time.deltaTime;
        }
    }

    public void Terminate() {
        foreach (var handle in addressableAssets)
            Addressables.Release(handle);
        SFXManager.Clear(className);
    }

    public void DealDamage(string entityId) {
        GameplayEntity entity = entities[entityId];
        foreach (GameplayEntity enemy in entities.Values) {
            if (enemy == null || enemy.allegiance == entity.allegiance || enemy.currentState == GameplayEntity.State.Die)
                continue;

            if (entity.IsInMeleeRange(enemy.xPos + 0.2f * enemy.direction))
                entity.MeleeHit(enemy);
        }
    }

    public void FireProjectile(string entityId) {
        entities[entityId].FireProjectile(closestTargets[entityId]);
    }

    public void DestroyEntity(string entityId) {
        entities[entityId] = null;
    }
};