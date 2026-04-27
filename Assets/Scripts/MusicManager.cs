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

    [Header("Victory Music by Character")]
    public AudioClip batmanVictoryClip;
    public AudioClip batmanVictoryTheme;
    public AudioClip jokerVictoryClip;
    public AudioClip jokerVictoryTheme;
    public AudioClip redHoodVictoryClip;
    public AudioClip redHoodVictoryTheme;

    [Header("SFX Clips")]
    public AudioClip menuMoveClip;
    public AudioClip menuSelectClip;
    public AudioClip menuBackClip;
    public AudioClip menuErrorClip;
    public AudioClip deathClip;
    public AudioClip respawnClip;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource victorySource;

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

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        victorySource = gameObject.AddComponent<AudioSource>();
        victorySource.loop = false;
        victorySource.playOnAwake = false;
        victorySource.volume = musicVolume;
    }

    private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void Update()
    {
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        victorySource.volume = musicVolume;
    }

    private AudioClip GetVictoryClip()
    {
        if (GameManager.Instance == null) return victoryMusic;

        string winner = GameManager.Instance.GetWinnerSelection();

        return winner switch
        {
            "Batman"  => batmanVictoryClip  != null ? batmanVictoryClip  : victoryMusic,
            "Joker"   => jokerVictoryClip   != null ? jokerVictoryClip   : victoryMusic,
            "RedHood" => redHoodVictoryClip != null ? redHoodVictoryClip : victoryMusic,
            _         => victoryMusic
        };
    }

    private AudioClip GetVictoryTheme()
    {
        if (GameManager.Instance == null) return victoryMusic;

        string winner = GameManager.Instance.GetWinnerSelection();

        return winner switch
        {
            "Batman"  => batmanVictoryTheme  != null ? batmanVictoryTheme  : victoryMusic,
            "Joker"   => jokerVictoryTheme   != null ? jokerVictoryTheme   : victoryMusic,
            "RedHood" => redHoodVictoryTheme != null ? redHoodVictoryTheme : victoryMusic,
            _         => victoryMusic
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenuScene":
                PlayMusic(mainMenuMusic);
                break;
            case "FightScene":
                PlayMusic(fightMusic);
                break;
            case "VictoryScene":
                PlayVictoryMusic();
                break;
        }
    }

    private void PlayVictoryMusic()
    {
        AudioClip clip  = GetVictoryClip();
        AudioClip theme = GetVictoryTheme();

        if (clip != null)
        {
            victorySource.loop   = false;
            victorySource.clip   = clip;
            victorySource.volume = musicVolume;
            victorySource.Play();
        }

        if (theme != null)
        {
            musicSource.loop   = true;
            musicSource.clip   = theme;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.loop = loop;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic() { musicSource.Stop(); }

    public void PlayMenuMove()   { if (menuMoveClip   != null) sfxSource.PlayOneShot(menuMoveClip,   sfxVolume); }
    public void PlayMenuSelect() { if (menuSelectClip != null) sfxSource.PlayOneShot(menuSelectClip, sfxVolume); }
    public void PlayMenuBack()   { if (menuBackClip   != null) sfxSource.PlayOneShot(menuBackClip,   sfxVolume); }
    public void PlayMenuError()  { if (menuErrorClip  != null) sfxSource.PlayOneShot(menuErrorClip,  sfxVolume); }
    public void PlayCharacterDeath(){ if (deathClip != null) sfxSource.PlayOneShot(deathClip, sfxVolume); }
    public void PlayCharacterRespawn(){ if (respawnClip != null) sfxSource.PlayOneShot(respawnClip, sfxVolume); }
}