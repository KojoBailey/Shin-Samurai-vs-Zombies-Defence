using UnityEngine;

public class Gameplay : MonoBehaviour {
    [SerializeField] private Transform cameraTransform;
    private Vector3 cameraOffset = new Vector3(0f, 1.39f, -5.07f);
    private Quaternion cameraRotation = Quaternion.Euler(4.94f, 0f, 0f);

    private async void Start() {
        cameraTransform.rotation = cameraRotation;
        await GameplayManager.Init(cameraTransform);
        GameplayManager.instance.StartWave();

        SceneLoadManager.FinishLoading();
    }

    private void Update() {
        if (SceneLoadManager.finishedLoading && GameplayManager.instance.paused == false) {
            GameplayManager.instance.Update();
        }
    }

    private void LateUpdate() {
        if (SceneLoadManager.finishedLoading) {
            if (!GameplayManager.instance.startSlowMo)
                cameraTransform.position = GameplayManager.instance.hero.transform.position + cameraOffset;
            else
                cameraTransform.position = new Vector3(
                    GameplayManager.instance.hero.xPos + cameraOffset.x,
                    GameplayManager.instance.hero.transform.position.y + cameraOffset.y,
                    cameraTransform.position.z
                );
        }
    }
}
