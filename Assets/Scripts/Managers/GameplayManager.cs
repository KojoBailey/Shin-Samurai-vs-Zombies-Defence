using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

/* Manages the main, non-menu gameplay. */
public class GameplayManager { // Gameplay Manager
    public static bool initialised = false;
    public static float gameTimer;
    private static float m_spawnSave;

    public static float heroX;

    public static Stage stage;
    private static GameObject m_hud;
    private static BGM m_bgm;

    public static Dictionary<string, GameplayEntity> entities;
    public static Hero hero;
    public static List<Enemy> enemies;
    public static List<Ally> allies;

    public static Dictionary<string, GameplayEntity> closestTargets;

    public enum AttackStatus { None, RangedHold, Melee, Ranged };

    public static async Task StartWave() {
        stage = new Stage("ZenGarden");
        await stage.Init();

        var hudHandle = Addressables.InstantiateAsync("Prefabs/Gameplay HUD");
        m_hud = await hudHandle.Task;

        m_bgm = new BGM("Zen Garden Day");
        await m_bgm.Init();

        entities = new();
        closestTargets = new();

        SaveManager.EquipCostume("Samurai", 0);
        SaveManager.EquipCostume("Kunoichi", 0);
        SaveManager.EquipCostume("Ronin", 0);
        hero = new Hero(SaveManager.selectedHero);
        hero.SetBounds(stage.leftBound, stage.rightBound);
        hero.allegiance = GameplayEntity.Side.Left;
        await hero.Init(stage.heroSpawn);
        AddEntity("Hero", hero);

        enemies = new List<Enemy>();
        allies = new List<Ally>();

        initialised = true;
    }

    private static void AddEntity(string id, GameplayEntity entity) {
        entity.SetEntityId(id);
        entities.Add(id, entity);
    }

    public static async Task SpawnEnemy(string id) {
        Enemy enemy = new Enemy(id);
        enemy.SetBounds(stage.leftBound, float.MaxValue);
        enemy.allegiance = GameplayEntity.Side.Right;
        await enemy.Init(stage.zombieSpawn);
        enemies.Add(enemy);
        AddEntity($"Enemy{entities.Count - 1}", enemy);
    }
     public static async Task SpawnAlly(string id) {
        Ally ally = new Ally(id);
        ally.SetBounds(float.MinValue, stage.rightBound);
        ally.allegiance = GameplayEntity.Side.Left;
        await ally.Init(stage.allySpawn);
        allies.Add(ally);
        AddEntity($"Ally{entities.Count - 1}", ally);
    }

    public static async Task Update() {
        if (initialised) {
            hero.Update();
            foreach (Enemy enemy in enemies) {
                if (enemy.toDestroy) {
                    DestroyEntity(enemy.entityId);
                    break;
                }
                enemy.Update();
            }
            foreach (Ally ally in allies) {
                if (ally.toDestroy) {
                    DestroyEntity(ally.entityId);
                    break;
                }
                ally.Update();
            }

            foreach (Enemy enemy in enemies) {
                if (enemy.currentState == GameplayEntity.State.Die) continue;
                float difference = enemy.xPos - hero.xPos;
                if (difference > 0) {
                    if (difference < hero.meleeRange) {
                        hero.attackStatus = AttackStatus.Melee;
                        break;
                    } else if (difference < hero.rangedRange) {
                        hero.attackStatus = AttackStatus.Ranged;
                        break;
                    } else if (difference < hero.rangedRange + 1) {
                        hero.attackStatus = AttackStatus.RangedHold;
                        break;
                    } else {
                        hero.attackStatus = AttackStatus.None;
                    }
                }
            }

            foreach (GameplayEntity entity in entities.Values) {
                if (entity != null) {
                    stage.ApplyGravity(entity);

                    float closestDistance = float.MaxValue;
                    foreach (GameplayEntity target in entities.Values) {
                        if (target != null && target.allegiance != entity.allegiance && target.health > 0) {
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
                // await SpawnEnemy("LightZombie");
                await SpawnAlly("Humans/Ashigaru");
            }

            gameTimer += Time.deltaTime;
        }
    }

    public static void DealDamage(string entityId) {
        GameplayEntity entity = entities[entityId];
        if (entity.allegiance == GameplayEntity.Side.Left) {
            foreach (Enemy enemy in enemies) {
                if (entity.IsInMeleeRange(enemy.xPos)) {
                    enemy.Damage(10); // Change to troop damage
                    entity.MeleeHitSFX();
                }
            }
        } else if (entity.allegiance == GameplayEntity.Side.Right) {
            if (entity.IsInMeleeRange(hero.xPos)) {
                hero.Damage(10); // Change to troop damage
                entity.MeleeHitSFX();
            }
        }
    }

    public static void FireProjectile(string entityId) {
        entities[entityId].FireProjectile(closestTargets[entityId]);
    }

    public static void DestroyEntity(string entityId) {
        if (enemies.Contains((Enemy)entities[entityId]))
            enemies.Remove((Enemy)entities[entityId]);
        entities[entityId] = null;
    }
};