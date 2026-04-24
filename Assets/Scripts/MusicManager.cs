using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip fightMusic;
    public AudioClip victoryMusic;
    public AudioClip selectionMusic;

    [Header("SFX Clips")]
    public AudioClip menuMoveClip;
    public AudioClip menuSelectClip;
    public AudioClip menuBackClip;
    public AudioClip menuErrorClip;
    // Sin [SerializeField] — se crean por código
    private AudioSource musicSource;
    private AudioSource sfxSource;

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

        // Se crean en el mismo GameObject que sobrevive entre escenas
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

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
            case "FightScene":
                PlayMusic(fightMusic);
                break;
            case "VictoryScene":
                PlayMusic(victoryMusic);
                break;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlayMenuMove()
    {
        if (menuMoveClip == null) return;
        sfxSource.PlayOneShot(menuMoveClip);
    }

    public void PlayMenuSelect()
    {
        if (menuSelectClip == null) return;
        sfxSource.PlayOneShot(menuSelectClip);
    }

    public void PlayMenuBack()
    {
        if (menuBackClip == null) return;
        sfxSource.PlayOneShot(menuBackClip);
    }

    public void PlayMenuError()
    {
        if (menuErrorClip == null) return;
        sfxSource.PlayOneShot(menuErrorClip);
    }
}