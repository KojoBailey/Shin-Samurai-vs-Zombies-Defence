using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

/* Manages the main, non-menu gameplay. */
public class GameplayManager { // Gameplay Manager
    /* Singleton Instance */
    public static GameplayManager instance;

    public AbilityManager abilityManager = new AbilityManager();

    /* Debug Tools */
    public const bool fastLoad = false;
    public const bool heroDoNotAttack = false;

    public bool waveStarted = false;

    private Transform camera;

    public float gameTimer = 0;

    public float heroX;

    public List<AllyData> alliesData = new List<AllyData>();

    public Stage stage;
    public Gate gate;
    private GameObject hud;
    private BGM bgm;

    public int smithy = 0;
    private static float smithySave = 0;
    public const float smithyRate = 1;

    public Dictionary<string, GameplayEntity> entities = new Dictionary<string, GameplayEntity>();
    public Hero hero;

    public Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

    public WaveData wave;
    public int totalEnemies;
    public int enemiesRemaining;
    private float spawnSave;
    public bool waveComplete = false;
    public bool startSlowMo = false;
    private float slowMoTimer;
    private float transitionTimer;
    private float victoryLength = 0; // Checked if 0 later on. !! consider changing this to be more explicit
    private int waveEntryIndex = 0; // Which sub-wave of enemies is currently being called.
    private List<EnemyData> enemySpawnQueue = new List<EnemyData>();
    private float enemySpawnTimer;

    public static GameObject healthBarPrefab;

    public List<float> allyCooldowns = new List<float>();

    public List<AbilityData> equippedAbilities = new List<AbilityData>();
    public List<float> abilityCooldowns = new List<float>();

    public Dictionary<string, GameplayEntity> closestTargets = new Dictionary<string, GameplayEntity>();

    private void Reset() {
        abilityManager = null;
        waveStarted = false;
        camera = null;
        gameTimer = 0;
        alliesData.Clear();
        audioClips.Clear();
    }

    // Initialises and loads the data but does not start the wave.
    public static async Task Init(Transform _cameraTransform) {
        // Initialise new instance.
        if (instance != null)
            instance.Reset();
        instance = new GameplayManager();
        instance.camera = _cameraTransform;

        // Load wave data.
        var waveHandle = Addressables.LoadAssetAsync<WaveData>("Data/Waves/1");
        instance.wave =  await waveHandle.Task;
        instance.totalEnemies = 0;
        foreach (WaveData.Entry entry in instance.wave.entries) {
            instance.totalEnemies += entry.enemyQuanitity;
        }
        instance.enemiesRemaining = instance.totalEnemies;

        // Load health bar prefab.
        var healthBarHandle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Entity Health Bar");
        healthBarPrefab = await healthBarHandle.Task;

        // Load ally data.
        var ashigaruDataHandle = Addressables.LoadAssetAsync<AllyData>($"Data/Allies/Humans/Ashigaru");
        AllyData ashigaruData = await ashigaruDataHandle.Task;
        if (ashigaruData == null) {
            Debug.LogError($"Could not find or load Ally of ID \"{"Humans/Ashigaru"}\".");
            return;
        }
        instance.alliesData.Add(ashigaruData);
        instance.allyCooldowns.Add(0); // !! Replace when Allies are loaded properly
        SaveManager.SetLevel(ashigaruData, 1); // !! Remove once save system implemented

        // Pre-load audio clips.
        await instance.LoadAudioClip("Wave Victory");
        if (!fastLoad) {
            for (int i = 0; i < 5; i++)
                await instance.LoadAudioClip($"Combat/Swoosh Small 0{i}");
            for (int i = 0; i < 5; i++)
                await instance.LoadAudioClip($"Combat/Swoosh Medium 0{i}");
            for (int i = 0; i < 3; i++)
                await instance.LoadAudioClip($"Combat/Arrow Fire 0{i}");
            for (int i = 0; i < 3; i++)
                await instance.LoadAudioClip($"Combat/Footstep Large 0{i}");
            for (int i = 0; i < 5; i++)
                await instance.LoadAudioClip($"Combat/Footstep 0{i}");
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

    private async Task LoadAudioClip(string address) {
        audioClips.Add(address, await SFXManager.Load(address));
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

            if (gameTimer - smithySave > smithyRate) {
                smithySave = gameTimer;
                smithy += 1;
            }

            if (enemiesRemaining == 0) {
                if (!startSlowMo) {
                    slowMoTimer = gameTimer;
                    bgm.Stop();
                    camera.position = new Vector3(camera.position.x, 1.12f, -3.3f);
                    Time.timeScale = 0.2f;
                    startSlowMo = true;
                } else {
                    if (slowMoTimer == 0) {
                        camera.localPosition += new Vector3(0, 0.01f, -0.1f) * Time.deltaTime;
                    } else if (gameTimer - slowMoTimer > 0.4f) {
                        slowMoTimer = 0;
                        victoryLength = SFXManager.Play("Wave Victory");
                        transitionTimer = gameTimer;
                        Time.timeScale = 1;
                        waveComplete = true;
                    }
                    if (victoryLength > 0 && gameTimer - transitionTimer > victoryLength) {
                        SceneLoadManager.LoadScene("TitleScreen");
                    }
                }
            } else {
                if (!(waveEntryIndex > wave.entries.Length - 1) && gameTimer - spawnSave > wave.entries[waveEntryIndex].delay) {
                    spawnSave = gameTimer;
                    for (int i = 0; i < wave.entries[waveEntryIndex].enemyQuanitity; i++)
                        enemySpawnQueue.Add(wave.entries[waveEntryIndex].enemy);
                    waveEntryIndex++;
                }

                if (gameTimer - enemySpawnTimer > 0.3f && enemySpawnQueue.Count > 0) {
                    enemySpawnTimer = gameTimer;
                    SpawnEnemy(enemySpawnQueue[0]);
                    enemySpawnQueue.RemoveAt(0);
                }
            }

            instance.gameTimer += Time.deltaTime;
        }
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