using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class Gameplay : MonoBehaviour {
    [FormerlySerializedAs("m_cameraTransform")]
        [SerializeField] private Transform cameraTransform;
    private readonly Vector3 _cameraOffset = new Vector3(0f, 1.39f, -5.07f);
    private readonly Quaternion _cameraRotation = Quaternion.Euler(4.94f, 0f, 0f);

    private void Start()
    {
        _ = StartAsync();
    }
    
    private async Task StartAsync()
    {
        cameraTransform.rotation = _cameraRotation;
        await GameplayManager.Init(cameraTransform);
        GameplayManager.instance.StartWave();

        SceneLoader.FinishLoading();
    }
    
    private void Update() 
    {
        if (SceneLoader.hasFinishedLoading && !GameplayManager.instance.paused) {
            GameplayManager.instance.Update();
        }
    }

    private void LateUpdate() 
    {
        if (SceneLoader.hasFinishedLoading) {
            if (!GameplayManager.instance.startSlowMo) {
                cameraTransform.position = GameplayManager.instance.hero.transform.position + _cameraOffset;
            } else {
                cameraTransform.position = new Vector3(
                    GameplayManager.instance.hero.xPos + _cameraOffset.x,
                    GameplayManager.instance.hero.transform.position.y + _cameraOffset.y,
                    cameraTransform.position.z
                );
            }
        }
    }
}
