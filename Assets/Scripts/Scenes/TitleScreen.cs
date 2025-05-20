using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleScreen : MonoBehaviour { // Title Screen
    [SerializeField] private Canvas m_canvas;
    [SerializeField] private Image m_bloodBottomRef;
    [SerializeField] private Image m_bloodTopRef;
    [SerializeField] private Image m_samurai;
    [SerializeField] private Image m_ronin;
    [SerializeField] private Image m_kunoichi;
    [SerializeField] private Image m_daimyo;
    [SerializeField] private Image m_onmyoji;
    [SerializeField] private RectTransform m_logoTransform;
    [SerializeField] private Image m_settingsButton;
    [SerializeField] private TextMeshProUGUI m_versionText;
    [SerializeField] private RectTransform m_startTextTransform;

    private GameObject[] m_bloodBottom = new GameObject[4];
    private GameObject[] m_bloodTop = new GameObject[4];
    private float m_bloodScrollSpeed = 300;
    private float m_bloodWidth;
    private float m_bloodPos;

    private Dictionary<string, Image> m_heroes = new();
    private Dictionary<string, Vector2> m_originalHeroPositions = new();
    private Dictionary<string, Vector2> m_targetHeroPositions = new();
    
    private Vector2 m_targetLogoPos;
    private Vector2 m_targetSettingsScale;

    private async void Start() {
        // Initialise managers.
        SFXManager.Init();
        await AllyManager.Init();
        SaveManager.EquipCostume("Samurai", 0);

        // Blood scroll animation
        m_bloodPos = 0;
        m_bloodWidth = m_bloodBottomRef.rectTransform.rect.width - 3;
        for (int i = 0; i < 4; i++) {
            m_bloodBottom[i] = new GameObject("Blood Bottom " + i);
            m_bloodBottom[i].transform.SetParent(m_canvas.transform, false);
            m_bloodBottom[i].transform.SetSiblingIndex(m_samurai.transform.GetSiblingIndex() + 1);

            Image img = m_bloodBottom[i].AddComponent<Image>();
            img.sprite = m_bloodBottomRef.sprite;

            RectTransform rectTransform = m_bloodBottom[i].GetComponent<RectTransform>();
            UIManager.CopyRectTransform(m_bloodBottomRef.rectTransform, rectTransform);
            rectTransform.anchoredPosition -= new Vector2(i * m_bloodWidth, 0);
        }
        for (int i = 0; i < 4; i++) {
            m_bloodTop[i] = new GameObject("Blood Top " + i);
            m_bloodTop[i].transform.SetParent(m_canvas.transform, false);
            m_bloodTop[i].transform.SetSiblingIndex(m_samurai.transform.GetSiblingIndex() + 1);

            Image img = m_bloodTop[i].AddComponent<Image>();
            img.sprite = m_bloodTopRef.sprite;

            RectTransform rectTransform = m_bloodTop[i].GetComponent<RectTransform>();
            UIManager.CopyRectTransform(m_bloodTopRef.rectTransform, rectTransform);
            rectTransform.anchoredPosition += new Vector2(i * m_bloodWidth, 0);
        }

        // Save m_logoTransform position.
        m_targetLogoPos = m_logoTransform.anchoredPosition;
        UIManager.SendOffScreen(m_logoTransform, UIManager.Direction.Up);

        // Hero hover
        m_heroes.Add("Samurai", m_samurai);
        m_heroes.Add("Ronin", m_ronin);
        m_heroes.Add("Kunoichi", m_kunoichi);
        m_heroes.Add("Daimyo", m_daimyo);
        m_heroes.Add("Onmyoji", m_onmyoji);
        foreach (var pair in m_heroes) {
            m_originalHeroPositions.Add(pair.Key, pair.Value.rectTransform.anchoredPosition);
            m_targetHeroPositions.Add(pair.Key, m_originalHeroPositions[pair.Key]);
            UIManager.AddEventTrigger(pair.Key, pair.Value.gameObject, EventTriggerType.PointerEnter, HeroOnPointerEnter);
            UIManager.AddEventTrigger(pair.Key, pair.Value.gameObject, EventTriggerType.PointerExit, HeroOnPointerExit);
        }

        // Settings button
        m_targetSettingsScale = new Vector2(1f, 1f);
        UIManager.AddEventTrigger("settings", m_settingsButton.gameObject, EventTriggerType.PointerEnter, SettingsOnPointerEnter);
        UIManager.AddEventTrigger("settings", m_settingsButton.gameObject, EventTriggerType.PointerExit, SettingsOnPointerExit);

        // Set version text to game version.
        m_versionText.text = "v" + Application.version;
        if (Debug.isDebugBuild)
            m_versionText.text += "-D";

        SceneLoadManager.FinishLoading();
    }

    private void HeroOnPointerEnter(string id) {
        m_targetHeroPositions[id] = m_originalHeroPositions[id] + new Vector2(5, 5);
    }
    private void HeroOnPointerExit(string id) {
        m_targetHeroPositions[id] = m_originalHeroPositions[id];
    }

    private void SettingsOnPointerEnter(string id) {
        m_targetSettingsScale = new Vector2(1.1f, 1.1f);
    }
    private void SettingsOnPointerExit(string id) {
        m_targetSettingsScale = new Vector2(1.0f, 1.0f);
    }


    private void Update() {
        if (SceneLoadManager.finishedLoading) {
            // Update blood scroll positions.
            m_bloodPos += m_bloodScrollSpeed * Time.deltaTime;
            m_bloodPos %= m_bloodWidth;
            for (int i = 0; i < 4; i++) {
                RectTransform rectTransform = m_bloodBottom[i].GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(
                    m_bloodBottomRef.rectTransform.anchoredPosition.x + m_bloodPos - m_bloodWidth * i,
                    rectTransform.anchoredPosition.y
                );
            }
            for (int i = 0; i < 4; i++) {
                RectTransform rectTransform = m_bloodTop[i].GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(
                    m_bloodTopRef.rectTransform.anchoredPosition.x - m_bloodPos + m_bloodWidth * i,
                    rectTransform.anchoredPosition.y
                );
            }

            // Update Hero positions.
            foreach (var pair in m_heroes) {
                UIManager.SmoothPos(pair.Value.GetComponent<RectTransform>(), m_targetHeroPositions[pair.Key]);
            }

            // Update m_logoTransform position;
            UIManager.SmoothPos(m_logoTransform, m_targetLogoPos, 0.7f);

            // Update settings button scale.
            UIManager.SmoothScale(m_settingsButton.GetComponent<RectTransform>(), m_targetSettingsScale);

            // Update start text scale.
            UIManager.SinScale(m_startTextTransform, new Vector2(1f, 1f), new Vector2(1.2f, 1.2f), 1);

            // Click/tap anywhere to proceed to next scene.
            if (Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Return))
                SceneLoadManager.LoadScene("Gameplay");
        }
    }
}
