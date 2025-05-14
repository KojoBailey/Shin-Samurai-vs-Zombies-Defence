using System.Threading.Tasks;
using UnityEngine;

public class Gameplay : MonoBehaviour {
    [SerializeField] private Transform cameraTransform;
    private Vector3 cameraOffset = new Vector3(0f, 1.39f, -5.07f);
    private Quaternion cameraRotation = Quaternion.Euler(4.94f, 0f, 0f);

    private async void Start() {
        await GameplayManager.StartWave();

        cameraTransform.rotation = cameraRotation;

        SceneLoadManager.FinishLoading();
    }

    private async void Update() {
        if (SceneLoadManager.finishedLoading) {
            await GameplayManager.Update();
        }
    }

    private void LateUpdate() {
        if (SceneLoadManager.finishedLoading) {
            cameraTransform.position = GameplayManager.hero.position + cameraOffset;
        }
    }
}
