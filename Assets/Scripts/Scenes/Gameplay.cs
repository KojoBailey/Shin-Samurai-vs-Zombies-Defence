using System.Threading.Tasks;
using UnityEngine;

public class Gameplay : MonoBehaviour {
    [SerializeField] private Transform m_cameraTransform;
    private Vector3 m_cameraOffset = new Vector3(0f, 1.39f, -5.07f);
    private Quaternion m_cameraRotation = Quaternion.Euler(4.94f, 0f, 0f);

    private async void Start() {
        m_cameraTransform.rotation = m_cameraRotation;
        await GameplayManager.Init(m_cameraTransform);
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
                m_cameraTransform.position = GameplayManager.instance.hero.transform.position + m_cameraOffset;
                // !! change to just heroX
        }
    }
}
