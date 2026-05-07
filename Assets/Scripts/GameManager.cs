using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameMode
{
    SinglePlayer,
    Multiplayer
}

public enum EnemyDifficulty
{
    Normal,
    Hard
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const string FightSceneName = "FightScene";
    private const string WinnerSceneName = "VictoryScene";

    [SerializeField] private GameMode currentMode = GameMode.Multiplayer;

    public Player player1;
    public Player player2;
    public Character character1;
    public Character character2;
    public GameObject batmanPrefab;
    public GameObject redHoodPrefab;
    public GameObject jokerPrefab;

    private bool firstTimeFightSceneLoaded = true;
    private Coroutine fightIntroRoutine;
    private CameraController fightCameraController;

    private const float FightIntroDuration = 4f;
    private static readonly Vector3 FightIntroCameraPosition = new Vector3(0f, 3f, 0f);
    private static readonly Quaternion FightIntroPlayer1Rotation = Quaternion.Euler(0f, -90f, 0f);
    private static readonly Quaternion FightIntroPlayer2Rotation = Quaternion.Euler(0f, 90f, 0f);

    private string player1Selection = "None";
    private string player2Selection = "None";
    private string stageSelection = "None";
    private string winnerSelection = "None";
    private EnemyDifficulty enemyDifficulty = EnemyDifficulty.Normal;

    private bool isVictoryLoading = false;

    public GameMode CurrentMode
    {
        get => currentMode;
        set => currentMode = value;
    }

    public bool IsSinglePlayer => currentMode == GameMode.SinglePlayer;

    public EnemyDifficulty EnemyAILevel => enemyDifficulty;

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
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        InitializeFightScene(SceneManager.GetActiveScene());
        Cursor.visible = false;
    }

    public Player GetPlayer1()
    {
        return player1;
    }

    public Player GetPlayer2()
    {
        return player2;
    }

    public void SetPlayer1(Player player)
    {
        player1 = player;
    }

    public void SetPlayer2(Player player)
    {
        player2 = player;
    }

    public string GetPlayer1Selection()
    {
        return player1Selection;
    }

    public string GetPlayer2Selection()
    {
        return player2Selection;
    }

    public string GetStageSelection()
    {
        return stageSelection;
    }

    public string GetWinnerSelection()
    {
        return winnerSelection;
    }

    public bool GetFirstTimeFightSceneLoaded()
    {
        return firstTimeFightSceneLoaded;
    }

    public void SetFirstTimeFightSceneLoaded(bool value)
    {
        firstTimeFightSceneLoaded = value;
    }

    public void SetPlayer1Selection(string selection)
    {
        player1Selection = selection;
    }

    public void SetPlayer2Selection(string selection)
    {
        player2Selection = selection;
    }

    public void SetStageSelection(string selection)
    {
        stageSelection = selection;
    }

    public void SetWinnerSelection(string selection)
    {
        winnerSelection = selection;
    }

    public void SetEnemyDifficulty(EnemyDifficulty difficulty)
    {
        enemyDifficulty = difficulty;
    }

    public EnemyDifficulty GetEnemyDifficulty()
    {
        return enemyDifficulty;
    }

    public void ClearSelections()
    {
        player1Selection = "None";
        player2Selection = "None";
        stageSelection = "None";
        currentMode = GameMode.Multiplayer;
        enemyDifficulty = EnemyDifficulty.Normal;
    }

    public void ClearStageSelection()
    {
        stageSelection = "None";
    }

    public void ClearWinnerSelection()
    {
        winnerSelection = "None";
        isVictoryLoading = false;
    }

    public void SetGameMode(GameMode mode)
    {
        currentMode = mode;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == FightSceneName)
        {
            InitializeFightScene(scene);
        }
    }

    private void InitializeFightScene(Scene scene)
    {
        if (!scene.IsValid() || scene.name != FightSceneName)
        {
            return;
        }

        isVictoryLoading = false;
        fightCameraController = FindAnyObjectByType<CameraController>();
        CacheFightScenePlayers();

        SetFightInputState(false);

        if (player1 != null)
        {
            SpawnCharacter(player1);
        }

        if (player2 != null)
        {
            SpawnCharacter(player2);
        }

        if (fightIntroRoutine != null)
        {
            StopCoroutine(fightIntroRoutine);
        }

        fightIntroRoutine = StartCoroutine(PlayFightIntroSequence());
    }

    private void CacheFightScenePlayers()
    {
        player1 = null;
        player2 = null;

        Player[] scenePlayers = FindObjectsByType<Player>(FindObjectsInactive.Exclude);

        foreach (Player scenePlayer in scenePlayers)
        {
            RegisterPlayer(scenePlayer);
        }
    }

    public void RegisterPlayer(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (player.Slot == Player.PlayerSlot.Player1 || player.name == "Player1")
        {
            player1 = player;
            return;
        }

        if (player.Slot == Player.PlayerSlot.Player2 || player.name == "Player2")
        {
            player2 = player;
            return;
        }

        if (player1 == null)
        {
            player1 = player;
            return;
        }

        if (player2 == null)
        {
            player2 = player;
        }
    }

    public void UnregisterPlayer(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (player1 == player)
        {
            player1 = null;
        }

        if (player2 == player)
        {
            player2 = null;
        }
    }

    public Character CreateCharacter(string playerSelection)
    {
        return CreateCharacter(playerSelection, Vector3.zero, Quaternion.identity);
    }

    public Character CreateCharacter(string playerSelection, Vector3 position, Quaternion rotation)
    {
        if (playerSelection == "Batman")
        {
            return Instantiate(batmanPrefab, position, rotation).GetComponent<Character>();
        }
        else if (playerSelection == "Joker")
        {
            return Instantiate(jokerPrefab, position, rotation).GetComponent<Character>();
        }
        else if (playerSelection == "RedHood")
        {
            return Instantiate(redHoodPrefab, position, rotation).GetComponent<Character>();
        }

        return null;
    }

    public void CreatePlayers()
    {
        CacheFightScenePlayers();

        if (player1 != null)
        {
            SpawnCharacter(player1);
        }

        if (player2 != null)
        {
            SpawnCharacter(player2);
        }
    }

    public void SpawnCharacter(Player player)
    {
        if (player == null)
        {
            return;
        }

        string selection = GetSelectionForPlayer(player);
        GameObject playerSpawnPoint = player.GetSpawnPoint();

        Vector3 spawnPosition = playerSpawnPoint != null
            ? playerSpawnPoint.transform.position
            : player.transform.position;

        Quaternion spawnRotation = playerSpawnPoint != null
            ? playerSpawnPoint.transform.rotation
            : player.transform.rotation;

        Character character = CreateCharacter(selection, spawnPosition, spawnRotation);

        if (character == null) return;

        player.SetCharacter(character);

    }

    private IEnumerator PlayFightIntroSequence()
    {
        yield return null;

        CameraController cameraController = GetFightCameraController();

        if (cameraController != null)
        {
            cameraController.SetIntroActive(true);
            cameraController.SetIntroPose(FightIntroCameraPosition, FightIntroPlayer1Rotation);
        }

        if (player1 != null && player1.character != null)
        {
            player1.character.PlayIntroAnimation();
        }

        yield return new WaitForSeconds(FightIntroDuration);

        if (cameraController != null)
        {
            cameraController.SetIntroPose(FightIntroCameraPosition, FightIntroPlayer2Rotation);
        }

        if (player2 != null && player2.character != null)
        {
            player2.character.PlayIntroAnimation();
        }

        yield return new WaitForSeconds(FightIntroDuration);

        if (cameraController != null)
        {
            cameraController.SetIntroActive(false);
            cameraController.RefreshCharacters();
        }

        ActivateSinglePlayerEnemy();

        SetFightInputState(true);
        fightIntroRoutine = null;
    }

    private void ActivateSinglePlayerEnemy()
    {
        if (!IsSinglePlayer || player2 == null || player2.character == null)
        {
            return;
        }

        EnsureNavMeshAgent(player2.character);
        EnsureEnemyAI(player2.character);
    }

    private void SetFightInputState(bool enabled)
    {
        if (player1 != null)
        {
            player1.SetKeybinds(enabled);
        }

        if (player2 != null)
        {
            player2.SetKeybinds(enabled);
        }
    }

    private CameraController GetFightCameraController()
    {
        if (fightCameraController != null)
        {
            return fightCameraController;
        }

        fightCameraController = FindAnyObjectByType<CameraController>();
        return fightCameraController;
    }

    private void EnsureNavMeshAgent(Character character)
    {
        if (character == null)
        {
            return;
        }

        NavMeshAgent agent = character.GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            agent = character.gameObject.AddComponent<NavMeshAgent>();
        }

        agent.speed = enemyDifficulty == EnemyDifficulty.Hard ? 5.5f : 4f;
        agent.acceleration = enemyDifficulty == EnemyDifficulty.Hard ? 14f : 10f;
        agent.angularSpeed = 0f;
        agent.stoppingDistance = enemyDifficulty == EnemyDifficulty.Hard ? 1.3f : 1.7f;
        agent.autoBraking = false;
        agent.autoTraverseOffMeshLink = false;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.baseOffset = 0f;

        if (NavMesh.SamplePosition(character.transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    private void EnsureEnemyAI(Character character)
    {
        if (character == null)
        {
            return;
        }

        NavMeshEnemyAI ai = character.GetComponent<NavMeshEnemyAI>();

        if (ai == null)
        {
            ai = character.gameObject.AddComponent<NavMeshEnemyAI>();
        }

        ai.SetDifficulty(enemyDifficulty);
    }

    private string GetSelectionForPlayer(Player player)
    {
        if (player == player1)
        {
            return player1Selection;
        }

        if (player == player2)
        {
            return player2Selection;
        }

        return "None";
    }

    public void Victory(Player defeatedPlayer)
    {
        if (isVictoryLoading)
        {
            return;
        }

        isVictoryLoading = true;

        if (defeatedPlayer == player1)
        {
            winnerSelection = player2Selection;
        }
        else if (defeatedPlayer == player2)
        {
            winnerSelection = player1Selection;
        }
        else
        {
            winnerSelection = "None";
        }

        SceneManager.LoadScene(WinnerSceneName);
    }
}