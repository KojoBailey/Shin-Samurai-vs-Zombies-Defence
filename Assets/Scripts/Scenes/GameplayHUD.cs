using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor.Experimental.GraphView;

public class GameplayHUD : MonoBehaviour { // Gameplay Heads-Up Display
    [SerializeField] private Image m_heroIcon;
    [SerializeField] private RectMask2D m_healthBarMask;
    [SerializeField] private Image m_healthBarImage;
    private const float m_healthBarWidth = 350;
    private float m_healthBarTargetPadding;
    private Color m_healthBarTargetColour;

    [SerializeField] private GameObject m_allySlotReference;
    [SerializeField] private Image m_allyIconReference;
    [SerializeField] private Image m_cooldownReference;
    [SerializeField] private TextMeshProUGUI m_smithyText;

    private async void Start() {
        var handle = Addressables.LoadAssetAsync<Sprite>($"Textures/Icons/{SaveManager.selectedHero}");
        m_heroIcon.sprite = await handle.Task;
        if (m_heroIcon.sprite == null) {
            Debug.LogError($"Could not load icon texture for {SaveManager.selectedHero}");
        }

        UIManager.AddEventTrigger("AllySlot1", m_cooldownReference.gameObject, EventTriggerType.PointerClick, AllySlotOnPointerClick);
    }

    private void AllySlotOnPointerClick(string id) {
        if (GameplayManager.allyCooldowns[0] <= 0 && GameplayManager.smithy >= AssetManager.alliesData[0].cost) {
            GameplayManager.SpawnAlly(AssetManager.alliesData[0]);
            GameplayManager.allyCooldowns[0] = AssetManager.alliesData[0].cooldown;
            GameplayManager.smithy -= AssetManager.alliesData[0].cost;
            AbilityManager.Lethargy();
        }
    }

    private void Update() {
        if (GameplayManager.initialised) {
            m_healthBarTargetPadding = m_healthBarWidth - GameplayManager.hero.health / GameplayManager.hero.data.health * m_healthBarWidth;
            m_healthBarMask.padding += new Vector4(0, 0, (m_healthBarTargetPadding - m_healthBarMask.padding.z) / 0.2f * Time.deltaTime, 0);
            m_healthBarTargetColour = HealthBar.LerpHSV(HealthBar.red, HealthBar.green, GameplayManager.hero.health / GameplayManager.hero.data.health);
            m_healthBarImage.color += (m_healthBarTargetColour - m_healthBarImage.color) / 0.2f * Time.deltaTime;

            if (GameplayManager.smithy < AssetManager.alliesData[0].cost) {
                m_allySlotReference.GetComponent<RectTransform>().localScale = new Vector3(0.9f, 0.9f, 0.9f);
                m_allyIconReference.color = new Color(0.3f, 0.3f, 0.3f);
            } else {
                m_allySlotReference.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                m_allyIconReference.color = Color.white;
            }
            m_cooldownReference.fillAmount = GameplayManager.allyCooldowns[0];
            m_smithyText.text = GameplayManager.smithy.ToString();
        }
    }
}
