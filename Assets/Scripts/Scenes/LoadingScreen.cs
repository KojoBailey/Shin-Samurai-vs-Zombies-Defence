using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour { // Loading Screen
    [SerializeField] private GameObject clock;
    private const float CLOCK_SPEED = 900;

    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private List<string> tips;

    private void Start() {
        DontDestroyOnLoad(gameObject);
        tipText.text = tips[Random.Range(0, tips.Count)];
    }

    private void Update() {
        Transform shortHand = clock.transform.Find("Short Hand");
        Transform longHand = clock.transform.Find("Long Hand");
        shortHand.GetComponent<RectTransform>().transform.Rotate(0, 0, -1 * CLOCK_SPEED / 12 * Time.deltaTime);
        longHand.GetComponent<RectTransform>().transform.Rotate(0, 0, -1 * CLOCK_SPEED * Time.deltaTime);
    }

    public void Destroy() {
        Destroy(gameObject);
    }
}
