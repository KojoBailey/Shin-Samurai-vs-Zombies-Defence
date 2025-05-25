using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

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

    [SerializeField] private GameObject m_abilitySlotReference;
    [SerializeField] private Image m_abilityCooldownReference;
    private List<GameObject> m_abilityButtons;

    private async void Start() {
        var handle = Addressables.LoadAssetAsync<Sprite>($"Textures/Icons/{SaveManager.selectedHero}");
        m_heroIcon.sprite = await handle.Task;
        if (m_heroIcon.sprite == null) {
            Debug.LogError($"Could not load icon texture for {SaveManager.selectedHero}");
        }

        UIManager.AddEventTrigger("AllySlot1", m_cooldownReference.gameObject, EventTriggerType.PointerClick, AllySlotOnPointerClick);

        m_abilityButtons = new List<GameObject>();
        for (int i = 0; i < GameplayManager.equippedAbilities.Count; i++) {
            GameObject abilityButton = Instantiate(m_abilitySlotReference, m_abilitySlotReference.transform.parent);
            AbilityData abilityData = GameplayManager.equippedAbilities[i];
            abilityButton.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = abilityData.icon;
            abilityButton.transform.Find("Cooldown").gameObject.GetComponent<Image>().sprite = abilityData.icon;
            abilityButton.GetComponent<RectTransform>().localPosition = new Vector3(
                -300 * (GameplayManager.equippedAbilities.Count - 1 - i), 0, 0);
            UIManager.AddEventTrigger(abilityData.id, i, abilityButton, EventTriggerType.PointerClick, AbilitySlotOnPointerClick);
            m_abilityButtons.Add(abilityButton);
        }
        m_abilitySlotReference.SetActive(false);
    }

    private void AllySlotOnPointerClick(string id) {
        if (GameplayManager.allyCooldowns[0] <= 0 && GameplayManager.smithy >= AssetManager.alliesData[0].cost) {
            GameplayManager.SpawnAlly(AssetManager.alliesData[0]);
            GameplayManager.allyCooldowns[0] = AssetManager.alliesData[0].cooldown;
            GameplayManager.smithy -= AssetManager.alliesData[0].cost;
        }
    }

    private void AbilitySlotOnPointerClick(string id, int index) {
        if (GameplayManager.abilityCooldowns[index] <= 0) {
            AbilityManager.QueueAbility(id);
            GameplayManager.abilityCooldowns[index] = GameplayManager.equippedAbilities[index].cooldown;
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
            m_cooldownReference.fillAmount = GameplayManager.allyCooldowns[0] / AssetManager.alliesData[0].cooldown;
            m_smithyText.text = GameplayManager.smithy.ToString();

            for (int i = 0; i < m_abilityButtons.Count; i++) {
                Image cooldown = m_abilityButtons[i].transform.Find("Cooldown").gameObject.GetComponent<Image>();
                cooldown.fillAmount = GameplayManager.abilityCooldowns[i] / GameplayManager.equippedAbilities[i].cooldown;
            }
        }
    }
}
