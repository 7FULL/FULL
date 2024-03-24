using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoorHorrorMaze : MonoBehaviour
{
    //Singleton
    public static ExitDoorHorrorMaze Instance { get; private set; }
    
    [SerializeField]
    [InspectorName("Interruptors")]
    private Interruptor[] interruptors;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void CheckInterruptors()
    {
        bool allOn = true;
        
        foreach (Interruptor interruptor in interruptors)
        {
            if (!interruptor.IsOn)
            {
                allOn = false;
                break;
            }
        }
        
        if (allOn)
        {
            //We activate the collider
            GetComponent<BoxCollider>().enabled = true;
            
            MenuManager.Instance.PopUp("You can now exit the maze");
        }
    }

    public void Exit(Player player)
    {
        //We add coins to the player and then send it to the lobby
        player.AddCoins(1000);
        
        MenuManager.Instance.PopUp("You have escaped the maze!!!");
        
        player.GoToLobby();
    }
}
