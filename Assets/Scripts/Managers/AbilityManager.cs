using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

// Stores functions for and handles Abilities during gameplay.
public class AbilityManager { // Ability Manager
    private static List<Action> queue;

    private static AbilityData lethargyData;
    private static bool lethargyActive = false;
    private static float lethargyTimer;

    public static async Task Init() {
        queue = new List<Action>();
        
        var lethargyDataHandle = Addressables.LoadAssetAsync<AbilityData>("Data/Abilities/Lethargy");
        lethargyData = await lethargyDataHandle.Task;
        SaveManager.SetLevel(lethargyData, 1); // !! Remove once save system implemented.
    }

    public static void Update() {
        if (lethargyTimer <= 0 && lethargyActive == true)
            LethargyEnd();
        lethargyTimer -= Time.deltaTime;
    }

    public static void QueueAbility(Action func) {
        queue.Add(func);
        if (func == Lethargy) {
            GameplayManager.hero.abilityStatus = Hero.AbilityStatus.CastForward;
        }
    }
    public static void ActivateAbility() {
        queue[0]();
        queue.RemoveAt(0);
    }

    public static void Lethargy() {
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
}