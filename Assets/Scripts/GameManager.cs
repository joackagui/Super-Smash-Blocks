using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Player player1;
    public Player player2;
    public Character character1;
    public Character character2;
    public GameObject batmanPrefab;
    public GameObject redHoodPrefab;
    public GameObject jokerPrefab;
    private string player1Selection = "None";
    private string player2Selection = "None";

    private string stageSelection = "None";

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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    public Character CreateCharacter(string playerSelection){
        if (playerSelection == "Batman")
        {
            return Instantiate(batmanPrefab).GetComponent<Character>();
        } 
        else if (playerSelection == "Joker")
        {
            return Instantiate(jokerPrefab).GetComponent<Character>();
        } 
        else if (playerSelection == "RedHood")
        {
            return Instantiate(redHoodPrefab).GetComponent<Character>();
        }
        return null;
    }

    public void CreatePlayers()
    {
        character1 = CreateCharacter(player1Selection);
        character2 = CreateCharacter(player2Selection);
        player1.SetCharacter(character1);
        player2.SetCharacter(character2);
    }
}
