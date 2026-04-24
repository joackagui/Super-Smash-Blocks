using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    private class StageBackground
    {
        public string stageName;
        public Texture texture;
    }

    [System.Serializable]
    private class StageColor
    {
        public string stageName;
        public Color color;
    }

    [SerializeField] private RawImage stageImage;
    [SerializeField] private Texture defaultBackground;
    [SerializeField] private StageBackground[] stageBackgrounds;
    [SerializeField] private StageColor[] stageColors;
    [SerializeField] private Renderer[] platforms;
    [SerializeField] private InputActionReference returnToMainMenuAction;

    private bool isLoadingMainMenu;

    private void OnEnable()
    {
        BindAction(returnToMainMenuAction, OnReturnToMainMenuPerformed);
    }

    private void OnDisable()
    {
        UnbindAction(returnToMainMenuAction, OnReturnToMainMenuPerformed);
    }

    private void Start()
    {
        if (stageImage == null)
        {
            stageImage = GetComponent<RawImage>();
        }

        ApplyStageBackground();
        ApplyPlatformColor();
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

    private void ApplyStageBackground()
    {
        if (stageImage == null)
        {
            return;
        }

        string stageSelection = "None";
        if (GameManager.Instance != null)
        {
            stageSelection = GameManager.Instance.GetStageSelection();
        }

        Texture selectedTexture = defaultBackground;
        for (int i = 0; i < stageBackgrounds.Length; i++)
        {
            StageBackground entry = stageBackgrounds[i];
            if (entry == null || string.IsNullOrEmpty(entry.stageName))
            {
                continue;
            }

            if (entry.stageName == stageSelection)
            {
                selectedTexture = entry.texture;
                break;
            }
        }

        stageImage.texture = selectedTexture;
    }

    private void ApplyPlatformColor()
    {
        if (platforms == null || platforms.Length == 0)
        {
            return;
        }

        string stageSelection = "None";
        if (GameManager.Instance != null)
        {
            stageSelection = GameManager.Instance.GetStageSelection();
        }

        Color selectedColor = Color.white;

        for (int i = 0; i < stageColors.Length; i++)
        {
            StageColor entry = stageColors[i];
            if (entry == null || string.IsNullOrEmpty(entry.stageName))
            {
                continue;
            }

            if (entry.stageName == stageSelection)
            {
                selectedColor = entry.color;
                break;
            }
        }

        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] == null) continue;

            Material[] mats = platforms[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                mats[j].color = selectedColor;
            }
        }
    }

    private void OnReturnToMainMenuPerformed(InputAction.CallbackContext context)
    {
        if (isLoadingMainMenu)
        {
            return;
        }

        isLoadingMainMenu = true;
        SceneManager.LoadScene(0);
    }
}