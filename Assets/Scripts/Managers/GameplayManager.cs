using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

/* Manages the main, non-menu gameplay. */
public class GameplayManager { // Gameplay Manager
    private static Lazy<GameplayManager> m_instance = new Lazy<GameplayManager>(() => new GameplayManager());
    public static GameplayManager instance => m_instance.Value;

    /* Debug Tools */
    public static bool heroDoNotAttack = false;

    private bool m_initialised;
    public static bool initialised { 
        get => instance.m_initialised;
        set => instance.m_initialised = value;
    }
    private static Transform camera;
    public static float gameTimer;
    private static float m_spawnSave;

    public static float heroX;

    public static Stage stage;
    public static Gate gate;
    private static GameObject m_hud;
    private static BGM m_bgm;

    public int smithy = 0;
    private static float m_smithySave = 0;
    public const float smithyRate = 1;

    public static Dictionary<string, GameplayEntity> entities;
    public static Hero hero;

    public static WaveData wave;
    public static int enemiesRemaining;
    public static bool waveComplete;
    public static bool startSlowMo;
    private static float slowMoTimer;
    private static float transitionTimer;
    private static float victoryLength;
    private static int waveEntryIndex;
    private static List<EnemyData> m_enemySpawnQueue;
    private static float m_enemySpawnTimer;

    public static List<float> allyCooldowns;

    public static List<AbilityData> equippedAbilities;
    public static List<float> abilityCooldowns;

    public static Dictionary<string, GameplayEntity> closestTargets;

    public static async Task StartWave(Transform _cameraTransform) {
        m_instance = new Lazy<GameplayManager>(() => new GameplayManager());
        initialised = false;
        camera = _cameraTransform;

        var waveHandle = Addressables.LoadAssetAsync<WaveData>("Data/Waves/1");
        wave =  await waveHandle.Task;
        waveComplete = false;
        startSlowMo = false;
        enemiesRemaining = 0;
        victoryLength = 0;
        foreach (WaveData.Entry entry in wave.entries) {
            enemiesRemaining += entry.enemyQuanitity;
        }

        await AssetManager.LoadGameplay();
        entities = new Dictionary<string, GameplayEntity>();
        closestTargets = new Dictionary<string, GameplayEntity>();

        waveEntryIndex = 0;
        m_enemySpawnQueue = new List<EnemyData>();

        allyCooldowns = new();
        allyCooldowns.Add(0);

        equippedAbilities = new List<AbilityData>();
        abilityCooldowns = new List<float>();
        await AbilityManager.Init();

        stage = new Stage("ZenGarden");
        await stage.Init();

        SaveManager.EquipCostume("AlliesGate", 0);
        gate = new Gate(GameplayEntity.Side.Left);
        await gate.Init();
        AddEntity("Gate", gate);

        var hudHandle = Addressables.InstantiateAsync("Prefabs/Gameplay HUD");
        m_hud = await hudHandle.Task;

        SaveManager.EquipCostume("Samurai", 0);
        SaveManager.EquipCostume("Kunoichi", 0);
        SaveManager.EquipCostume("Ronin", 0);
        SaveManager.EquipCostume("Ashigaru", 0);
        hero = new Hero(SaveManager.selectedHero);
        hero.SetBounds(stage.leftBound, stage.rightBound);
        hero.allegiance = GameplayEntity.Side.Left;
        await hero.Init(stage.heroSpawn);
        AddEntity("Hero", hero);

        // Load BGM last so audio only starts once the game is ready.
        m_bgm = new BGM("Zen Garden Day");
        await m_bgm.Init();

        initialised = true;
    }

    private static void AddEntity(string id, GameplayEntity entity) {
        entity.SetEntityId(id);
        entities.Add(id, entity);
    }

    public static void SpawnEnemy(EnemyData _data) {
        Enemy enemy = new Enemy(_data, GameplayEntity.Side.Right);
        enemy.SetBounds(stage.leftBound, float.MaxValue);
        AddEntity($"Enemy{entities.Count - 1}", enemy);
        enemy.Spawn(stage.zombieSpawn);
    }
     public static void SpawnAlly(AllyData _data) {
        Ally ally = new Ally(_data, GameplayEntity.Side.Left);
        ally.SetBounds(float.MinValue, stage.rightBound);
        AddEntity($"Ally{entities.Count - 1}", ally);
        ally.Spawn(stage.allySpawn);
    }

    public static void Update() {
        if (initialised) {
            AbilityManager.Update();

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

            if (gameTimer - m_smithySave > smithyRate) {
                m_smithySave = gameTimer;
                instance.smithy += 1;
            }

            if (enemiesRemaining == 0) {
                if (!startSlowMo) {
                    slowMoTimer = gameTimer;
                    m_bgm.Stop();
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
                if (!(waveEntryIndex > wave.entries.Length - 1) && gameTimer - m_spawnSave > wave.entries[waveEntryIndex].delay) {
                    m_spawnSave = gameTimer;
                    for (int i = 0; i < wave.entries[waveEntryIndex].enemyQuanitity; i++)
                        m_enemySpawnQueue.Add(wave.entries[waveEntryIndex].enemy);
                    waveEntryIndex++;
                }

                if (gameTimer - m_enemySpawnTimer > 0.3f && m_enemySpawnQueue.Count > 0) {
                    m_enemySpawnTimer = gameTimer;
                    SpawnEnemy(m_enemySpawnQueue[0]);
                    m_enemySpawnQueue.RemoveAt(0);
                }
            }

            gameTimer += Time.deltaTime;
        }
    }

    public static void DealDamage(string entityId) {
        GameplayEntity entity = entities[entityId];
        foreach (GameplayEntity enemy in entities.Values) {
            if (enemy == null || enemy.allegiance == entity.allegiance || enemy.currentState == GameplayEntity.State.Die)
                continue;

            if (entity.IsInMeleeRange(enemy.xPos + 0.2f * enemy.direction))
                entity.MeleeHit(enemy);
        }
    }

    public static void FireProjectile(string entityId) {
        entities[entityId].FireProjectile(closestTargets[entityId]);
    }

    public static void DestroyEntity(string entityId) {
        entities[entityId] = null;
    }
};