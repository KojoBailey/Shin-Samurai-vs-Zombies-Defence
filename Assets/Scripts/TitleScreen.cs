using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [FormerlySerializedAs("m_canvas")]
        [SerializeField] private Canvas canvas;
    [FormerlySerializedAs("m_bloodBottomRef")]
        [SerializeField] private Image bloodBottomRef;
    [FormerlySerializedAs("m_bloodTopRef")] 
        [SerializeField] private Image bloodTopRef;
    [FormerlySerializedAs("m_samurai")] 
        [SerializeField] private Image samurai;
    [FormerlySerializedAs("m_ronin")] 
        [SerializeField] private Image ronin;
    [FormerlySerializedAs("m_kunoichi")] 
        [SerializeField] private Image kunoichi;
    [FormerlySerializedAs("m_daimyo")]
        [SerializeField] private Image daimyo;
    [FormerlySerializedAs("m_onmyoji")]
        [SerializeField] private Image onmyoji;
    [FormerlySerializedAs("m_logoTransform")]
        [SerializeField] private RectTransform logoTransform;
    [FormerlySerializedAs("m_settingsButton")]
        [SerializeField] private Image settingsButton;
    private RectTransform _settingsButtonRect;
    [FormerlySerializedAs("m_versionText")]
        [SerializeField] private TextMeshProUGUI versionText;
    [FormerlySerializedAs("m_startTextTransform")]
        [SerializeField] private RectTransform startTextTransform;

    private readonly GameObject[] _bloodBottom = new GameObject[4];
    private readonly RectTransform[] _bloodBottomRect = new RectTransform[4];
    private readonly GameObject[] _bloodTop = new GameObject[4];
    private readonly RectTransform[] _bloodTopRect = new RectTransform[4];
    private const float BloodScrollSpeed = 300f;
    private float _bloodWidth;
    private float _bloodPos;

    private readonly Dictionary<string, Image> _heroes = new();
    private readonly Dictionary<string, Vector2> _originalHeroPositions = new();
    private readonly Dictionary<string, Vector2> _targetHeroPositions = new();
    
    private Vector2 _targetLogoPos;
    private Vector2 _targetSettingsScale;

    private void Awake()
    {
        _settingsButtonRect = settingsButton.GetComponent<RectTransform>();
        
        _bloodPos = 0;
        _bloodWidth = bloodBottomRef.rectTransform.rect.width - 3;
    }
    
    private void Start()
    {
        SFXManager.Init();
        
        PrepareBlood();
        
        PrepareHeroes();
        
        PrepareSettingsButton();
        
        PrepareLogo();

        PrepareVersionText();

        SceneLoadManager.FinishLoading();
    }
    
    private void Update()
    {
        if (!SceneLoadManager.hasFinishedLoading) return;

        UpdateBlood();

        UpdateHeroes();

        UpdateLogo();

        UpdateSettingsButton();

        UpdateStartText();
        
        if (Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Return))
        {
            SceneLoadManager.LoadScene("Gameplay");
        }
    }
    
    private void PrepareBlood()
    {
        for (uint i = 0; i < 4; i++)
        {
            _bloodBottom[i] = new GameObject("Blood Bottom " + i);
            _bloodBottom[i].transform.SetParent(canvas.transform, false);
            _bloodBottom[i].transform.SetSiblingIndex(samurai.transform.GetSiblingIndex() + 1);
            
            _bloodBottom[i].AddComponent<Image>().sprite = bloodBottomRef.sprite;

            _bloodBottomRect[i] = _bloodBottom[i].GetComponent<RectTransform>();
            UIUtils.CopyRectTransform(bloodBottomRef.rectTransform, _bloodBottomRect[i]);
            _bloodBottomRect[i].anchoredPosition -= new Vector2(i * _bloodWidth, 0);
        }
        for (uint i = 0; i < 4; i++)
        {
            _bloodTop[i] = new GameObject("Blood Top " + i);
            _bloodTop[i].transform.SetParent(canvas.transform, false);
            _bloodTop[i].transform.SetSiblingIndex(samurai.transform.GetSiblingIndex() + 1);

            _bloodTop[i].AddComponent<Image>().sprite = bloodTopRef.sprite;

            _bloodTopRect[i] = _bloodTop[i].GetComponent<RectTransform>();
            UIUtils.CopyRectTransform(bloodTopRef.rectTransform, _bloodTopRect[i]);
            _bloodTopRect[i].anchoredPosition += new Vector2(i * _bloodWidth, 0);
        }
    }

    private void PrepareHeroes()
    {
        _heroes.Add("Samurai", samurai);
        _heroes.Add("Ronin", ronin);
        _heroes.Add("Kunoichi", kunoichi);
        _heroes.Add("Daimyo", daimyo);
        _heroes.Add("Onmyoji", onmyoji);
        foreach (var pair in _heroes)
        {
            _originalHeroPositions.Add(pair.Key, pair.Value.rectTransform.anchoredPosition);
            _targetHeroPositions.Add(pair.Key, _originalHeroPositions[pair.Key]);
            UIUtils.AddEventTrigger(
                pair.Key, pair.Value.gameObject, EventTriggerType.PointerEnter, HeroOnPointerEnter);
            UIUtils.AddEventTrigger(
                pair.Key, pair.Value.gameObject, EventTriggerType.PointerExit, HeroOnPointerExit);
        }
    }
    
    private void PrepareSettingsButton()
    {
        ResetSettingsScale();
        UIUtils.AddEventTrigger(
            "settings", settingsButton.gameObject, EventTriggerType.PointerEnter, SettingsOnPointerEnter);
        UIUtils.AddEventTrigger(
            "settings", settingsButton.gameObject, EventTriggerType.PointerExit, SettingsOnPointerExit);
    }

    private void PrepareLogo()
    {
        _targetLogoPos = logoTransform.anchoredPosition;
        UIUtils.SendOffscreen(logoTransform, UIUtils.Direction.Up);
    }

    private void PrepareVersionText()
    {
        versionText.text = $"v{Application.version}";
        if (Debug.isDebugBuild)
        {
            versionText.text += "-D";
        }
    }
    
    private void HeroOnPointerEnter(string id)
    {
        _targetHeroPositions[id] = _originalHeroPositions[id] + new Vector2(5, 5);
    }

    private void HeroOnPointerExit(string id)
    {
        _targetHeroPositions[id] = _originalHeroPositions[id];
    }

    private void SettingsOnPointerEnter(string id)
    {
        _targetSettingsScale = new Vector2(1.1f, 1.1f);
    }

    private void SettingsOnPointerExit(string id)
    {
        ResetSettingsScale();
    }
    
    private void ResetSettingsScale()
    {
        _targetSettingsScale = new Vector2(1.0f, 1.0f);
    }

    private void UpdateBlood()
    {
        _bloodPos += BloodScrollSpeed * Time.deltaTime;
        _bloodPos %= _bloodWidth;
        for (uint i = 0; i < 4; i++)
        {
            _bloodBottomRect[i].anchoredPosition = new Vector2(
                bloodBottomRef.rectTransform.anchoredPosition.x + _bloodPos - _bloodWidth * i,
                _bloodBottomRect[i].anchoredPosition.y
            );
        }
        for (uint i = 0; i < 4; i++)
        {
            _bloodTopRect[i].anchoredPosition = new Vector2(
                bloodTopRef.rectTransform.anchoredPosition.x - _bloodPos + _bloodWidth * i,
                _bloodTopRect[i].anchoredPosition.y
            );
        }
    }
    
    private void UpdateHeroes()
    {
        foreach (var pair in _heroes)
        {
            UIUtils.SmoothPos(pair.Value.rectTransform, _targetHeroPositions[pair.Key]);
        }
    }

    private void UpdateLogo()
    {
        UIUtils.SmoothPos(logoTransform, _targetLogoPos, 0.7f);
    }
    
    private void UpdateSettingsButton()
    {
        UIUtils.SmoothScale(_settingsButtonRect, _targetSettingsScale);
    }

    private void UpdateStartText()
    {
        UIUtils.SinScale(
            startTextTransform, new Vector2(1f, 1f), new Vector2(1.2f, 1.2f), 1);
    }
}