using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ControlsSceneManager : MonoBehaviour
{
    [SerializeField] private InputActionReference continueAction;
    [SerializeField] private string fightSceneName = "FightScene";

    private bool isLoading;

    private void OnEnable()
    {
        BindAction(continueAction, OnContinuePerformed);
    }

    private void OnDisable()
    {
        UnbindAction(continueAction, OnContinuePerformed);
    }

    private static void BindAction(
        InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> onPerformed)
    {
        if (actionReference == null || actionReference.action == null)
            return;

        actionReference.action.performed += onPerformed;
        actionReference.action.Enable();
    }

    private static void UnbindAction(
        InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> onPerformed)
    {
        if (actionReference == null || actionReference.action == null)
            return;

        actionReference.action.performed -= onPerformed;
    }

    private void OnContinuePerformed(InputAction.CallbackContext context)
    {
        if (isLoading) return;

        isLoading = true;
        SceneManager.LoadScene(fightSceneName);
    }
}