using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WinnerScreenUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private RawImage winnerImage;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Winner Textures")]
    [SerializeField] private Texture batmanImage;
    [SerializeField] private Texture jokerImage;
    [SerializeField] private Texture redHoodImage;
    [SerializeField] private Texture defaultImage;

    [Header("Timing")]
    [SerializeField] private float delayBeforeFade = 1f;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Blink")]
    [SerializeField] private float blinkSpeed = 1.2f;

    [SerializeField] private InputActionReference returnAction;

    private bool isLoading;
    private bool _canInteract;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene(0);
            return;
        }
        ApplyWinnerImage();
    }

    private void Start()
    {
        SetAlpha(blackScreen, 1f);
        winnerImage.gameObject.SetActive(true);
        SetAlpha(winnerImage, 0f);

        if (promptText != null)
            SetAlphaTMP(promptText, 0f);

        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        _canInteract = false;

        yield return new WaitForSeconds(delayBeforeFade);

        if (promptText != null)
            yield return FadeMultiple(0f, 1f, winnerImage, promptText);
        else
            yield return FadeGraphic(winnerImage, 0f, 1f);

        if (promptText != null)
            StartCoroutine(BlinkText(promptText));

        _canInteract = true;
    }

    private IEnumerator FadeGraphic(Graphic img, float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(img, Mathf.Lerp(from, to, t / fadeDuration));
            yield return null;
        }
        SetAlpha(img, to);
    }

    private IEnumerator FadeMultiple(float from, float to, params Graphic[] images)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / fadeDuration);
            foreach (var img in images)
                SetAlpha(img, alpha);
            yield return null;
        }
        foreach (var img in images)
            SetAlpha(img, to);
    }

    private IEnumerator BlinkText(TextMeshProUGUI tmp)
    {
        while (true)
        {
            float alpha = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f) / 2f;
            SetAlphaTMP(tmp, alpha);
            yield return null;
        }
    }

    private void SetAlpha(Graphic img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void SetAlphaTMP(TextMeshProUGUI tmp, float alpha)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }

    private void ApplyWinnerImage()
    {
        if (winnerImage == null || GameManager.Instance == null) return;

        string winner = GameManager.Instance.GetWinnerSelection();
        Texture selected = defaultImage;

        if (winner == "Batman")       selected = batmanImage;
        else if (winner == "Joker")   selected = jokerImage;
        else if (winner == "RedHood") selected = redHoodImage;

        winnerImage.texture = selected;
    }

    private void OnEnable()  => BindAction(returnAction, OnReturnPerformed);
    private void OnDisable() => UnbindAction(returnAction, OnReturnPerformed);

    private static void BindAction(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
    {
        if (r == null || r.action == null) return;
        r.action.performed += cb;
        r.action.Enable();
    }

    private static void UnbindAction(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
    {
        if (r == null || r.action == null) return;
        r.action.performed -= cb;
    }

    private void OnReturnPerformed(InputAction.CallbackContext context)
    {
        if (isLoading || !_canInteract) return;
        isLoading = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearSelections();
            GameManager.Instance.ClearWinnerSelection();
        }

        SceneManager.LoadScene(0);
    }
}