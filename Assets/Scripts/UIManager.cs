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

    [Header("Stage")]
    [SerializeField] private RawImage stageImage;
    [SerializeField] private Texture defaultBackground;
    [SerializeField] private StageBackground[] stageBackgrounds;
    [SerializeField] private StageColor[] stageColors;
    [SerializeField] private Renderer[] platforms;

    [Header("Player Character Images")]
    [SerializeField] private RawImage player1CharacterImage;
    [SerializeField] private RawImage player2CharacterImage;

    [Header("Player Hearts")]
    [SerializeField] private RawImage[] player1Hearts;
    [SerializeField] private RawImage[] player2Hearts;

    [Header("Icon Images")]
    [SerializeField] private Texture batmanImage;
    [SerializeField] private Texture jokerImage;
    [SerializeField] private Texture redHoodImage;

    [Header("Input")]
    [SerializeField] private InputActionReference returnToMainMenuAction;

    private bool isLoadingMainMenu;

    private void Awake()
    {
        ValidateGameManager();
    }

    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name != "FightScene")
        {
            BindAction(returnToMainMenuAction, OnReturnToMainMenuPerformed);
        }
    }

    private void OnDisable()
    {
        UnbindAction(returnToMainMenuAction, OnReturnToMainMenuPerformed);
    }

    private void Start()
    {
        if (stageImage == null)
            stageImage = GetComponent<RawImage>();

        ApplyStageBackground();
        ApplyPlatformColor();
        ApplyCharacterImages();
        RefreshAllHearts();
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

    private void ApplyStageBackground()
    {
        if (stageImage == null)
            return;

        string stageSelection = GameManager.Instance != null ? GameManager.Instance.GetStageSelection() : "None";
        stageSelection = Normalize(stageSelection);

        Texture selectedTexture = defaultBackground;

        if (stageBackgrounds != null)
        {
            for (int i = 0; i < stageBackgrounds.Length; i++)
            {
                if (stageBackgrounds[i] == null)
                    continue;

                if (Normalize(stageBackgrounds[i].stageName) == stageSelection)
                {
                    selectedTexture = stageBackgrounds[i].texture;
                    break;
                }
            }
        }

        stageImage.texture = selectedTexture;
    }

    private void ApplyPlatformColor()
    {
        if (platforms == null || platforms.Length == 0)
            return;

        string stageSelection = GameManager.Instance != null ? GameManager.Instance.GetStageSelection() : "None";
        stageSelection = Normalize(stageSelection);

        Color selectedColor = Color.white;

        if (stageColors != null)
        {
            for (int i = 0; i < stageColors.Length; i++)
            {
                if (stageColors[i] == null)
                    continue;

                if (Normalize(stageColors[i].stageName) == stageSelection)
                {
                    selectedColor = stageColors[i].color;
                    break;
                }
            }
        }

        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] == null)
                continue;

            Material[] mats = platforms[i].materials;

            for (int j = 0; j < mats.Length; j++)
            {
                mats[j].color = selectedColor;
            }
        }
    }

    private void ApplyCharacterImages()
    {
        if (GameManager.Instance == null)
            return;

        ApplySingleCharacterImage(player1CharacterImage, GameManager.Instance.GetPlayer1Selection());
        ApplySingleCharacterImage(player2CharacterImage, GameManager.Instance.GetPlayer2Selection());
    }

    private void ApplySingleCharacterImage(RawImage target, string selection)
    {
        if (target == null)
            return;

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

    public void RefreshAllHearts()
    {
        if (GameManager.Instance == null)
            return;

        Player p1 = GameManager.Instance.GetPlayer1();
        Player p2 = GameManager.Instance.GetPlayer2();

        SetHearts(Player.PlayerSlot.Player1, p1 != null ? p1.lives : 3);
        SetHearts(Player.PlayerSlot.Player2, p2 != null ? p2.lives : 3);
    }

    public void SetHearts(Player.PlayerSlot slot, int lives)
    {
        RawImage[] hearts = slot == Player.PlayerSlot.Player1 ? player1Hearts : player2Hearts;

        if (hearts == null || hearts.Length == 0)
            return;

        int max = hearts.Length;
        int active = Mathf.Clamp(lives, 0, max);

        for (int i = 0; i < max; i++)
        {
            if (hearts[i] != null)
                hearts[i].gameObject.SetActive(true);
        }

        for (int i = active; i < max; i++)
        {
            int index;

            if (slot == Player.PlayerSlot.Player1)
                index = max - 1 - (i - active);
            else
                index = i - active;

            if (index >= 0 && index < hearts.Length && hearts[index] != null)
                hearts[index].gameObject.SetActive(false);
        }
    }

    private void OnReturnToMainMenuPerformed(InputAction.CallbackContext context)
    {
        if (SceneManager.GetActiveScene().name == "FightScene")
            return;

        if (isLoadingMainMenu)
            return;

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

    private string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "None" : value.Trim();
    }
}