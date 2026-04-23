using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Character player1;
    private Character player2;
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
        // switch (player1Selection)
        // {
        //     case "Batman":
        //         player1 = new Batman();
        //         break;
        //     case "Joker":
        //         player1 = new Joker();
        //         break;
        // }
        // switch (player2Selection)
        // {
        //     case "Batman":
        //         player2 = new Batman();
        //         break;
        //     case "Joker":
        //         player2 = new Joker();
        //         break;
        // }
        // //player1.Transform.position = new Vector3(-10, 0.5f, 0);
        // // player2.Transform.position = new Vector3(10, 0.5f, 0);
    }
}
