using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GameplayManager {
    public static float GameTimer;
    private static float spawnSave;

    public static float HeroX;

    public static Stage stage;
    private static BGM bgm;

    public static Dictionary<string, GameplayEntity> Entities;
    public static Hero hero;
    public static List<Troop> Enemies;

    public static async Task StartWave() {
        stage = new Stage("ZenGarden");
        await stage.Init();

        bgm = new BGM("Zen Garden Day");
        await bgm.Init();

        Entities = new();

        hero = new Hero("Samurai");
        await hero.Init(stage.HeroSpawn);
        hero.SetBounds(stage.LeftBound, stage.RightBound);
        hero.Allegiance = GameplayEntity.Side.Left;
        AddEntity("Hero", hero);

        Enemies = new List<Troop>();
    }

    private static void AddEntity(string id, GameplayEntity entity) {
        entity.SetEntityId(id);
        Entities.Add(id, entity);
    }

    public static async Task SpawnEnemy(string id) {
        Troop enemy = new Troop(id);
        await enemy.Init(stage.ZombieSpawn);
        enemy.SetBounds(stage.LeftBound, float.MaxValue);
        enemy.Allegiance = GameplayEntity.Side.Right;
        Enemies.Add(enemy);
        AddEntity($"Enemy{Entities.Count - 1}", enemy);
    }

    public static async Task Update() {
        hero.Update();
        foreach (Troop enemy in Enemies) {
            enemy.Update();
        }

        foreach (GameplayEntity entity in Entities.Values) {
            stage.SnapToGround(entity.transform);
        }

        if (GameTimer - spawnSave > 5) {
            spawnSave = GameTimer;
            await SpawnEnemy("LightZombie");
        }

        GameTimer += Time.deltaTime;
    }

    public static void DealDamage(string entityId) {
        GameplayEntity entity = Entities[entityId];
        if (entity.Allegiance == GameplayEntity.Side.Left) {
            foreach (Troop enemy in Enemies) {
                if (entity.IsInMeleeRange(enemy.position.x)) {
                    enemy.Damage(entity.GetMeleeDamage());
                    entity.MeleeAttackSFX();
                }
            }
        } else if (entity.Allegiance == GameplayEntity.Side.Right) {
            if (entity.IsInMeleeRange(hero.position.x)) {
                hero.Damage(entity.GetMeleeDamage());
                entity.MeleeAttackSFX();
            }
        }
    }
};