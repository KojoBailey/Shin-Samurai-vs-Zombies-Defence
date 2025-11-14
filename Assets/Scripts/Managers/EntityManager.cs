using UnityEngine;
using System.Collections.Generic;

public class EntityManager {
    public Dictionary<string, GameplayEntity> entities = new Dictionary<string, GameplayEntity>();

    public Dictionary<string, GameplayEntity> closestTargets = new Dictionary<string, GameplayEntity>();

    private void DestroyFinishedEntities() {
        List<string> entitiesToDestroy = new List<string>();
        foreach (GameplayEntity entity in entities.Values) {
            if (entity.toDestroy) {
                entitiesToDestroy.Add(entity.entityId);
            }
        }
        foreach (string entityId in entitiesToDestroy) {
            entities.Remove(entityId);
        }
    }

    private void CalculateClosestTarget(GameplayEntity entity) {
        float closestDistance = float.MaxValue;
        foreach (GameplayEntity target in entities.Values) {
            if (target.allegiance == entity.allegiance || target.isDead) continue;
            float distance = target.xPos - entity.xPos;
            if (entity.allegiance == GameplayEntity.Side.Right) {
                distance *= -1;
            }
            if (distance > 0 && distance < closestDistance) {
                closestDistance = distance;
                closestTargets[entity.entityId] = target;
            }
        }
    }

    private void UpdateEntities() {
        foreach (GameplayEntity entity in entities.Values) {
            if (entity.rangedWeapon != null) {
                CalculateClosestTarget(entity);
            }
            GameplayManager.instance.stage.ApplyGravity(entity);
            entity.Update();
        }
    }

    public void Update() {
        DestroyFinishedEntities();
        UpdateEntities();
    }

    public void AddEntity(string id, GameplayEntity entity) {
        entity.SetEntityId(id);
        entities.Add(id, entity);
    }
}
