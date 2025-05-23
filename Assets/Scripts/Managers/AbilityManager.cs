using UnityEngine;

// Stores functions for and handles Abilities during gameplay.
public class AbilityManager { // Ability Manager
    private static bool lethargyActive = false;
    private static float lethargyTimer;

    public static void Update() {
        if (lethargyTimer <= 0 && lethargyActive == true)
            LethargyEnd();
        lethargyTimer -= Time.deltaTime;
    }

    public static void Lethargy() {
        foreach (GameplayEntity entity in GameplayManager.entities.Values) {
            if (entity == null || entity.currentState == GameplayEntity.State.Die)
                continue;

            if (entity.allegiance == GameplayEntity.Side.Right)
                entity.ChangeSpeed(0.3f);
        }
        lethargyTimer = 5;
        lethargyActive = true;
    }
    private static void LethargyEnd() {
        foreach (GameplayEntity entity in GameplayManager.entities.Values) {
            if (entity == null || entity.currentState == GameplayEntity.State.Die)
                continue;

            if (entity.allegiance == GameplayEntity.Side.Right)
                entity.ChangeSpeed(1);
        }
        lethargyActive = false;
    }
}