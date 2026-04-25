using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinnerScreenUI : MonoBehaviour
{
    [SerializeField] private RawImage winnerImage;
    [SerializeField] private Texture batmanImage;
    [SerializeField] private Texture jokerImage;
    [SerializeField] private Texture redHoodImage;
    [SerializeField] private Texture defaultImage;

    [SerializeField] private InputActionReference returnAction;

    [SerializeField] private string characterSelectionSceneName = "CharacterSelectionScene";

    private bool isLoading;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene(0);
            return;
        }

        ApplyWinnerImage();
    }

    private void OnEnable()
    {
        BindAction(returnAction, OnReturnPerformed);
    }

    private void OnDisable()
    {
        UnbindAction(returnAction, OnReturnPerformed);
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

    private void ApplyWinnerImage()
    {
        if (winnerImage == null || GameManager.Instance == null)
        {
            return;
        }

        string winner = GameManager.Instance.GetWinnerSelection();
        Texture selected = defaultImage;

        if (winner == "Batman")
        {
            selected = batmanImage;
        }
        else if (winner == "Joker")
        {
            selected = jokerImage;
        }
        else if (winner == "RedHood")
        {
            selected = redHoodImage;
        }

        winnerImage.texture = selected;
    }

    private void OnReturnPerformed(InputAction.CallbackContext context)
    {
        if (isLoading)
        {
            return;
        }

        isLoading = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearSelections();
            GameManager.Instance.ClearWinnerSelection();
        }

        SceneManager.LoadScene(characterSelectionSceneName);
    }
}