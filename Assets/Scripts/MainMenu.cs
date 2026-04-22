using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    void Start()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayMusic(MusicManager.Instance.mainMenuMusic);
    }

    public void Play()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.StopMusic();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
