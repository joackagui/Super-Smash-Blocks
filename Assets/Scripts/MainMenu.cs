using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [System.Serializable]
    private class PlayerInputBindings
    {
        public InputActionReference up;
        public InputActionReference down;
        public InputActionReference left;
        public InputActionReference right;
        public InputActionReference select;
        public InputActionReference deselect;
    }

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
    [SerializeField] private float optionInputCooldown = 0.2f;

    [Header("Input")]
    [SerializeField] private InputActionReference introNextAction;
    [SerializeField] private PlayerInputBindings player1Input;

    private MenuState _state = MenuState.Intro;
    private bool _isTransitioning;
    private bool _canInteract;
    private int _selectedIndex;

    private Vector3[] _optionBaseScales;
    private Color[] _optionBaseColors;
    private Coroutine _blinkCoroutine;
    private float _optionInputUnlockTime;

    private void Awake()
    {
        CacheOptionData();
    }

    private void OnEnable()
    {
        ApplyInputRouting();
        InputSystem.onDeviceChange += OnDeviceChange;

        BindAction(introNextAction, OnIntroNextPerformed);
        BindAction(player1Input.up, OnNavigateUpPerformed);
        BindAction(player1Input.down, OnNavigateDownPerformed);
        BindAction(player1Input.left, OnNavigateUpPerformed);
        BindAction(player1Input.right, OnNavigateDownPerformed);
        BindAction(player1Input.select, OnSelectPerformed);
        BindAction(player1Input.deselect, OnBackPerformed);
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        UnbindAction(introNextAction, OnIntroNextPerformed);
        UnbindAction(player1Input.up, OnNavigateUpPerformed);
        UnbindAction(player1Input.down, OnNavigateDownPerformed);
        UnbindAction(player1Input.left, OnNavigateUpPerformed);
        UnbindAction(player1Input.right, OnNavigateDownPerformed);
        UnbindAction(player1Input.select, OnSelectPerformed);
        UnbindAction(player1Input.deselect, OnBackPerformed);
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
        _optionInputUnlockTime = Time.unscaledTime + optionInputCooldown;
        UpdateOptionVisuals();
    }

    private void ReturnToIntroMenu()
    {
        if (_isTransitioning)
            return;

        _state = MenuState.Intro;

        HideOptions();

        if (logoImage != null)
            logoImage.gameObject.SetActive(true);

        if (promptText != null)
            promptText.gameObject.SetActive(true);

        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        RestoreOptionVisuals();
        MusicManager.Instance?.PlayMenuBack();
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
            case 0:
                GameManager.Instance?.SetGameMode(GameMode.SinglePlayer);
                break;

            case 1:
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

    private void RestoreOptionVisuals()
    {
        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] == null)
                continue;

            optionTexts[i].transform.localScale = _optionBaseScales[i];
            optionTexts[i].color = _optionBaseColors[i];
        }
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

    private void ApplyInputRouting()
    {
        PlayerInputDeviceRouter.AssignDevices(introNextAction, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.up, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.down, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.left, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.right, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.select, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.deselect, Player.PlayerSlot.Player1);
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad && device is not Keyboard && device is not Mouse)
        {
            return;
        }

        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Reconnected:
            case InputDeviceChange.Enabled:
            case InputDeviceChange.Disabled:
            case InputDeviceChange.ConfigurationChanged:
                ApplyInputRouting();
                break;
        }
    }

    private static void BindAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> onPerformed)
    {
        if (actionReference == null || actionReference.action == null)
            return;

        actionReference.action.performed += onPerformed;
        actionReference.action.Enable();
    }

    private static void UnbindAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> onPerformed)
    {
        if (actionReference == null || actionReference.action == null)
            return;

        actionReference.action.performed -= onPerformed;
    }

    private void OnIntroNextPerformed(InputAction.CallbackContext context)
    {
        if (!_canInteract || _isTransitioning || _state != MenuState.Intro)
            return;

        EnterOptionsMenu();
    }

    private void OnNavigateUpPerformed(InputAction.CallbackContext context)
    {
        if (!_canInteract || _isTransitioning || _state != MenuState.Options || !CanUseOptionInputs())
            return;

        MoveSelection(-1);
    }

    private void OnNavigateDownPerformed(InputAction.CallbackContext context)
    {
        if (!_canInteract || _isTransitioning || _state != MenuState.Options || !CanUseOptionInputs())
            return;

        MoveSelection(1);
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        if (!_canInteract || _isTransitioning || _state != MenuState.Options || !CanUseOptionInputs())
            return;

        ConfirmSelection();
    }

    private void OnBackPerformed(InputAction.CallbackContext context)
    {
        if (!_canInteract || _isTransitioning || _state != MenuState.Options || !CanUseOptionInputs())
            return;

        ReturnToIntroMenu();
    }

    private bool CanUseOptionInputs()
    {
        return Time.unscaledTime >= _optionInputUnlockTime;
    }
}
