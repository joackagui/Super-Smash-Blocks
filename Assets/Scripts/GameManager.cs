using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Character player1;
    private Character player2;
    private string player1Selection = "none";
    private string player2Selection = "none";

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

    public void SetPlayer1Selection(string selection)
    {
        player1Selection = selection;
    }

    public void SetPlayer2Selection(string selection)
    {
        player2Selection = selection;
    }

    public void ClearSelections()
    {
        player1Selection = "none";
        player2Selection = "none";
    }
    public void CreatePlayers(){
        switch (player1Selection)
        {
            case "Batman":
                player1 = new Character();
                break;
        }
        switch (player2Selection)
        {
            case "Batman":
                player2 = new Character();
                break;
        }
        //player1.Transform.position = new Vector3(-10, 0.5f, 0);
        // player2.Transform.position = new Vector3(10, 0.5f, 0);
    }
}
