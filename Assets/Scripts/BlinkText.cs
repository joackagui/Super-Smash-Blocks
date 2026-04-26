using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float blinkSpeed = 1f;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (text == null) return;

        float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}