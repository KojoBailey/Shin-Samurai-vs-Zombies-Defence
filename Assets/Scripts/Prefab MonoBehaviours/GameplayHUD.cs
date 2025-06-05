using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class GameplayHUD : MonoBehaviour { // Gameplay Heads-Up Display
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject pauseMenu;
    private CanvasGroup pauseMenuCanvasGroup;

    [SerializeField] private Image heroIcon;
    [SerializeField] private RectMask2D healthBarMask;
    [SerializeField] private Image healthBarImage;
    private const float healthBarWidth = 350;
    private float healthBarTargetPadding;
    private Color healthBarTargetColour;

    private const float SLOT_SPACING = 230;

    [SerializeField] private GameObject allySlotReference;
    [SerializeField] private Image allyIconReference;
    [SerializeField] private Image cooldownReference;
    private List<GameObject> allyButtons;
    [SerializeField] private TextMeshProUGUI smithyText;

    [SerializeField] private GameObject abilitySlotReference;
    [SerializeField] private Image abilityCooldownReference;
    private List<GameObject> abilityButtons;

    private async void Start() {
        pauseMenu.SetActive(false);
        pauseMenuCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();

        var handle = Addressables.LoadAssetAsync<Sprite>($"Textures/Icons/{SaveManager.selectedHero}");
        heroIcon.sprite = await handle.Task;
        if (heroIcon.sprite == null) {
            Debug.LogError($"Could not load icon texture for {SaveManager.selectedHero}");
        }

        allyButtons = new List<GameObject>();
        for (int i = 0; i < GameplayManager.instance.equippedAllies.Count; i++) {
            GameObject allyButton = Instantiate(allySlotReference, allySlotReference.transform.parent);
            AllyData allyData = GameplayManager.instance.equippedAllies[i];
            allyButton.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = allyData.icon;
            allyButton.transform.Find("Cooldown").gameObject.GetComponent<Image>().sprite = allyData.icon;
            allyButton.GetComponent<RectTransform>().localPosition = new Vector3(
                SLOT_SPACING * i, 0, 0);
            UIManager.AddEventTrigger(allyData.id, i, allyButton, EventTriggerType.PointerClick, AllySlotOnPointerClick);
            allyButtons.Add(allyButton);
        }
        allySlotReference.SetActive(false);

        abilityButtons = new List<GameObject>();
        for (int i = 0; i < GameplayManager.instance.equippedAbilities.Count; i++) {
            GameObject abilityButton = Instantiate(abilitySlotReference, abilitySlotReference.transform.parent);
            AbilityData abilityData = GameplayManager.instance.equippedAbilities[i];
            abilityButton.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = abilityData.icon;
            abilityButton.transform.Find("Cooldown").gameObject.GetComponent<Image>().sprite = abilityData.icon;
            abilityButton.GetComponent<RectTransform>().localPosition = new Vector3(
                -SLOT_SPACING * (GameplayManager.instance.equippedAbilities.Count - 1 - i), 0, 0);
            UIManager.AddEventTrigger(abilityData.id, i, abilityButton, EventTriggerType.PointerClick, AbilitySlotOnPointerClick);
            abilityButtons.Add(abilityButton);
        }
        abilitySlotReference.SetActive(false);
    }

    public void PauseGameplay() {
        GameplayManager.Pause();
        pauseMenu.SetActive(true);
    }
    public void ResumeGameplay() {
        GameplayManager.Resume();
        pauseMenu.SetActive(false);
    }
    public void Quit() {
        GameplayManager.Terminate();
        SceneLoadManager.LoadScene("TitleScreen");
    }
    public void Restart() {
        GameplayManager.Resume();
        SceneLoadManager.LoadScene("Gameplay");
    }

    private void AllySlotOnPointerClick(string id, int index) {
        AllyData ally = GameplayManager.instance.equippedAllies[index];
        if (GameplayManager.instance.allyCooldowns[index] <= 0 
            && GameplayManager.instance.smithy >= ally.cost
        ) {
            GameplayManager.instance.SpawnAlly(ally);
            GameplayManager.instance.allyCooldowns[index] = ally.cooldown;
            GameplayManager.instance.smithy -= ally.cost;
        }
    }

    private void AbilitySlotOnPointerClick(string id, int index) {
        if (GameplayManager.instance.abilityCooldowns[index] <= 0) {
            GameplayManager.instance.abilityManager.QueueAbility(id);
            GameplayManager.instance.abilityCooldowns[index] = GameplayManager.instance.equippedAbilities[index].cooldown;
        }
    }

    private void Update() {
        if (GameplayManager.instance.waveStarted) {
            healthBarTargetPadding = healthBarWidth - GameplayManager.instance.hero.health / GameplayManager.instance.hero.data.maxHealth * healthBarWidth;
            healthBarMask.padding += new Vector4(0, 0, (healthBarTargetPadding - healthBarMask.padding.z) / 0.2f * Time.deltaTime, 0);
            healthBarTargetColour = HealthBar.LerpHSV(HealthBar.red, HealthBar.green, GameplayManager.instance.hero.health / GameplayManager.instance.hero.data.maxHealth);
            healthBarImage.color += (healthBarTargetColour - healthBarImage.color) / 0.2f * Time.deltaTime;

            for (int i = 0; i < allyButtons.Count; i++) {
                Image icon = allyButtons[i].transform.Find("Icon").gameObject.GetComponent<Image>();
                Image cooldown = allyButtons[i].transform.Find("Cooldown").gameObject.GetComponent<Image>();
                if (GameplayManager.instance.smithy < GameplayManager.instance.equippedAllies[i].cost) {
                    allyButtons[i].GetComponent<RectTransform>().localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    icon.color = new Color(0.3f, 0.3f, 0.3f);
                } else {
                    allyButtons[i].GetComponent<RectTransform>().localScale = new Vector3(0.9f, 0.9f, 0.9f);
                    icon.color = Color.white;
                }
                cooldown.fillAmount = GameplayManager.instance.allyCooldowns[i] / GameplayManager.instance.equippedAllies[i].cooldown;
            }
            smithyText.text = GameplayManager.instance.smithy.ToString();

            for (int i = 0; i < abilityButtons.Count; i++) {
                Image cooldown = abilityButtons[i].transform.Find("Cooldown").gameObject.GetComponent<Image>();
                cooldown.fillAmount = GameplayManager.instance.abilityCooldowns[i] / GameplayManager.instance.equippedAbilities[i].cooldown;
            }
        }
    }
}
