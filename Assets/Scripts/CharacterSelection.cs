using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        public RawImage characterImage;
        public RawImage player1Marker;
        public RawImage player2Marker;
    }

    [SerializeField] private PlayerInputBindings player1Input;
    [SerializeField] private PlayerInputBindings player2Input;
    [SerializeField] private CharacterSlot[] characterSlots = new CharacterSlot[4];
    [SerializeField] private string nextSceneName = "StageSelectionScene";

    private Vector2Int player1Position = new Vector2Int(0, 0);
    private Vector2Int player2Position = new Vector2Int(1, 1);
    private bool player1Selected;
    private bool player2Selected;
    private bool isTransitioning;

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearSelections();
        }
    }

    private void OnEnable()
    {
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

        RefreshMarkers();
    }

    private void OnDisable()
    {
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
    }

    private static void BindAction(
        InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> onPerformed)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.performed += onPerformed;

        actionReference.action.Enable();
    }

    private static void UnbindAction(
        InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> onPerformed)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.performed -= onPerformed;
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
        MovePlayer(ref player2Position, player1Position, player2Selected, new Vector2Int(0, -1));
    }

    private void OnPlayer2DownPerformed(InputAction.CallbackContext context)
    {
        MovePlayer(ref player2Position, player1Position, player2Selected, new Vector2Int(0, 1));
    }

    private void OnPlayer2LeftPerformed(InputAction.CallbackContext context)
    {
        MovePlayer(ref player2Position, player1Position, player2Selected, Vector2Int.left);
    }

    private void OnPlayer2RightPerformed(InputAction.CallbackContext context)
    {
        MovePlayer(ref player2Position, player1Position, player2Selected, Vector2Int.right);
    }

    private void OnPlayer1SelectPerformed(InputAction.CallbackContext context)
    {
        if (player1Selected)
        {
            return;
        }

        player1Selected = true;
        RegisterSelection();
        Debug.Log($"Player1 selection: {GetCharacterName(player1Position)}");
        TryLoadNextScene();
    }

    private void OnPlayer2SelectPerformed(InputAction.CallbackContext context)
    {
        if (player2Selected)
        {
            return;
        }

        player2Selected = true;
        RegisterSelection();
        Debug.Log($"Player2 selection: {GetCharacterName(player2Position)}");
        TryLoadNextScene();
    }

    private void OnPlayer1DeselectPerformed(InputAction.CallbackContext context)
    {
        if (!player1Selected)
        {
            return;
        }

        player1Selected = false;
        RegisterSelection();
        Debug.Log("Player1 selection: none");
    }

    private void OnPlayer2DeselectPerformed(InputAction.CallbackContext context)
    {
        if (!player2Selected)
        {
            return;
        }

        player2Selected = false;
        RegisterSelection();
        Debug.Log("Player2 selection: none");
    }

    private void MovePlayer(ref Vector2Int currentPosition, Vector2Int otherPlayerPosition, bool isSelected, Vector2Int delta)
    {
        if (isSelected)
        {
            return;
        }

        Vector2Int nextPosition = currentPosition + delta;

        if (nextPosition.x < 0 || nextPosition.x > 1 || nextPosition.y < 0 || nextPosition.y > 1)
        {
            return;
        }

        if (nextPosition == otherPlayerPosition)
        {
            return;
        }

        currentPosition = nextPosition;
        RefreshMarkers();
    }

    private void RegisterSelection()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.SetPlayer1Selection(player1Selected ? GetCharacterName(player1Position) : "none");
        GameManager.Instance.SetPlayer2Selection(player2Selected ? GetCharacterName(player2Position) : "none");
    }

    private string GetCharacterName(Vector2Int position)
    {
        int slotIndex = position.y * 2 + position.x;

        if (slotIndex < 0 || slotIndex >= characterSlots.Length)
        {
            return "none";
        }

        CharacterSlot slot = characterSlots[slotIndex];
        if (slot == null || slot.characterImage == null)
        {
            return "none";
        }

        return slot.characterImage.name;
    }

    private void TryLoadNextScene()
    {
        if (isTransitioning || !player1Selected || !player2Selected)
        {
            return;
        }

        isTransitioning = true;
        SceneManager.LoadScene(nextSceneName);
    }

    private void RefreshMarkers()
    {
        for (int i = 0; i < characterSlots.Length; i++)
        {
            CharacterSlot slot = characterSlots[i];

            if (slot == null)
            {
                continue;
            }

            Vector2Int slotPosition = GetSlotPosition(i);
            bool showPlayer1 = slotPosition == player1Position;
            bool showPlayer2 = slotPosition == player2Position;

            if (slot.player1Marker != null)
            {
                slot.player1Marker.enabled = showPlayer1;
            }

            if (slot.player2Marker != null)
            {
                slot.player2Marker.enabled = showPlayer2;
            }
        }
    }

    private static Vector2Int GetSlotPosition(int slotIndex)
    {
        return new Vector2Int(slotIndex % 2, slotIndex / 2);
    }
}
