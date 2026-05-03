using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private enum MenuState
    {
        Intro,
        Options
    }

    [SerializeField] private Image blackScreen;
    [SerializeField] private RawImage diffuseImage;
    [SerializeField] private RawImage logoImage;
    [SerializeField] private TextMeshProUGUI promptText;

    [SerializeField] private TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[3];
    [SerializeField] private float selectedScale = 1.15f;
    [SerializeField] private float unselectedScale = 1f;
    [SerializeField] private Color blinkColor = Color.black;
    [SerializeField] private float blinkInterval = 0.12f;

    [SerializeField] private string characterSelectionSceneName = "CharacterSelection";

    [SerializeField] private float delayBeforeFade = 2f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float logoExtraDelay = 1f;
    [SerializeField] private float textDelayAfterLogo = 0.8f;

    private MenuState _state = MenuState.Intro;
    private bool _isTransitioning;
    private bool _canInteract;
    private int _selectedIndex;

    private Vector3[] _optionBaseScales;
    private Color[] _optionBaseColors;
    private Coroutine _blinkCoroutine;

    private void Awake()
    {
        CacheOptionData();
    }

    private void Start()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayMusic(MusicManager.Instance.mainMenuMusic);

        SetAlpha(blackScreen, 1f);

        if (diffuseImage != null)
        {
            diffuseImage.gameObject.SetActive(true);
            SetAlpha(diffuseImage, 0f);
        }

        if (logoImage != null)
        {
            logoImage.gameObject.SetActive(true);
            SetAlpha(logoImage, 0f);
        }

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        HideOptions();
        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        if (!_canInteract || _isTransitioning)
            return;

        if (_state == MenuState.Intro)
        {
            if (WasEnterPressed())
                EnterOptionsMenu();

            return;
        }

        if (_state == MenuState.Options)
        {
            if (WasWPressed())
                MoveSelection(-1);

            if (WasSPressed())
                MoveSelection(1);

            if (WasEnterPressed())
                ConfirmSelection();
        }
    }

    private IEnumerator IntroSequence()
    {
        _canInteract = false;
        _state = MenuState.Intro;

        yield return new WaitForSeconds(delayBeforeFade);

        if (diffuseImage != null)
            yield return FadeImage(diffuseImage, 0f, 1f);

        yield return new WaitForSeconds(logoExtraDelay);

        if (logoImage != null)
            yield return FadeImage(logoImage, 0f, 1f);

        yield return new WaitForSeconds(textDelayAfterLogo);

        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            SetAlphaTMP(promptText, 1f);
        }

        _canInteract = true;
    }

    private void EnterOptionsMenu()
    {
        if (_isTransitioning)
            return;

        MusicManager.Instance?.PlayMenuSelect();

        _state = MenuState.Options;

        if (logoImage != null)
            logoImage.gameObject.SetActive(false);

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        ShowOptions();
        _selectedIndex = 0;
        UpdateOptionVisuals();
    }

    private void MoveSelection(int direction)
    {
        if (optionTexts == null || optionTexts.Length == 0)
            return;

        _selectedIndex += direction;

        if (_selectedIndex < 0)
            _selectedIndex = optionTexts.Length - 1;
        else if (_selectedIndex >= optionTexts.Length)
            _selectedIndex = 0;

        MusicManager.Instance?.PlayMenuMove();
        UpdateOptionVisuals();
    }

    private void ConfirmSelection()
    {
        if (_selectedIndex == 1)
        {
            _isTransitioning = true;
            MusicManager.Instance?.PlayMenuSelect();
            SceneManager.LoadScene(characterSelectionSceneName);
        }
        else
        {
            MusicManager.Instance?.PlayMenuError();
        }
    }

    private void ShowOptions()
    {
        if (optionTexts == null)
            return;

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] != null)
                optionTexts[i].gameObject.SetActive(true);
        }
    }

    private void HideOptions()
    {
        if (optionTexts == null)
            return;

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] != null)
                optionTexts[i].gameObject.SetActive(false);
        }
    }

    private void UpdateOptionVisuals()
    {
        if (optionTexts == null || optionTexts.Length == 0)
            return;

        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] == null)
                continue;

            Vector3 baseScale = (_optionBaseScales != null && i < _optionBaseScales.Length)
                ? _optionBaseScales[i]
                : Vector3.one;

            if (i == _selectedIndex)
            {
                optionTexts[i].transform.localScale = baseScale * selectedScale;
            }
            else
            {
                optionTexts[i].transform.localScale = baseScale * unselectedScale;

                Color baseColor = (_optionBaseColors != null && i < _optionBaseColors.Length)
                    ? _optionBaseColors[i]
                    : optionTexts[i].color;

                optionTexts[i].color = baseColor;
            }
        }

        if (optionTexts[_selectedIndex] != null)
            _blinkCoroutine = StartCoroutine(BlinkSelectedOption(_selectedIndex));
    }

    private IEnumerator BlinkSelectedOption(int index)
    {
        if (optionTexts == null || index < 0 || index >= optionTexts.Length || optionTexts[index] == null)
            yield break;

        TextMeshProUGUI txt = optionTexts[index];
        Color normalColor = (_optionBaseColors != null && index < _optionBaseColors.Length)
            ? _optionBaseColors[index]
            : txt.color;

        while (_state == MenuState.Options && _selectedIndex == index)
        {
            txt.color = normalColor;
            yield return new WaitForSeconds(blinkInterval);

            txt.color = blinkColor;
            yield return new WaitForSeconds(blinkInterval);
        }

        if (txt != null)
            txt.color = normalColor;
    }

    private void CacheOptionData()
    {
        if (optionTexts == null)
            return;

        _optionBaseScales = new Vector3[optionTexts.Length];
        _optionBaseColors = new Color[optionTexts.Length];

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] != null)
            {
                _optionBaseScales[i] = optionTexts[i].transform.localScale;
                _optionBaseColors[i] = optionTexts[i].color;
            }
            else
            {
                _optionBaseScales[i] = Vector3.one;
                _optionBaseColors[i] = Color.white;
            }
        }
    }

    private IEnumerator FadeImage(Graphic img, float from, float to)
    {
        if (img == null)
            yield break;

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

    private bool WasEnterPressed()
    {
        return Keyboard.current != null &&
               (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
    }

    private bool WasWPressed()
    {
        return Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame;
    }

    private bool WasSPressed()
    {
        return Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
    }
}