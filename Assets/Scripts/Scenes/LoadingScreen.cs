using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;

public class LoadingScreen : MonoBehaviour {
    [SerializeField] private GameObject clock;
    private const float clockSpeed = 900;

    [SerializeField] private TextMeshProUGUI tipText;

    private void Start() {
        DontDestroyOnLoad(gameObject);
        tipText.text = "Abilities can come in handy at the right moment, but beware of their cooldowns!";
    }

    private void Update() {
        Transform shortHand = clock.transform.Find("Short Hand");
        Transform longHand = clock.transform.Find("Long Hand");
        shortHand.GetComponent<RectTransform>().transform.Rotate(0, 0, -1 * clockSpeed / 12 * Time.deltaTime);
        longHand.GetComponent<RectTransform>().transform.Rotate(0, 0, -1 * clockSpeed * Time.deltaTime);
    }

    public void Destroy() {
        Destroy(gameObject);
    }
}
