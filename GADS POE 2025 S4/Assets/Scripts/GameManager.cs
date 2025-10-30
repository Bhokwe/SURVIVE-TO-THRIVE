using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton Pattern
    public static GameManager Instance { get; private set; }

    [Header("System Preferences")]

    public PlayerStats playerStats;
    public EventManager eventManager;
    public UIManager uiManager;

    [Header("Game State")]
    public GamePhase currentPhase;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        playerStats.currentDay = 1;
        StartNewDay();
    }

    public void StartNewDay()
    {
        //check for the game over conditions from the previous night
        if (CheckForGameOver())
        {
            return;
        }

        //Increase the day counter
        if(playerStats.currentDay > 1)
        {
            playerStats.currentDay++;
        }

        //set the phase to morning
        currentPhase = GamePhase.Morning;   

        Debug.Log($"--- Day {playerStats.currentDay} - Morning ---");

    }
    public void AdvanceToAfternoon()
    {
        currentPhase = GamePhase.Afternoon;
        Debug.Log($"--- Day {playerStats.currentDay} - Afternoon ---");
    }

    public void AdvanceToEvening()
    {
        currentPhase = GamePhase.Evening;
        Debug.Log($"--- Day {playerStats.currentDay} - Evening ---");
    }


    public void EndDay()
    {
        Debug.Log($"--- Day {playerStats.currentDay} has ended. ---");
        StartNewDay();
    }

    public bool CheckForGameOver()
    {
        // Lose if Hope or Health is 0, or Money is too low.
        if (playerStats.currentHope <= 0)
        {
            Debug.Log("GAME OVER: You lost all hope.");
            // uiManager.ShowGameOverScreen("You lost all hope.");
            return true;
        }
        if (playerStats.currentHealth <= 0)
        {
            Debug.Log("GAME OVER: Your health failed.");
            // uiManager.ShowGameOverScreen("Your health failed.");
            return true;
        }
        if (playerStats.currentMoney < -1000) // Your GDD example
        {
            Debug.Log("GAME OVER: You fell too deep into debt.");
            // uiManager.ShowGameOverScreen("You fell too deep into debt.");
            return true;
        }
        return false;
    }

    public void AddStatusEffect(string effectToAdd)
    {
        if (string.IsNullOrEmpty(effectToAdd)) return;

        if (!playerStats.activeStatusEffects.Contains(effectToAdd))
        {
            playerStats.activeStatusEffects.Add(effectToAdd);
            Debug.Log($"Status Effect Added: {effectToAdd}");
            //uiManager.UpdateStatusEffectsUI();
        }
    }

    public void RemoveStatusEffect(string effectToRemove)
    {
        if (string.IsNullOrEmpty(effectToRemove)) return;
        if (playerStats.activeStatusEffects.Contains(effectToRemove))
        {
            playerStats.activeStatusEffects.Remove(effectToRemove);
            Debug.Log($"Status Effect Removed: {effectToRemove}");
            //uiManager.UpdateStatusEffectsUI();
        }
    }


}
