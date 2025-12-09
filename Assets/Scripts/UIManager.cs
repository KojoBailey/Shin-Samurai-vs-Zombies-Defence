using System;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIUtils
{
    public const float ScreenWidth = 1920f;
    public const float ScreenHeight = 1080f;
    
    private const float HoverSmoothSlow = 0.15f;

    public enum Direction { Up, Down, Left, Right };

    public static void CopyRectTransform(RectTransform source, RectTransform target)
    {
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.localPosition = source.localPosition;
    }

    public static void AddEventTrigger(
        string id,
        GameObject button,
        EventTriggerType type,
        Action<string> func
    ) {
        var trigger = button.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => func(id));
        trigger.triggers.Add(entry);
    }
    
    public static void AddEventTrigger(
        string id,
        int index,
        GameObject button,
        EventTriggerType type,
        Action<string, int> func
    ) {
        var trigger = button.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => func(id, index));
        trigger.triggers.Add(entry);
    }

    public static void SendOffscreen(RectTransform rectTransform, Direction direction)
    {
        var newPos = rectTransform.anchoredPosition;
        
        switch (direction) {
        case Direction.Up:
            newPos.y = (ScreenHeight + rectTransform.rect.height) / 2;
            break;
        case Direction.Down:
            newPos.y = (ScreenHeight - rectTransform.rect.height) / 2;
            break;
        case Direction.Left:
            newPos.x = (ScreenWidth - rectTransform.rect.width) / 2;
            break;
        case Direction.Right:
            newPos.x = (ScreenWidth + rectTransform.rect.width) / 2;
            break;
        default:
            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        rectTransform.anchoredPosition = newPos;
    }
    
    public static void SmoothScale(
        RectTransform rectTransform,
        Vector2 target,
        float slow = HoverSmoothSlow
    ) {
        var targetScale = new Vector3(target.x, target.y, 1);
        rectTransform.localScale += (targetScale - rectTransform.localScale) / slow * Time.deltaTime;
    }
    
    public static void SmoothPos(
        RectTransform rectTransform,
        Vector2 target,
        float slow = HoverSmoothSlow
    ) {
        rectTransform.anchoredPosition += (target - rectTransform.anchoredPosition) / slow * Time.deltaTime;
    }
    
    public static void SinScale(
        RectTransform rectTransform,
        Vector2 minScale,
        Vector2 maxScale,
        float speed,
        float offset = 0
    ) {
        rectTransform.localScale = new Vector3(
            minScale.x + (float)((1 + Math.Sin((Time.time + offset) * speed)) / (2 / (maxScale.x - 1))),
            minScale.y + (float)((1 + Math.Sin((Time.time + offset) * speed)) / (2 / (maxScale.y - 1))),
            1
        );
    }
}
