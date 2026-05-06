using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CharacterSelection : MonoBehaviour
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

    [System.Serializable]
    private class CharacterSlot
    {
        public string characterName;
        public Texture icon;
        public RawImage characterImage;
        public RawImage player1Marker;
        public RawImage player2Marker;
    }

    [Header("Input")]
    [SerializeField] private PlayerInputBindings player1Input;
    [SerializeField] private PlayerInputBindings player2Input;
    [SerializeField] private InputActionReference backAction;

    [Header("Slots")]
    [SerializeField] private CharacterSlot[] characterSlots = new CharacterSlot[4];

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI player1SelectText;
    [SerializeField] private TextMeshProUGUI player1DeselectText;
    [SerializeField] private TextMeshProUGUI player2SelectText;
    [SerializeField] private TextMeshProUGUI player2DeselectText;

    [Header("Player 1 Preview")]
    [SerializeField] private RawImage player1Icon;
    [SerializeField] private TextMeshProUGUI player1Name;

    [Header("Player 2 Preview")]
    [SerializeField] private RawImage player2Icon;
    [SerializeField] private TextMeshProUGUI player2Name;

    private Vector2Int player1Position = new Vector2Int(0, 0);
    private Vector2Int player2Position = new Vector2Int(1, 1);
    private bool player1Selected;
    private bool player2Selected;
    private bool isTransitioning;

    private bool IsSinglePlayer => GameManager.Instance != null && GameManager.Instance.IsSinglePlayer;

    private void Awake()
    {
        ValidateGameManager();
        player1Selected = false;
        player2Selected = false;
    }

    private void OnEnable()
    {
        ApplyInputRouting();
        InputSystem.onDeviceChange += OnDeviceChange;

        BindAction(player1Input.up, OnPlayer1UpPerformed);
        BindAction(player1Input.down, OnPlayer1DownPerformed);
        BindAction(player1Input.left, OnPlayer1LeftPerformed);
        BindAction(player1Input.right, OnPlayer1RightPerformed);
        BindAction(player1Input.select, OnPlayer1SelectPerformed);
        BindAction(player1Input.deselect, OnPlayer1DeselectPerformed);

        BindAction(player2Input.up, OnPlayer2UpPerformed);
        BindAction(player2Input.down, OnPlayer2DownPerformed);
        BindAction(player2Input.left, OnPlayer2LeftPerformed);
        BindAction(player2Input.right, OnPlayer2RightPerformed);
        BindAction(player2Input.select, OnPlayer2SelectPerformed);
        BindAction(player2Input.deselect, OnPlayer2DeselectPerformed);

        BindAction(backAction, OnBackPerformed);

        RefreshMarkers();
        UpdateUIText();
        UpdatePlayerPreview();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        UnbindAction(player1Input.up, OnPlayer1UpPerformed);
        UnbindAction(player1Input.down, OnPlayer1DownPerformed);
        UnbindAction(player1Input.left, OnPlayer1LeftPerformed);
        UnbindAction(player1Input.right, OnPlayer1RightPerformed);
        UnbindAction(player1Input.select, OnPlayer1SelectPerformed);
        UnbindAction(player1Input.deselect, OnPlayer1DeselectPerformed);

        UnbindAction(player2Input.up, OnPlayer2UpPerformed);
        UnbindAction(player2Input.down, OnPlayer2DownPerformed);
        UnbindAction(player2Input.left, OnPlayer2LeftPerformed);
        UnbindAction(player2Input.right, OnPlayer2RightPerformed);
        UnbindAction(player2Input.select, OnPlayer2SelectPerformed);
        UnbindAction(player2Input.deselect, OnPlayer2DeselectPerformed);

        UnbindAction(backAction, OnBackPerformed);
    }

    private static void BindAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> onPerformed)
    {
        if (actionReference == null || actionReference.action == null) return;
        actionReference.action.performed += onPerformed;
        actionReference.action.Enable();
    }

    private static void UnbindAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> onPerformed)
    {
        if (actionReference == null || actionReference.action == null) return;
        actionReference.action.performed -= onPerformed;
    }

    private void ApplyInputRouting()
    {
        ApplyBindingsForPlayer(player1Input, Player.PlayerSlot.Player1);
        ApplyBindingsForPlayer(player2Input, Player.PlayerSlot.Player2, false);
    }

    private static void ApplyBindingsForPlayer(PlayerInputBindings bindings, Player.PlayerSlot slot, bool includeKeyboardAndMouseForPlayerOne = true)
    {
        if (bindings == null)
        {
            return;
        }

        PlayerInputDeviceRouter.AssignDevices(bindings.up, slot, includeKeyboardAndMouseForPlayerOne);
        PlayerInputDeviceRouter.AssignDevices(bindings.down, slot, includeKeyboardAndMouseForPlayerOne);
        PlayerInputDeviceRouter.AssignDevices(bindings.left, slot, includeKeyboardAndMouseForPlayerOne);
        PlayerInputDeviceRouter.AssignDevices(bindings.right, slot, includeKeyboardAndMouseForPlayerOne);
        PlayerInputDeviceRouter.AssignDevices(bindings.select, slot, includeKeyboardAndMouseForPlayerOne);
        PlayerInputDeviceRouter.AssignDevices(bindings.deselect, slot, includeKeyboardAndMouseForPlayerOne);
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

    private void OnPlayer1UpPerformed(InputAction.CallbackContext context)
    {
        MovePlayer(ref player1Position, player2Position, player1Selected, new Vector2Int(0, -1));
    }

    private void OnPlayer1DownPerformed(InputAction.CallbackContext context)
    {
        MovePlayer(ref player1Position, player2Position, player1Selected, new Vector2Int(0, 1));
    }

    private void OnPlayer1LeftPerformed(InputAction.CallbackContext context)
    {
        MovePlayer(ref player1Position, player2Position, player1Selected, Vector2Int.left);
    }

    private void OnPlayer1RightPerformed(InputAction.CallbackContext context)
    {
        MovePlayer(ref player1Position, player2Position, player1Selected, Vector2Int.right);
    }

    private void OnPlayer2UpPerformed(InputAction.CallbackContext context)
    {
        if (IsSinglePlayer) return;
        MovePlayer(ref player2Position, player1Position, player2Selected, new Vector2Int(0, -1));
    }

    private void OnPlayer2DownPerformed(InputAction.CallbackContext context)
    {
        if (IsSinglePlayer) return;
        MovePlayer(ref player2Position, player1Position, player2Selected, new Vector2Int(0, 1));
    }

    private void OnPlayer2LeftPerformed(InputAction.CallbackContext context)
    {
        if (IsSinglePlayer) return;
        MovePlayer(ref player2Position, player1Position, player2Selected, Vector2Int.left);
    }

    private void OnPlayer2RightPerformed(InputAction.CallbackContext context)
    {
        if (IsSinglePlayer) return;
        MovePlayer(ref player2Position, player1Position, player2Selected, Vector2Int.right);
    }

    private void OnPlayer1SelectPerformed(InputAction.CallbackContext context)
    {
        if (isTransitioning) return;
        if (player1Selected) return;
        if (IsBlockedSelection(player1Position)) return;

        player1Selected = true;
        MusicManager.Instance?.PlayMenuSelect();

        if (IsSinglePlayer)
        {
            AssignRandomPlayer2();
            player2Selected = true;
        }

        RegisterSelection();
        UpdateUIText();
        UpdatePlayerPreview();
        TryLoadNextScene();
    }

    private void OnPlayer2SelectPerformed(InputAction.CallbackContext context)
    {
        if (IsSinglePlayer) return;
        if (isTransitioning) return;
        if (player2Selected) return;
        if (IsBlockedSelection(player2Position)) return;

        player2Selected = true;
        MusicManager.Instance?.PlayMenuSelect();

        RegisterSelection();
        UpdateUIText();
        UpdatePlayerPreview();
        TryLoadNextScene();
    }

    private void OnPlayer1DeselectPerformed(InputAction.CallbackContext context)
    {
        if (!player1Selected) return;

        player1Selected = false;
        MusicManager.Instance?.PlayMenuBack();

        RegisterSelection();
        UpdateUIText();
    }

    private void OnPlayer2DeselectPerformed(InputAction.CallbackContext context)
    {
        if (IsSinglePlayer) return;
        if (!player2Selected) return;

        player2Selected = false;
        MusicManager.Instance?.PlayMenuBack();

        RegisterSelection();
        UpdateUIText();
    }

    private void OnBackPerformed(InputAction.CallbackContext context)
    {
        if (isTransitioning) return;

        MusicManager.Instance?.PlayMenuBack();

        if (GameManager.Instance != null)
            GameManager.Instance.ClearSelections();

        isTransitioning = true;

        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;

        if (previousSceneIndex >= 0)
            SceneManager.LoadScene(previousSceneIndex);
        else
            isTransitioning = false;
    }

    private void MovePlayer(ref Vector2Int currentPosition, Vector2Int otherPlayerPosition, bool isSelected, Vector2Int delta)
    {
        if (isSelected) return;

        Vector2Int nextPosition = currentPosition + delta;

        if (nextPosition.x < 0 || nextPosition.x > 1 || nextPosition.y < 0 || nextPosition.y > 1)
            return;

        if (!IsSinglePlayer && nextPosition == otherPlayerPosition)
            return;

        currentPosition = nextPosition;

        MusicManager.Instance?.PlayMenuMove();

        RefreshMarkers();
        UpdatePlayerPreview();
    }

    private void AssignRandomPlayer2()
    {
        List<int> availableSlots = new List<int>();

        int player1Index = player1Position.y * 2 + player1Position.x;

        for (int i = 0; i < characterSlots.Length; i++)
        {
            CharacterSlot slot = characterSlots[i];
            if (slot == null) continue;

            if (string.Equals(slot.characterName, "Blocked", System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (i == player1Index)
                continue;

            availableSlots.Add(i);
        }

        if (availableSlots.Count == 0) return;

        int randomIndex = availableSlots[Random.Range(0, availableSlots.Count)];
        player2Position = new Vector2Int(randomIndex % 2, randomIndex / 2);
    }

    private void RegisterSelection()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.SetPlayer1Selection(player1Selected ? GetCharacterName(player1Position) : "None");

        if (IsSinglePlayer)
            GameManager.Instance.SetPlayer2Selection(GetCharacterName(player2Position));
        else
            GameManager.Instance.SetPlayer2Selection(player2Selected ? GetCharacterName(player2Position) : "None");
    }

    private string GetCharacterName(Vector2Int position)
    {
        int index = position.y * 2 + position.x;

        if (index < 0 || index >= characterSlots.Length)
            return "None";

        CharacterSlot slot = characterSlots[index];

        if (slot == null)
            return "None";

        return slot.characterName;
    }

    private bool IsBlockedSelection(Vector2Int position)
    {
        if (GetCharacterName(position) != "Blocked")
            return false;

        MusicManager.Instance?.PlayMenuError();
        return true;
    }

    private void TryLoadNextScene()
    {
        if (isTransitioning) return;

        if (IsSinglePlayer)
        {
            if (!player1Selected) return;
        }
        else
        {
            if (!player1Selected || !player2Selected) return;
        }

        isTransitioning = true;

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            isTransitioning = false;
    }

    private void RefreshMarkers()
    {
        for (int i = 0; i < characterSlots.Length; i++)
        {
            CharacterSlot slot = characterSlots[i];
            if (slot == null) continue;

            Vector2Int slotPosition = new Vector2Int(i % 2, i / 2);

            if (slot.player1Marker != null)
                slot.player1Marker.enabled = slotPosition == player1Position;

            if (slot.player2Marker != null)
                slot.player2Marker.enabled = !IsSinglePlayer && slotPosition == player2Position;
        }
    }

    private void UpdateUIText()
    {
        if (player1SelectText != null)
            player1SelectText.gameObject.SetActive(!player1Selected);

        if (player1DeselectText != null)
            player1DeselectText.gameObject.SetActive(player1Selected);

        if (IsSinglePlayer)
        {
            if (player2SelectText != null) player2SelectText.gameObject.SetActive(false);
            if (player2DeselectText != null) player2DeselectText.gameObject.SetActive(false);
        }
        else
        {
            if (player2SelectText != null) player2SelectText.gameObject.SetActive(!player2Selected);
            if (player2DeselectText != null) player2DeselectText.gameObject.SetActive(player2Selected);
        }
    }

    private void UpdatePlayerPreview()
    {
        UpdateSinglePreview(player1Position, player1Icon, player1Name);
        UpdateSinglePreview(player2Position, player2Icon, player2Name);
    }

    private void UpdateSinglePreview(Vector2Int pos, RawImage icon, TextMeshProUGUI text)
    {
        if (icon == null || text == null) return;

        int index = pos.y * 2 + pos.x;

        if (index < 0 || index >= characterSlots.Length)
        {
            text.text = "None";
            icon.texture = null;
            return;
        }

        CharacterSlot slot = characterSlots[index];

        if (slot == null)
        {
            text.text = "None";
            icon.texture = null;
            return;
        }

        text.text = slot.characterName;
        icon.texture = slot.icon;
    }

    private void ValidateGameManager()
    {
        if (GameManager.Instance == null)
            SceneManager.LoadScene(0);
    }
}
