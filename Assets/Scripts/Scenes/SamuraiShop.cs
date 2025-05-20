using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SamuraiShop : MonoBehaviour { // Samurai Shop
    [SerializeField] private Canvas m_sceneCanvas;
    [SerializeField] private RectTransform m_allyReference;

    private Dictionary<string, AllyData> m_allies = new();
    private Dictionary<string, GameObject> m_allyButtons = new();
    private Dictionary<string, Vector2> m_targetScales = new();

    private async void Start() {
        int i = 0;
        foreach (string allyId in AllyManager.allyIds) {
            AllyData ally = await AllyManager.LoadAlly(allyId);
            
            GameObject allyButton = new GameObject("DynamicSprite");
            allyButton.transform.SetParent(m_sceneCanvas.transform, false);

            Image img = allyButton.AddComponent<Image>();
            img.sprite = ally.icon;

            RectTransform rectTransform = allyButton.GetComponent<RectTransform>();
            UIManager.CopyRectTransform(m_allyReference, rectTransform);
            rectTransform.anchoredPosition += new Vector2(i++ * 275, 0);

            UIManager.AddEventTrigger(allyId, allyButton, EventTriggerType.PointerEnter, OnPointerEnter);
            UIManager.AddEventTrigger(allyId, allyButton, EventTriggerType.PointerExit, OnPointerExit);
            UIManager.AddEventTrigger(allyId, allyButton, EventTriggerType.PointerClick, OnPointerClick);

            m_allies.Add(allyId, ally);
            m_allyButtons.Add(allyId, allyButton);
            m_targetScales.Add(allyId, m_allyReference.localScale);
        }

        SceneLoadManager.FinishLoading();
    }

    private void OnPointerEnter(string id) {
        m_targetScales[id] = m_allyReference.localScale * new Vector2(1.2f, 1.2f);
    }
    private void OnPointerExit(string id) {
        m_targetScales[id] = m_allyReference.localScale;
    }
    private void OnPointerClick(string id) {
        Debug.Log(m_allies[id].displayName);
    }

    private void Update() {
        foreach (var pair in m_allyButtons) {
            UIManager.SmoothScale(pair.Value.GetComponent<RectTransform>(), m_targetScales[pair.Key]);
        }
    }
}
