using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleScreen : MonoBehaviour {
    [SerializeField] private Canvas sceneCanvas;
    [SerializeField] private Image bloodBottomRef;
    [SerializeField] private Image bloodTopRef;
    [SerializeField] private Image samurai;
    [SerializeField] private Image ronin;
    [SerializeField] private Image kunoichi;
    [SerializeField] private Image daimyo;
    [SerializeField] private Image onmyoji;
    [SerializeField] private RectTransform logo;
    [SerializeField] private Image settingsButton;
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private RectTransform startText;

    private GameObject[] bloodBottom = new GameObject[4];
    private GameObject[] bloodTop = new GameObject[4];
    private float bloodScrollSpeed = 300;
    private float bloodWidth;
    private float bloodPos;

    private Dictionary<string, Image> heroes = new();
    private Dictionary<string, Vector2> originalHeroPositions = new();
    private Dictionary<string, Vector2> targetHeroPositions = new();

    private Vector2 targetLogoPos;

    private Vector2 settingsScale;

    private float startTextScale;

    private async void Start() {
        // Initialise managers.
        SFXManager.Init();
        await AllyManager.Init();
        SaveManager.EquipCostume("Samurai", 0);

        // Blood scroll animation
        bloodPos = 0;
        bloodWidth = bloodBottomRef.rectTransform.rect.width - 3;
        for (int i = 0; i < 4; i++) {
            bloodBottom[i] = new GameObject("Blood Bottom " + i);
            bloodBottom[i].transform.SetParent(sceneCanvas.transform, false);
            bloodBottom[i].transform.SetSiblingIndex(samurai.transform.GetSiblingIndex() + 1);

            Image img = bloodBottom[i].AddComponent<Image>();
            img.sprite = bloodBottomRef.sprite;

            RectTransform rectTransform = bloodBottom[i].GetComponent<RectTransform>();
            UIManager.CopyRectTransform(bloodBottomRef.rectTransform, rectTransform);
            rectTransform.anchoredPosition -= new Vector2(i * bloodWidth, 0);
        }
        for (int i = 0; i < 4; i++) {
            bloodTop[i] = new GameObject("Blood Top " + i);
            bloodTop[i].transform.SetParent(sceneCanvas.transform, false);
            bloodTop[i].transform.SetSiblingIndex(samurai.transform.GetSiblingIndex() + 1);

            Image img = bloodTop[i].AddComponent<Image>();
            img.sprite = bloodTopRef.sprite;

            RectTransform rectTransform = bloodTop[i].GetComponent<RectTransform>();
            UIManager.CopyRectTransform(bloodTopRef.rectTransform, rectTransform);
            rectTransform.anchoredPosition += new Vector2(i * bloodWidth, 0);
        }

        // Save logo position.
        targetLogoPos = logo.anchoredPosition;
        UIManager.SendOffScreen(logo, UIManager.Direction.Up);

        // Hero hover
        heroes.Add("samurai", samurai);
        heroes.Add("ronin", ronin);
        heroes.Add("kunoichi", kunoichi);
        heroes.Add("daimyo", daimyo);
        heroes.Add("onmyoji", onmyoji);
        foreach (var pair in heroes) {
            originalHeroPositions.Add(pair.Key, pair.Value.rectTransform.anchoredPosition);
            targetHeroPositions.Add(pair.Key, originalHeroPositions[pair.Key]);
            UIManager.AddEventTrigger(pair.Key, pair.Value.gameObject, EventTriggerType.PointerEnter, HeroOnPointerEnter);
            UIManager.AddEventTrigger(pair.Key, pair.Value.gameObject, EventTriggerType.PointerExit, HeroOnPointerExit);
        }

        // Settings button
        settingsScale = new Vector2(1f, 1f);
        UIManager.AddEventTrigger("settings", settingsButton.gameObject, EventTriggerType.PointerEnter, SettingsOnPointerEnter);
        UIManager.AddEventTrigger("settings", settingsButton.gameObject, EventTriggerType.PointerExit, SettingsOnPointerExit);

        // Set version text to game version.
        versionText.text = "v" + Application.version;
        if (Debug.isDebugBuild)
            versionText.text += "-D";

        SceneLoadManager.FinishLoading();
    }

    private void HeroOnPointerEnter(string id) {
        targetHeroPositions[id] = originalHeroPositions[id] + new Vector2(5, 5);
    }
    private void HeroOnPointerExit(string id) {
        targetHeroPositions[id] = originalHeroPositions[id];
    }

    private void SettingsOnPointerEnter(string id) {
        settingsScale = new Vector2(1.1f, 1.1f);
    }
    private void SettingsOnPointerExit(string id) {
        settingsScale = new Vector2(1.0f, 1.0f);
    }


    private void Update() {
        if (SceneLoadManager.finishedLoading) {
            // Update blood scroll positions.
            bloodPos += bloodScrollSpeed * Time.deltaTime;
            bloodPos %= bloodWidth;
            for (int i = 0; i < 4; i++) {
                RectTransform rectTransform = bloodBottom[i].GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(
                    bloodBottomRef.rectTransform.anchoredPosition.x + bloodPos - bloodWidth * i,
                    rectTransform.anchoredPosition.y
                );
            }
            for (int i = 0; i < 4; i++) {
                RectTransform rectTransform = bloodTop[i].GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(
                    bloodTopRef.rectTransform.anchoredPosition.x - bloodPos + bloodWidth * i,
                    rectTransform.anchoredPosition.y
                );
            }

            // Update Hero positions.
            foreach (var pair in heroes) {
                UIManager.SmoothPos(pair.Value.GetComponent<RectTransform>(), targetHeroPositions[pair.Key]);
            }

            // Update logo position;
            UIManager.SmoothPos(logo, targetLogoPos, 0.7f);

            // Update settings button scale.
            UIManager.SmoothScale(settingsButton.GetComponent<RectTransform>(), settingsScale);

            // Update start text scale.
            UIManager.SinScale(startText, new Vector2(1f, 1f), new Vector2(1.2f, 1.2f), 1);

            // Click/tap anywhere to proceed to next scene.
            if (Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Return))
                SceneLoadManager.LoadScene("Gameplay");
        }
    }
}
