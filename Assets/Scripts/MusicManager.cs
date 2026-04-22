using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip mainMenuMusic;

    public AudioClip fightMusic;

    public AudioClip victoryMusic;

    public AudioClip selectionMusic;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenuScene":
                PlayMusic(mainMenuMusic);
                break;

            case "CharacterSelectionScene":
                PlayMusic(selectionMusic);
                break;

            case "StageSelectionScene":
                PlayMusic(selectionMusic);
                break;

            case "Stage1Scene":
                PlayMusic(fightMusic);
                break;

            case "Stage2Scene":
                PlayMusic(fightMusic);
                break;
        }
    }
    public void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip && audioSource.isPlaying) return;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
