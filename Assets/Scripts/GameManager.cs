using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    private class CharacterPrefabEntry
    {
        public string selectionName;
        public GameObject prefab;
    }

    [System.Serializable]
    private class PlayerSpawnConfig
    {
        public Player.GameplayInputBindings inputBindings;
        public Vector3 spawnPosition;
    }

    public static GameManager Instance { get; private set; }

    [Header("Scene")]
    [SerializeField] private string fightSceneName = "FightScene";

    [Header("Character Prefabs")]
    [SerializeField] private CharacterPrefabEntry[] characterPrefabs;

    [Header("Player Setup")]
    [SerializeField] private PlayerSpawnConfig player1Config = new PlayerSpawnConfig
    {
        spawnPosition = new Vector3(10f, 2f, 0f)
    };
    [SerializeField] private PlayerSpawnConfig player2Config = new PlayerSpawnConfig
    {
        spawnPosition = new Vector3(-10f, 2f, 0f)
    };

    private Character player1;
    private Character player2;
    private string player1Selection = "None";
    private string player2Selection = "None";

    private string stageSelection = "None";

    private GameObject player1Root;
    private GameObject player2Root;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public Character GetPlayer1()
    {
        return player1;
    }
    public Character GetPlayer2()
    {
        return player2;
    }
    public void SetPlayer1(Character player)
    {
        player1 = player;
    }
    public void SetPlayer2(Character player)
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

    public void ClearSelections()
    {
        player1Selection = "None";
        player2Selection = "None";
        stageSelection = "None";
    }
    public void CreatePlayers(){
        DestroySpawnedPlayers();

        player1 = SpawnCharacter(player1Selection, player1Config, out player1Root);
        player2 = SpawnCharacter(player2Selection, player2Config, out player2Root);

        if (player1 == null)
        {
            Debug.LogWarning($"Could not spawn player 1. Selection: {player1Selection}");
        }

        if (player2 == null)
        {
            Debug.LogWarning($"Could not spawn player 2. Selection: {player2Selection}");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsFightScene(scene))
        {
            return;
        }

        CreatePlayers();
    }

    private bool IsFightScene(Scene scene)
    {
        if (!string.IsNullOrEmpty(fightSceneName))
        {
            return scene.name == fightSceneName;
        }

        return scene.buildIndex == 3;
    }

    private Character SpawnCharacter(string selectionName, PlayerSpawnConfig config, out GameObject root)
    {
        root = null;

        GameObject prefab = GetPrefabBySelection(selectionName);
        if (prefab == null)
        {
            return null;
        }

        GameObject spawned = Instantiate(prefab, config.spawnPosition, Quaternion.identity);
        Character characterComponent = spawned.GetComponent<Character>();
        if (characterComponent == null)
        {
            characterComponent = spawned.GetComponentInChildren<Character>();
        }

        if (characterComponent == null)
        {
            Debug.LogWarning($"Spawned prefab '{prefab.name}' has no Character component.");
            Destroy(spawned);
            return null;
        }

        Player playerComponent = spawned.GetComponent<Player>();
        if (playerComponent == null)
        {
            playerComponent = spawned.AddComponent<Player>();
        }

        playerComponent.Initialize(characterComponent, config.inputBindings);

        root = spawned;
        return characterComponent;
    }

    private GameObject GetPrefabBySelection(string selectionName)
    {
        if (string.IsNullOrEmpty(selectionName) || selectionName == "None" || characterPrefabs == null)
        {
            return null;
        }

        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            CharacterPrefabEntry entry = characterPrefabs[i];
            if (entry == null || entry.prefab == null)
            {
                continue;
            }

            if (string.Equals(entry.selectionName, selectionName, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.prefab.name, selectionName, System.StringComparison.OrdinalIgnoreCase))
            {
                return entry.prefab;
            }
        }

        return null;
    }

    private void DestroySpawnedPlayers()
    {
        if (player1Root != null)
        {
            Destroy(player1Root);
            player1Root = null;
            player1 = null;
        }

        if (player2Root != null)
        {
            Destroy(player2Root);
            player2Root = null;
            player2 = null;
        }
    }
}
