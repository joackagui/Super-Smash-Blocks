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
    private class CharacterImageEntry
    {
        public RawImage characterImage;
    }
    [System.Serializable]
    private class StageColor
    {
        public string stageName;
        public Color color;
    }

    [Header("Stage")]
    [SerializeField] private RawImage stageImage;
    [SerializeField] private Texture defaultBackground;
    [SerializeField] private StageBackground[] stageBackgrounds;
    [SerializeField] private StageColor[] stageColors;
    [SerializeField] private Renderer[] platforms;

    [Header("Player Character Images")]
    [SerializeField] private RawImage player1CharacterImage;
    [SerializeField] private RawImage player2CharacterImage;

    [Header("Icon Images")]
    [SerializeField] private Texture batmanImage;
    [SerializeField] private Texture jokerImage;
    [SerializeField] private Texture redHoodImage;

    [Header("Input")]
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

    private void Awake()
    {
        ValidateGameManager();
    }

    private void Start()
    {
        if (stageImage == null)
        {
            stageImage = GetComponent<RawImage>();
        }

        ApplyStageBackground();
        ApplyPlatformColor();
        ApplyCharacterImages();
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

    private void ApplyCharacterImages()
    {
        if (GameManager.Instance == null) return;

        ApplySingleCharacterImage(
            player1CharacterImage,
            GameManager.Instance.GetPlayer1Selection()
        );

        ApplySingleCharacterImage(
            player2CharacterImage,
            GameManager.Instance.GetPlayer2Selection()
        );
    }

    private void ApplySingleCharacterImage(RawImage target, string selection)
    {
        if (target == null) return;

        if (selection == "Batman")
        {
            target.texture = batmanImage;
        }
        else if (selection == "Joker")
        {
            target.texture = jokerImage;
        }
        else if (selection == "RedHood")
        {
            target.texture = redHoodImage;
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

    private void ValidateGameManager()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene(0);
        }
    }
}