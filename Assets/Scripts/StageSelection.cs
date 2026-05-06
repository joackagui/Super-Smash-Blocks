using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StageSelection : MonoBehaviour
{
    [System.Serializable]
    private class PlayerInputBindings
    {
        public InputActionReference up;
        public InputActionReference down;
        public InputActionReference left;
        public InputActionReference right;
        public InputActionReference select;
    }

    [System.Serializable]
    private class StageSlot
    {
        public string stageName;
        public Texture icon;
        public RawImage stageImage;
        public RawImage marker;
    }

    [SerializeField] private PlayerInputBindings player1Input;
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private StageSlot[] stageSlots = new StageSlot[4];

    [SerializeField] private RawImage player1Icon;
    [SerializeField] private RawImage player2Icon;
    [SerializeField] private TextMeshProUGUI player1Name;
    [SerializeField] private TextMeshProUGUI player2Name;

    [SerializeField] private RawImage stagePreviewIcon;
    [SerializeField] private TextMeshProUGUI stagePreviewName;

    [SerializeField] private Texture batmanIcon;
    [SerializeField] private Texture jokerIcon;
    [SerializeField] private Texture redHoodIcon;

    private Vector2Int stagePosition = new Vector2Int(0, 0);
    private bool isTransitioning;

    private void Awake()
    {
        ValidateGameManager();
    }

    private void OnEnable()
    {
        ApplyInputRouting();
        InputSystem.onDeviceChange += OnDeviceChange;

        BindAction(player1Input.up, OnUpPerformed);
        BindAction(player1Input.down, OnDownPerformed);
        BindAction(player1Input.left, OnLeftPerformed);
        BindAction(player1Input.right, OnRightPerformed);
        BindAction(player1Input.select, OnSelectPerformed);
        BindAction(backAction, OnDeselectPerformed);

        EnsureValidStartPosition();
        RefreshMarkers();
        ApplyPlayerSelections();
        UpdateStagePreview();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        UnbindAction(player1Input.up, OnUpPerformed);
        UnbindAction(player1Input.down, OnDownPerformed);
        UnbindAction(player1Input.left, OnLeftPerformed);
        UnbindAction(player1Input.right, OnRightPerformed);
        UnbindAction(player1Input.select, OnSelectPerformed);
        UnbindAction(backAction, OnDeselectPerformed);
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
        PlayerInputDeviceRouter.AssignDevices(player1Input.up, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.down, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.left, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.right, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(player1Input.select, Player.PlayerSlot.Player1);
        PlayerInputDeviceRouter.AssignDevices(backAction, Player.PlayerSlot.Player1);
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

    private void OnUpPerformed(InputAction.CallbackContext context)
    {
        MoveStage(new Vector2Int(0, -1));
    }

    private void OnDownPerformed(InputAction.CallbackContext context)
    {
        MoveStage(new Vector2Int(0, 1));
    }

    private void OnLeftPerformed(InputAction.CallbackContext context)
    {
        MoveStage(Vector2Int.left);
    }

    private void OnRightPerformed(InputAction.CallbackContext context)
    {
        MoveStage(Vector2Int.right);
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        if (isTransitioning) return;

        string stageName = GetStageName(stagePosition);
        if (stageName == "None") return;

        if (GameManager.Instance != null)
            GameManager.Instance.SetStageSelection(stageName);

        MusicManager.Instance?.PlayMenuSelect();

        isTransitioning = true;

        if (GameManager.Instance.GetFirstTimeFightSceneLoaded())
        {
            GameManager.Instance.SetFirstTimeFightSceneLoaded(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
        }
    }

    private void OnDeselectPerformed(InputAction.CallbackContext context)
    {
        if (isTransitioning) return;

        if (GameManager.Instance != null)
            GameManager.Instance.SetStageSelection("None");

        MusicManager.Instance?.PlayMenuBack();

        isTransitioning = true;
        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;

        if (previousSceneIndex >= 0)
            SceneManager.LoadScene(previousSceneIndex);
        else
            isTransitioning = false;
    }

    private void MoveStage(Vector2Int delta)
    {
        if (isTransitioning) return;
        if (stageSlots == null || stageSlots.Length == 0) return;

        Vector2Int nextPosition = stagePosition + delta;

        if (nextPosition.x < 0 || nextPosition.x > 1 || nextPosition.y < 0 || nextPosition.y > 1)
            return;

        if (!IsSlotConfigured(nextPosition)) return;

        stagePosition = nextPosition;
        MusicManager.Instance?.PlayMenuMove();
        RefreshMarkers();
        UpdateStagePreview();
    }

    private void UpdateStagePreview()
    {
        int index = stagePosition.y * 2 + stagePosition.x;

        if (index < 0 || index >= stageSlots.Length) return;

        StageSlot slot = stageSlots[index];

        if (slot == null)
        {
            if (stagePreviewName != null) stagePreviewName.text = "None";
            if (stagePreviewIcon != null) stagePreviewIcon.texture = null;
            return;
        }

        if (stagePreviewName != null)
            stagePreviewName.text = slot.stageName;

        if (stagePreviewIcon != null)
            stagePreviewIcon.texture = slot.icon;
    }

    private void EnsureValidStartPosition()
    {
        if (IsSlotConfigured(stagePosition))
            return;

        for (int i = 0; i < stageSlots.Length; i++)
        {
            Vector2Int candidate = GetSlotPosition(i);
            if (IsSlotConfigured(candidate))
            {
                stagePosition = candidate;
                return;
            }
        }
    }

    private bool IsSlotConfigured(Vector2Int position)
    {
        int slotIndex = GetSlotIndex(position);

        if (slotIndex < 0 || slotIndex >= stageSlots.Length)
            return false;

        StageSlot slot = stageSlots[slotIndex];
        return slot != null && slot.stageImage != null;
    }

    private string GetStageName(Vector2Int position)
    {
        int index = position.y * 2 + position.x;

        if (index < 0 || index >= stageSlots.Length) return "None";

        StageSlot slot = stageSlots[index];
        return slot != null ? slot.stageName : "None";
    }

    private void RefreshMarkers()
    {
        if (stageSlots == null) return;

        for (int i = 0; i < stageSlots.Length; i++)
        {
            StageSlot slot = stageSlots[i];

            if (slot == null || slot.marker == null)
                continue;

            Vector2Int slotPosition = GetSlotPosition(i);
            slot.marker.enabled = slotPosition == stagePosition;
        }
    }

    private static Vector2Int GetSlotPosition(int slotIndex)
    {
        return new Vector2Int(slotIndex % 2, slotIndex / 2);
    }

    private static int GetSlotIndex(Vector2Int position)
    {
        return position.y * 2 + position.x;
    }

    private void ApplyPlayerSelections()
    {
        if (GameManager.Instance == null) return;

        ApplySingle(GameManager.Instance.GetPlayer1Selection(), player1Icon, player1Name);
        ApplySingle(GameManager.Instance.GetPlayer2Selection(), player2Icon, player2Name);
    }

    private void ApplySingle(string selection, RawImage icon, TextMeshProUGUI text)
    {
        if (text != null)
            text.text = selection;

        if (icon == null)
            return;

        switch (selection)
        {
            case "Batman":
                icon.texture = batmanIcon;
                break;
            case "Joker":
                icon.texture = jokerIcon;
                break;
            case "RedHood":
                icon.texture = redHoodIcon;
                break;
            default:
                icon.texture = null;
                break;
        }
    }

    private void ValidateGameManager()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene(0);
        }
    }
}
