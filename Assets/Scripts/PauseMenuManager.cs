using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject hudObject;
    [SerializeField] private Button[] buttons;
    [SerializeField] private TextMeshProUGUI[] buttonTexts;

    [SerializeField] private int mainMenuSceneIndex = 0;

    private int currentIndex;
    private float blinkSpeed = 5f;

    private void Start()
    {
        ResumeGame();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (hudObject != null)
            hudObject.SetActive(true);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "FightScene")
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused) ResumeGame();
            else PauseGame();
        }

        if (!IsPaused)
            return;

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = buttons.Length - 1;

            MusicManager.Instance?.PlayMenuMove();
            UpdateSelection();
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            currentIndex++;
            if (currentIndex >= buttons.Length) currentIndex = 0;

            MusicManager.Instance?.PlayMenuMove();
            UpdateSelection();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            MusicManager.Instance?.PlayMenuSelect();
            buttons[currentIndex].onClick.Invoke();
        }

        HandleBlink();
    }

    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (hudObject != null)
            hudObject.SetActive(false);

        Time.timeScale = 0f;
        IsPaused = true;

        MusicManager.Instance?.PlayMenuSelect();

        currentIndex = 1;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == currentIndex)
                buttons[i].Select();

            if (buttonTexts[i] != null)
                buttonTexts[i].alpha = 1f;
        }
    }

    private void HandleBlink()
    {
        for (int i = 0; i < buttonTexts.Length; i++)
        {
            if (buttonTexts[i] == null) continue;

            if (i == currentIndex)
            {
                float alpha = (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1f) / 2f;
                buttonTexts[i].alpha = alpha;
            }
            else
            {
                buttonTexts[i].alpha = 1f;
            }
        }
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (hudObject != null)
            hudObject.SetActive(true);

        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void OnReturnButton()
    {
        ResumeGame();
    }

    public void OnBackToMenuButton()
    {
        ResumeGame();
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
}