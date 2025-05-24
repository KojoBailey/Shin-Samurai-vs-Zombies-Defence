using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

// Stores functions for and handles Abilities during gameplay.
public class AbilityManager { // Ability Manager
    private static List<Action<string>> queue;

    private static AbilityData lethargyData;
    private static bool lethargyActive = false;
    private static float lethargyTimer;

    private static AbilityData katanaSlashData;

    public static async Task Init() {
        queue = new List<Action<string>>();

        var katanaSlashDataHandle = Addressables.LoadAssetAsync<AbilityData>("Data/Abilities/Katana Slash");
        katanaSlashData = await katanaSlashDataHandle.Task;
        GameplayManager.equippedAbilities.Add(katanaSlashData); // !! Remove for proper management.
        SaveManager.SetLevel(katanaSlashData, 1); // !! Remove once save system implemented.

        var lethargyDataHandle = Addressables.LoadAssetAsync<AbilityData>("Data/Abilities/Lethargy");
        lethargyData = await lethargyDataHandle.Task;
        GameplayManager.equippedAbilities.Add(lethargyData); // !! Remove for proper management.
        SaveManager.SetLevel(lethargyData, 1); // !! Remove once save system implemented.
    }

    public static void Update() {
        if (lethargyTimer <= 0 && lethargyActive == true)
            LethargyEnd();
        lethargyTimer -= Time.deltaTime;
    }

    public static void QueueAbility(Action<string> func) {
        queue.Add(func);
        if (func == Lethargy) {
            GameplayManager.hero.abilityStatus = Hero.AbilityStatus.CastForward;
        } else if (func == KatanaSlash) {
            GameplayManager.hero.abilityStatus = Hero.AbilityStatus.KatanaSlash;
        }
    }
    public static void ActivateAbility(string entityId) {
        queue[0](entityId);
        queue.RemoveAt(0);
    }

    public static void Lethargy(string entityId) {
        foreach (GameplayEntity entity in GameplayManager.entities.Values) {
            if (entity == null || entity.currentState == GameplayEntity.State.Die)
                continue;

            if (entity.allegiance == GameplayEntity.Side.Right) {
                entity.ChangeSpeed(lethargyData.speedMultiplier);
                entity.obj.GetComponent<SkinnedMeshRenderer>().material.color = lethargyData.materialColour;
            }
        }
        SFXManager.Play(lethargyData.soundEffect);
        lethargyTimer = lethargyData.duration;
        lethargyActive = true;
    }
    private static void LethargyEnd() {
        foreach (GameplayEntity entity in GameplayManager.entities.Values) {
            if (entity == null || entity.currentState == GameplayEntity.State.Die)
                continue;

            if (entity.allegiance == GameplayEntity.Side.Right) {
                entity.ChangeSpeed(1);
                entity.obj.GetComponent<SkinnedMeshRenderer>().material.color = Color.white;
            }
        }
        lethargyActive = false;
    }

    public static void KatanaSlash(string entityId) {
        Hero hero = (Hero)GameplayManager.entities[entityId];
        foreach (GameplayEntity enemy in GameplayManager.entities.Values) {
            if (enemy == null || enemy.allegiance == hero.allegiance || enemy.currentState == GameplayEntity.State.Die)
                continue;

            float distance = enemy.xPos - hero.xPos;
            distance *= hero.direction;
            if ((distance < katanaSlashData.range) && (distance > 0))
                hero.MeleeHit(enemy, katanaSlashData.damage);
        }
    }
}