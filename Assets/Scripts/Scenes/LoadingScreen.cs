using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour { // Loading Screen
    [SerializeField] private GameObject m_clock;
    private const float m_clockSpeed = 900;

    [SerializeField] private TextMeshProUGUI m_tipText;

    private void Start() {
        DontDestroyOnLoad(gameObject);
        m_tipText.text = "Abilities can come in handy at the right moment, but beware of their cooldowns!";
    }

    private void Update() {
        Transform shortHand = m_clock.transform.Find("Short Hand");
        Transform longHand = m_clock.transform.Find("Long Hand");
        shortHand.GetComponent<RectTransform>().transform.Rotate(0, 0, -1 * m_clockSpeed / 12 * Time.deltaTime);
        longHand.GetComponent<RectTransform>().transform.Rotate(0, 0, -1 * m_clockSpeed * Time.deltaTime);
    }

    public void Destroy() {
        Destroy(gameObject);
    }
}
