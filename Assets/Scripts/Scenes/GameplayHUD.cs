using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class GameplayHUD : MonoBehaviour { // Gameplay Heads-Up Display
    [SerializeField] private Image m_heroIcon;
    [SerializeField] private RectMask2D m_healthBarMask;
    [SerializeField] private Image m_healthBarImage;
    private const float m_healthBarWidth = 350;
    private float m_healthBarTargetPadding;
    private Color m_healthBarTargetColour;

    private async void Start() {
        var handle = Addressables.LoadAssetAsync<Sprite>($"Textures/Icons/{SaveManager.selectedHero}");
        m_heroIcon.sprite = await handle.Task;
        if (m_heroIcon.sprite == null) {
            Debug.LogError($"Could not load icon texture for {SaveManager.selectedHero}");
        }
    }

    private void Update() {
        if (GameplayManager.initialised) {
            m_healthBarTargetPadding = m_healthBarWidth - GameplayManager.hero.health / GameplayManager.hero.data.health * m_healthBarWidth;
            m_healthBarMask.padding += new Vector4(0, 0, (m_healthBarTargetPadding - m_healthBarMask.padding.z) / 0.2f * Time.deltaTime, 0);
            m_healthBarTargetColour = HealthBar.LerpHSV(HealthBar.red, HealthBar.green, GameplayManager.hero.health / GameplayManager.hero.data.health);
            m_healthBarImage.color += (m_healthBarTargetColour - m_healthBarImage.color) / 0.2f * Time.deltaTime;
        }
    }
}
