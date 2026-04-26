using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference playAction;
    [SerializeField] private InputActionReference quitAction;

    [Header("UI")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private RawImage diffuseImage;
    [SerializeField] private RawImage logoImage;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Timing")]
    [SerializeField] private float delayBeforeFade = 2f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float logoExtraDelay = 1f;
    [SerializeField] private float textDelayAfterLogo = 0.8f;

    private bool _isTransitioning;
    private bool _canInteract;

    private void Start()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayMusic(MusicManager.Instance.mainMenuMusic);

        SetAlpha(blackScreen, 1f);

        diffuseImage.gameObject.SetActive(true);
        logoImage.gameObject.SetActive(true);

        SetAlpha(diffuseImage, 0f);
        SetAlpha(logoImage, 0f);

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        StartCoroutine(IntroSequence());
    }

    private IEnumerator FadeImage(Graphic img, float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / fadeDuration);
            SetAlpha(img, alpha);
            yield return null;
        }
        SetAlpha(img, to);
    }

    private IEnumerator IntroSequence()
    {
        _canInteract = false;

        yield return new WaitForSeconds(delayBeforeFade);

        yield return FadeImage(diffuseImage, 0f, 1f);

        yield return new WaitForSeconds(logoExtraDelay);

        yield return FadeImage(logoImage, 0f, 1f);

        yield return new WaitForSeconds(textDelayAfterLogo);

        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            SetAlphaTMP(promptText, 1f);
        }

        _canInteract = true;
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

    private void OnEnable()
    {
        Bind(playAction, OnPlayPerformed);
        Bind(quitAction, OnQuitPerformed);
    }

    private void OnDisable()
    {
        Unbind(playAction, OnPlayPerformed);
        Unbind(quitAction, OnQuitPerformed);
    }

    private static void Bind(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
    {
        if (r == null || r.action == null) return;
        r.action.performed += cb;
        r.action.Enable();
    }

    private static void Unbind(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
    {
        if (r == null || r.action == null) return;
        r.action.performed -= cb;
    }

    private void OnPlayPerformed(InputAction.CallbackContext _) => Play();
    private void OnQuitPerformed(InputAction.CallbackContext _) => Quit();

    public void Play()
    {
        if (_isTransitioning || !_canInteract) return;
        _isTransitioning = true;
        MusicManager.Instance?.PlayMenuSelect();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}