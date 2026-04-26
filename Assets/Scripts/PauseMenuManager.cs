using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject hudObject;

    [Header("Scenes")]
    [SerializeField] private int mainMenuSceneIndex = 0;

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
    }

    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (hudObject != null)
            hudObject.SetActive(false);

        Time.timeScale = 0f;
        IsPaused = true;
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