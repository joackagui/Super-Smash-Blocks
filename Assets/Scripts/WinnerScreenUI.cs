using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class WinnerScreenUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private InputActionReference nextAction;
    [SerializeField] private float blinkSpeed = 1.2f;

    private bool isLoading;
    private bool canInteract;
    private Coroutine blinkRoutine;

    private void Awake()
    {
        // Ensure promptText is hidden at start
        if (promptText != null)
        {
            SetAlphaTMP(promptText, 0f);
            promptText.gameObject.SetActive(false);
        }
        
        canInteract = false;
    }

    private void OnEnable()
    {
        BindAction(nextAction, OnNextPerformed);
    }

    private void OnDisable()
    {
        UnbindAction(nextAction, OnNextPerformed);
    }

    public void ShowPrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            SetAlphaTMP(promptText, 1f);
            
            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
            }
            
            blinkRoutine = StartCoroutine(BlinkText(promptText));
            canInteract = true;
        }
    }

    private IEnumerator BlinkText(TextMeshProUGUI tmp)
    {
        while (true)
        {
            float alpha = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f) / 2f;
            SetAlphaTMP(tmp, alpha);
            yield return null;
        }
    }

    private void SetAlphaTMP(TextMeshProUGUI tmp, float alpha)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }

    private static void BindAction(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
    {
        if (r == null || r.action == null) return;
        r.action.performed += cb;
        r.action.Enable();
    }

    private static void UnbindAction(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
    {
        if (r == null || r.action == null) return;
        r.action.performed -= cb;
    }

    private void OnNextPerformed(InputAction.CallbackContext context)
    {
        if (isLoading || !canInteract) return;
        isLoading = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearSelections();
            GameManager.Instance.ClearWinnerSelection();
        }

        SceneManager.LoadScene(0);
    }
}