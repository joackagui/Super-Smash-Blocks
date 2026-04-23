using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private InputActionReference playAction;
    [SerializeField] private InputActionReference quitAction;

    private bool _isTransitioning;

    private void Start()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayMusic(MusicManager.Instance.mainMenuMusic);
    }

    private void OnEnable()
    {
        Bind(playAction, OnPlayPerformed);
        Bind(quitAction, OnQuitPerformed);
    }

    private void OnDisable()
    {
        Unbind(playAction, OnPlayPerformed);
        Unbind(quitAction, OnQuitPerformed);
    }

    private static void Bind(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
    {
        if (r == null || r.action == null) return;
        r.action.performed += cb;
        r.action.Enable();
    }

    private static void Unbind(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
    {
        if (r == null || r.action == null) return;
        r.action.performed -= cb;
    }

    private void OnPlayPerformed(InputAction.CallbackContext _) => Play();
    private void OnQuitPerformed(InputAction.CallbackContext _) => Quit();

    public void Play()
    {
        if (_isTransitioning) return;

        if (MusicManager.Instance != null)
            MusicManager.Instance.StopMusic();

        _isTransitioning = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}