using UnityEngine;
using System.Threading.Tasks;

public class Gameplay : MonoBehaviour {
    [SerializeField] private Transform m_cameraTransform;
    private readonly Vector3 CAMERA_OFFSET = new Vector3(0f, 1.39f, -5.07f);
    private readonly Quaternion CAMERA_ROTATION = Quaternion.Euler(4.94f, 0f, 0f);

    private void Start()
    {
        _ = StartAsync();
    }
    
    private async Task StartAsync()
    {
        m_cameraTransform.rotation = CAMERA_ROTATION;
        await GameplayManager.Init(m_cameraTransform);
        GameplayManager.instance.StartWave();

        SceneLoadManager.FinishLoading();
    }
    
    private void Update() 
    {
        if (SceneLoadManager.finishedLoading && !GameplayManager.instance.paused) {
            GameplayManager.instance.Update();
        }
    }

    private void LateUpdate() 
    {
        if (SceneLoadManager.finishedLoading) {
            if (!GameplayManager.instance.startSlowMo) {
                m_cameraTransform.position = GameplayManager.instance.hero.transform.position + CAMERA_OFFSET;
            } else {
                m_cameraTransform.position = new Vector3(
                    GameplayManager.instance.hero.xPos + CAMERA_OFFSET.x,
                    GameplayManager.instance.hero.transform.position.y + CAMERA_OFFSET.y,
                    m_cameraTransform.position.z
                );
            }
        }
    }
}
