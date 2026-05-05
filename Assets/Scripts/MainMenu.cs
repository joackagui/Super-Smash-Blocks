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

    [Header("UI")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private RawImage diffuseImage;
    [SerializeField] private RawImage logoImage;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Options")]
    [SerializeField] private TextMeshProUGUI[] optionTexts;
    [SerializeField] private float selectedScale = 1.15f;
    [SerializeField] private float unselectedScale = 1f;
    [SerializeField] private float blinkSpeed = 4f;

    [Header("Scenes")]
    [SerializeField] private string characterSelectionSceneName = "CharacterSelection";

    [Header("Timings")]
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
        MusicManager.Instance?.PlayMusic(MusicManager.Instance.mainMenuMusic);

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
            if (WasConfirmPressed())
                EnterOptionsMenu();

            return;
        }

        if (_state == MenuState.Options)
        {
            if (WasUpPressed())
                MoveSelection(-1);

            if (WasDownPressed())
                MoveSelection(1);

            if (WasConfirmPressed())
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

        _selectedIndex = 0; // SIEMPRE empieza en la primera opción
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
        if (_isTransitioning)
            return;

        _isTransitioning = true;
        MusicManager.Instance?.PlayMenuSelect();

        switch (_selectedIndex)
        {
            case 0: // Singleplayer
                GameManager.Instance?.SetGameMode(GameMode.SinglePlayer);
                break;

            case 1: // Multiplayer
                GameManager.Instance?.SetGameMode(GameMode.Multiplayer);
                break;

            default:
                _isTransitioning = false;
                MusicManager.Instance?.PlayMenuError();
                return;
        }

        SceneManager.LoadScene(characterSelectionSceneName);
    }

    private void ShowOptions()
    {
        foreach (var txt in optionTexts)
            if (txt != null)
                txt.gameObject.SetActive(true);
    }

    private void HideOptions()
    {
        foreach (var txt in optionTexts)
            if (txt != null)
                txt.gameObject.SetActive(false);
    }

    private void UpdateOptionVisuals()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] == null) continue;

            Vector3 baseScale = _optionBaseScales[i];

            if (i == _selectedIndex)
            {
                optionTexts[i].transform.localScale = baseScale * selectedScale;
            }
            else
            {
                optionTexts[i].transform.localScale = baseScale * unselectedScale;
                optionTexts[i].color = _optionBaseColors[i];
            }
        }

        _blinkCoroutine = StartCoroutine(BlinkSelectedOption(_selectedIndex));
    }

    private IEnumerator BlinkSelectedOption(int index)
    {
        TextMeshProUGUI txt = optionTexts[index];
        Color baseColor = _optionBaseColors[index];

        while (_state == MenuState.Options && _selectedIndex == index)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color c = baseColor;
            c.a = alpha;
            txt.color = c;
            yield return null;
        }

        txt.color = baseColor;
    }

    private void CacheOptionData()
    {
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
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void SetAlphaTMP(TextMeshProUGUI tmp, float alpha)
    {
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }

    private bool WasConfirmPressed()
    {
        return Keyboard.current != null &&
               (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                Keyboard.current.cKey.wasPressedThisFrame) ||
               Mouse.current != null &&
               Mouse.current.leftButton.wasPressedThisFrame;
    }

    private bool WasUpPressed()
    {
        return Keyboard.current != null &&
               (Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.upArrowKey.wasPressedThisFrame);
    }

    private bool WasDownPressed()
    {
        return Keyboard.current != null &&
               (Keyboard.current.sKey.wasPressedThisFrame ||
                Keyboard.current.downArrowKey.wasPressedThisFrame);
    }
}