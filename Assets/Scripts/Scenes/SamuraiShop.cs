using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SamuraiShop : MonoBehaviour {
    [SerializeField] private Canvas sceneCanvas;
    [SerializeField] private RectTransform allyReference;

    private Dictionary<string, Ally> allies = new();
    private Dictionary<string, GameObject> allyButtons = new();
    private Dictionary<string, Vector2> targetScales = new();

    private async void Start() {
        int i = 0;
        foreach (string allyId in AllyManager.AllyIds) {
            Ally ally = await AllyManager.LoadAlly(allyId);
            
            GameObject allyButton = new GameObject("DynamicSprite");
            allyButton.transform.SetParent(sceneCanvas.transform, false);

            Image img = allyButton.AddComponent<Image>();
            img.sprite = ally.Icon;

            RectTransform rectTransform = allyButton.GetComponent<RectTransform>();
            UIManager.CopyRectTransform(allyReference, rectTransform);
            rectTransform.anchoredPosition += new Vector2(i++ * 275, 0);

            UIManager.AddEventTrigger(allyId, allyButton, EventTriggerType.PointerEnter, OnPointerEnter);
            UIManager.AddEventTrigger(allyId, allyButton, EventTriggerType.PointerExit, OnPointerExit);
            UIManager.AddEventTrigger(allyId, allyButton, EventTriggerType.PointerClick, OnPointerClick);

            allies.Add(allyId, ally);
            allyButtons.Add(allyId, allyButton);
            targetScales.Add(allyId, allyReference.localScale);
        }

        SceneLoadManager.FinishLoading();
    }

    private void OnPointerEnter(string id) {
        targetScales[id] = allyReference.localScale * new Vector2(1.2f, 1.2f);
    }
    private void OnPointerExit(string id) {
        targetScales[id] = allyReference.localScale;
    }
    private void OnPointerClick(string id) {
        Debug.Log(allies[id].DisplayName);
    }

    private void Update() {
        foreach (var pair in allyButtons) {
            UIManager.SmoothScale(pair.Value.GetComponent<RectTransform>(), targetScales[pair.Key]);
        }
    }
}
