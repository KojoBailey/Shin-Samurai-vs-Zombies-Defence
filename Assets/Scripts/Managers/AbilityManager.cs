using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

// Stores functions for and handles Abilities during gameplay.
public class AbilityManager { // Ability Manager
    private List<Action<string>> queue;

    private AbilityData lethargyData;
    private bool lethargyActive = false;
    private float lethargyTimer;

    private AbilityData katanaSlashData;

    public async Task Init() {
        queue = new List<Action<string>>();

        var katanaSlashDataHandle = Addressables.LoadAssetAsync<AbilityData>("Data/Abilities/KatanaSlash");
        katanaSlashData = await katanaSlashDataHandle.Task;
        GameplayManager.instance.equippedAbilities.Add(katanaSlashData); // !! Remove for proper management.
        GameplayManager.instance.abilityCooldowns.Add(0);
        SaveManager.SetLevel(katanaSlashData, 1); // !! Remove once save system implemented.

        var lethargyDataHandle = Addressables.LoadAssetAsync<AbilityData>("Data/Abilities/Lethargy");
        lethargyData = await lethargyDataHandle.Task;
        GameplayManager.instance.equippedAbilities.Add(lethargyData); // !! Remove for proper management.
        GameplayManager.instance.abilityCooldowns.Add(0);
        SaveManager.SetLevel(lethargyData, 1); // !! Remove once save system implemented.
    }

    public void Update() {
        if (lethargyTimer <= 0 && lethargyActive == true)
            LethargyEnd();
        lethargyTimer -= Time.deltaTime;
    }

    public void QueueAbility(string abilityId) {
        if (abilityId == "Lethargy") {
            queue.Add(Lethargy);
            GameplayManager.instance.hero.abilityState = Hero.AbilityState.CastForward;
        } else if (abilityId == "KatanaSlash") {
            queue.Add(KatanaSlash);
            GameplayManager.instance.hero.abilityState = Hero.AbilityState.KatanaSlash;
        }
    }
    public void ActivateAbility(string entityId) {
        queue[0](entityId);
        queue.RemoveAt(0);
    }

    public void Lethargy(string entityId) {
        foreach (GameplayEntity entity in GameplayManager.instance.entities.Values) {
            if (entity == null || entity.isDead)
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
    private void LethargyEnd() {
        foreach (GameplayEntity entity in GameplayManager.instance.entities.Values) {
            if (entity == null || entity.isDead)
                continue;

            if (entity.allegiance == GameplayEntity.Side.Right) {
                entity.ChangeSpeed(1);
                entity.obj.GetComponent<SkinnedMeshRenderer>().material.color = Color.white;
            }
        }
        lethargyActive = false;
    }

    public void KatanaSlash(string entityId) {
        Hero hero = (Hero)GameplayManager.instance.entities[entityId];
        foreach (GameplayEntity enemy in GameplayManager.instance.entities.Values) {
            if (enemy == null || enemy.allegiance == hero.allegiance || enemy.isDead)
                continue;

            float distance = enemy.xPos - hero.xPos;
            distance *= hero.direction;
            if ((distance < katanaSlashData.range) && (distance > 0))
                hero.MeleeHit(enemy, katanaSlashData.damage);
        }
    }
}