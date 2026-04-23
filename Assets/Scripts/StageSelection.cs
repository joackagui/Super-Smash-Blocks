using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        public RawImage stageImage;
        public RawImage marker;
    }

    [SerializeField] private PlayerInputBindings player1Input;
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private StageSlot[] stageSlots = new StageSlot[4];

    private Vector2Int stagePosition = new Vector2Int(0, 0);
    private bool isTransitioning;

    private void OnEnable()
    {
        BindAction(player1Input.up, OnUpPerformed);
        BindAction(player1Input.down, OnDownPerformed);
        BindAction(player1Input.left, OnLeftPerformed);
        BindAction(player1Input.right, OnRightPerformed);
        BindAction(player1Input.select, OnSelectPerformed);
        BindAction(backAction, OnBackPerformed);

        EnsureValidStartPosition();
        RefreshMarkers();
    }

    private void OnDisable()
    {
        UnbindAction(player1Input.up, OnUpPerformed);
        UnbindAction(player1Input.down, OnDownPerformed);
        UnbindAction(player1Input.left, OnLeftPerformed);
        UnbindAction(player1Input.right, OnRightPerformed);
        UnbindAction(player1Input.select, OnSelectPerformed);
        UnbindAction(backAction, OnBackPerformed);
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
        if (isTransitioning)
        {
            return;
        }

        string stageName = GetStageName(stagePosition);
        if (stageName == "None")
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetStageSelection(stageName);
        }

        Debug.Log($"Stage selected: {stageName}");
        isTransitioning = true;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            isTransitioning = false;
        }
    }

    private void OnBackPerformed(InputAction.CallbackContext context)
    {
        if (isTransitioning)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetStageSelection("None");
        }

        isTransitioning = true;
        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (previousSceneIndex >= 0)
        {
            SceneManager.LoadScene(previousSceneIndex);
        }
        else
        {
            isTransitioning = false;
        }
    }

    private void MoveStage(Vector2Int delta)
    {
        if (isTransitioning)
        {
            return;
        }

        if (stageSlots == null || stageSlots.Length == 0)
        {
            return;
        }

        Vector2Int nextPosition = stagePosition + delta;
        if (nextPosition.x < 0 || nextPosition.x > 1 || nextPosition.y < 0 || nextPosition.y > 1)
        {
            return;
        }

        if (!IsSlotConfigured(nextPosition))
        {
            return;
        }

        stagePosition = nextPosition;
        RefreshMarkers();
    }

    private void EnsureValidStartPosition()
    {
        if (IsSlotConfigured(stagePosition))
        {
            return;
        }

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
        {
            return false;
        }

        StageSlot slot = stageSlots[slotIndex];
        return slot != null && slot.stageImage != null;
    }

    private string GetStageName(Vector2Int position)
    {
        int slotIndex = GetSlotIndex(position);
        if (slotIndex < 0 || slotIndex >= stageSlots.Length)
        {
            return "None";
        }

        StageSlot slot = stageSlots[slotIndex];
        if (slot == null || slot.stageImage == null)
        {
            return "None";
        }

        return slot.stageImage.name;
    }

    private void RefreshMarkers()
    {
        if (stageSlots == null)
        {
            return;
        }

        for (int i = 0; i < stageSlots.Length; i++)
        {
            StageSlot slot = stageSlots[i];
            if (slot == null || slot.marker == null)
            {
                continue;
            }

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
}
