using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ControlsSceneManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private RawImage controlsImage;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Timing")]
    [SerializeField] private float delayBeforeFade = 1f;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Blink")]
    [SerializeField] private float textBlinkSpeed = 1.2f;

    [Header("Input")]
    [SerializeField] private InputActionReference continueAction;
    [SerializeField] private InputActionReference returnAction;
    [SerializeField] private string fightSceneName = "FightScene";

    private bool isLoading;
    private bool _canInteract;

    private void Awake()
    {
        ValidateGameManager();
    }

    private void Start()
    {
        SetAlpha(blackScreen, 1f);

        controlsImage.gameObject.SetActive(true);
        SetAlpha(controlsImage, 0f);

        if (promptText != null)
            SetAlphaTMP(promptText, 0f);

        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        _canInteract = false;

        yield return new WaitForSeconds(delayBeforeFade);

        _canInteract = true;

        if (promptText != null)
            StartCoroutine(BlinkText(promptText, textBlinkSpeed));

        if (promptText != null)
            yield return FadeMultiple(0f, 1f, controlsImage, promptText);
        else
            yield return FadeGraphic(controlsImage, 0f, 1f);
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

    private IEnumerator BlinkText(TextMeshProUGUI tmp, float speed)
    {
        while (true)
        {
            float alpha = (Mathf.Sin(Time.time * speed * Mathf.PI * 2f) + 1f) / 2f;
            SetAlphaTMP(tmp, alpha);
            yield return null;
        }
    }

    // ---- Helpers ----

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

    // ---- Input ----

    private void OnEnable()
    {
        BindAction(continueAction, OnContinuePerformed);
        BindAction(returnAction, OnReturnPerformed);
    }

    private void OnDisable()
    {
        UnbindAction(continueAction, OnContinuePerformed);
        UnbindAction(returnAction, OnReturnPerformed);
    }

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

    private void OnContinuePerformed(InputAction.CallbackContext context)
    {
        if (isLoading || !_canInteract) return;
        isLoading = true;
        MusicManager.Instance?.PlayMenuSelect();
        SceneManager.LoadScene(fightSceneName);
    }

    private void OnReturnPerformed(InputAction.CallbackContext context)
    {
        if (isLoading || !_canInteract) return;
        isLoading = true;
        MusicManager.Instance?.PlayMenuBack();
        GameManager.Instance?.ClearStageSelection();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    private void ValidateGameManager()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene(0);
        }
    }
}