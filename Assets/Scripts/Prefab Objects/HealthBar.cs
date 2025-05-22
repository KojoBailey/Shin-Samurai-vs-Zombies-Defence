using UnityEngine;
using UnityEngine.UI;

public class HealthBar { // Health Bar
    public GameplayEntity entity;
    public float maxHealth;
    private RectMask2D m_mask;
    private GameObject m_canvas;
    private EntityHealthBar m_textures;
    private float m_width;
    private float m_targetPadding;
    public static Color green = new Color(0f, 0.9f, 0f);
    public static Color red = new Color(1f, 0f, 0f);

    private bool m_initialised = false;

    public HealthBar(GameObject prefab, GameplayEntity _entity, float _maxHealth) {
        entity = _entity;
        maxHealth = _maxHealth;
        m_canvas = Object.Instantiate(prefab, entity.transform);
        m_mask = m_canvas.GetComponent<RectMask2D>();
        m_width = m_canvas.GetComponent<RectTransform>().rect.width;

        m_textures = m_canvas.GetComponent<EntityHealthBar>();
        m_textures.back.color = new Color(0.2f, 0.2f, 0.2f); // Dark grey
        if (entity.allegiance == GameplayEntity.Side.Left)
            m_textures.front.color = green;
        else m_textures.front.color = red;

        m_canvas.transform.localPosition = new Vector3(0f, 1.5f, 0.15f);
        m_canvas.transform.localRotation = Quaternion.Euler(0f, -90f * entity.direction, 0f);
        m_canvas.GetComponent<Canvas>().sortingOrder = 999; // Send to front.

        m_initialised = true;
    }

    public void Update() {
        if (m_initialised) {
            // Update mask.
            m_targetPadding = m_width - entity.health / maxHealth * m_width;
            m_mask.padding += new Vector4(0, 0, (m_targetPadding - m_mask.padding.z) / 0.2f * Time.deltaTime, 0);
        }
    }

    public static Color LerpHSV(Color a, Color b, float t) {
        Color.RGBToHSV(a, out float h1, out float s1, out float v1);
        Color.RGBToHSV(b, out float h2, out float s2, out float v2);

        // Interpolate each component separately.
        float h = Mathf.LerpAngle(h1 * 360f, h2 * 360f, t) / 360f; // Interpolate hue correctly across 360 degrees.
        float s = Mathf.Lerp(s1, s2, t);
        float v = Mathf.Lerp(v1, v2, t);

        return Color.HSVToRGB(h, s, v);
    }
};