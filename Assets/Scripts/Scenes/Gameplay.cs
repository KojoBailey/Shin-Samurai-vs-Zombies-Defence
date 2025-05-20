using System.Threading.Tasks;
using UnityEngine;

public class Gameplay : MonoBehaviour {
    [SerializeField] private Transform m_cameraTransform;
    private Vector3 m_cameraOffset = new Vector3(0f, 1.39f, -5.07f);
    private Quaternion m_cameraRotation = Quaternion.Euler(4.94f, 0f, 0f);

    private async void Start() {
        await GameplayManager.StartWave();

        m_cameraTransform.rotation = m_cameraRotation;

        SceneLoadManager.FinishLoading();
    }

    private async void Update() {
        if (SceneLoadManager.finishedLoading) {
            await GameplayManager.Update();
        }
    }

    private void LateUpdate() {
        if (SceneLoadManager.finishedLoading) {
            m_cameraTransform.position = GameplayManager.hero.transform.position + m_cameraOffset;
        }
    }
}
