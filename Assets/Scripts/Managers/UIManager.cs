using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager {
    public const float screenWidth = 1920;
    public const float screenHeight = 1080
    ;
    public static EventTrigger Trigger;
    
    public const float HoverSmoothSlow = 0.15f;

    public enum Direction {
        Up, Down, Left, Right
    };

    public static void CopyRectTransform(RectTransform source, RectTransform target) {
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.localPosition = source.localPosition;
    }

    public static void AddEventTrigger(string id, GameObject button, EventTriggerType type, Action<string> func) {
        Trigger = button.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => func(id));
        Trigger.triggers.Add(entry);
    }

    public static void SendOffScreen(RectTransform rectTransform, Direction direction) {
        switch (direction) {
            case Direction.Up:
                rectTransform.anchoredPosition = new Vector2(
                    rectTransform.anchoredPosition.x,
                    screenHeight / 2 + rectTransform.rect.height / 2
                );
                return;
        }
    }

    // Gradually and smoothly change the scale of an object.
    public static void SmoothScale(RectTransform rectTransform, Vector2 target) {
        Vector3 targetScale = new Vector3(target.x, target.y, 1);
        rectTransform.localScale += (targetScale - rectTransform.localScale) / HoverSmoothSlow * Time.deltaTime;
    }
    // Gradually and smoothly change the scale of an object, with a custom slow amount.
    public static void SmoothScale(RectTransform rectTransform, Vector2 target, float customSlow) {
        Vector3 targetScale = new Vector3(target.x, target.y, 1);
        rectTransform.localScale += (targetScale - rectTransform.localScale) / customSlow * Time.deltaTime;
    }

    // Gradually and smoothly change the position of an object.
    public static void SmoothPos(RectTransform rectTransform, Vector2 target) {
        rectTransform.anchoredPosition += (target - rectTransform.anchoredPosition) / HoverSmoothSlow * Time.deltaTime;
    }
    // Gradually and smoothly change the position of an object, with a custom slow amount.
    public static void SmoothPos(RectTransform rectTransform, Vector2 target, float customSlow) {
        rectTransform.anchoredPosition += (target - rectTransform.anchoredPosition) / customSlow * Time.deltaTime;
    }

    public static void SinScale(RectTransform rectTransform, Vector2 minScale, Vector2 maxScale, float speed) {
        float targetScaleX = minScale.x + (float)((1 + Math.Sin(Time.time * speed)) / (2 / (maxScale.x - 1)));
        float targetScaleY = minScale.y + (float)((1 + Math.Sin(Time.time * speed)) / (2 / (maxScale.y - 1)));
        rectTransform.localScale = new Vector3(targetScaleX, targetScaleY, 1);
    }
    public static void SinScale(RectTransform rectTransform, Vector2 minScale, Vector2 maxScale, float speed, float offset) {
        float targetScaleX = minScale.x + (float)((1 + Math.Sin((Time.time + offset) * speed)) / (2 / (maxScale.x - 1)));
        float targetScaleY = minScale.y + (float)((1 + Math.Sin((Time.time + offset) * speed)) / (2 / (maxScale.y - 1)));
        rectTransform.localScale = new Vector3(targetScaleX, targetScaleY, 1);
    }
}
