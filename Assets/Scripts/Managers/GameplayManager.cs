using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Threading.Tasks;

/* Manages the main, non-menu gameplay. */
public class GameplayManager { // Gameplay Manager
    public static bool initialised = false;
    public static float gameTimer;
    private static float m_spawnSave;

    public static float heroX;

    public static Stage stage;
    private static GameObject m_hud;
    private static BGM m_bgm;

    public static int smithy = 0;
    private static float m_smithySave = 0;
    public const float smithyRate = 1;

    public static Dictionary<string, GameplayEntity> entities;
    public static Hero hero;

    public static List<float> allyCooldowns;

    public static List<AbilityData> equippedAbilities;
    public static List<float> abilityCooldowns;

    public static Dictionary<string, GameplayEntity> closestTargets;

    public static async Task StartWave() {
        await AssetManager.LoadGameplay();

        allyCooldowns = new();
        allyCooldowns.Add(0);

        equippedAbilities = new();
        abilityCooldowns = new();
        abilityCooldowns.Add(0);
        await AbilityManager.Init();

        stage = new Stage("ZenGarden");
        await stage.Init();

        var hudHandle = Addressables.InstantiateAsync("Prefabs/Gameplay HUD");
        m_hud = await hudHandle.Task;

        entities = new();
        closestTargets = new();

        SaveManager.EquipCostume("Samurai", 0);
        SaveManager.EquipCostume("Kunoichi", 0);
        SaveManager.EquipCostume("Ronin", 0);
        SaveManager.EquipCostume("Ashigaru", 0);
        hero = new Hero(SaveManager.selectedHero);
        hero.SetBounds(stage.leftBound, stage.rightBound);
        hero.allegiance = GameplayEntity.Side.Left;
        hero.doNotAttack = false;
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

            // Call Update() on each non-null entity.
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

            if (gameTimer - m_spawnSave > 5) {
                m_spawnSave = gameTimer;
                SpawnEnemy(AssetManager.enemiesData["LightZombie"]);
            }
            allyCooldowns[0] -= Time.deltaTime;
            abilityCooldowns[0] -= Time.deltaTime;

            if (gameTimer - m_smithySave > smithyRate) {
                m_smithySave = gameTimer;
                smithy += 1;
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