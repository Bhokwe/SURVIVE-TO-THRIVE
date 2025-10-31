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
        eventManager initialEvent = Resources.Load<EventData>("PlaceholderEvent");

        if (initialEvent == null)
        {
            Debug.LogError("ERROR: PlaceholderEvent not found. Game cannot start!");
            return;
        }
        StartNewDay(initialEvent);
    }

    public void StartNewDay(EventData morningEvent = null)
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

        ApplyStatusConsequences();


        //set the phase to morning
        currentPhase = GamePhase.Morning;   

        Debug.Log($"--- Day {playerStats.currentDay} - Morning ---");

        uiManager.UpdateDayUI();
        currentEvent = morningEvent;
        if (currentEvent != null)
        {
            uiManager.DisplayEvent(currentEvent);
        }
        else
        {
            Debug.LogWarning("No Morning Event available! Need random event logic.");
            // **TODO: Implement random event selection logic here.**
        }

    }
    public void AdvanceToAfternoon()
    {
        currentPhase = GamePhase.Afternoon;
        currentEvent = afternoonEvent;
        Debug.Log($"--- Day {playerStats.currentDay} - Afternoon ---");
        // Display the afternoon event/consequence
        uiManager.UpdateStatUI();
        uiManager.DisplayEvent(currentEvent);
    }

    public void AdvanceToEvening()
    {
        currentPhase = GamePhase.Evening;
        Debug.Log($"--- Day {playerStats.currentDay} - Evening ---");
        EndDay(currentEvent.defaultNextEvent);
    }


    public void EndDay()
    {
        Debug.Log($"--- Day {playerStats.currentDay} has ended. ---");
        StartNewDay();
    }
    // --- CORE MECHANIC: PROCESS CHOICE ---
    // This function is called by the UIManager when the player clicks an event button.
    public void MakeChoice(EventChoice choice)
    {
        // 1. Apply all the outcomes (stat changes, status effects)
        ProcessOutcomes(choice.outcomes);

        // 2. Update the UI immediately to reflect the changes
        uiManager.UpdateStatUI();
        uiManager.HideEventPanel();

        // 3. Determine the next step based on the choice and the current phase.
        if (choice.nextEvent != null)
        {
            AdvanceToAfternoon(choice.nextEvent);
        }
        else if (currentEvent.eventphase == GamePhase.Morning)
        {
            AdvanceToAfternoon(currentEvent.defaultNextEvent);
        }
        else if (currentEvent.eventphase == GamePhase.Afternoon)
        {
            AdvanceToEvening();
        }
        else if (currentEvent.eventphase == GamePhase.Evening)
        {
            // If no next event is specified, advance the game phase.
            if (currentPhase == GamePhase.Morning)
            {
                AdvanceToAfternoon();
            }
            else if (currentPhase == GamePhase.Afternoon)
            {
                AdvanceToEvening();
            }
            else
            {
                EndDay();
            }
        }
    }
    private void  ProcessOutcomes(System.Collections.Generic.List<EventOutcome> outcomes) //SD
    {
        foreach (EventOutcome outcome in outcomes)
        {
            switch (outcome.statToChange)
            {
                case StatType.Hope:
                    playerStats.currentHope = Mathf.Clamp(playerStats.currentHope + (int)outcome.amountToChange, 0, 100);
                    break;
                case StatType.Health:
                    playerStats.currentHealth = Mathf.Clamp(playerStats.currentHealth + (int)outcome.amountToChange, 0, 100);
                    break;
                case StatType.Money:
                    playerStats.currentMoney += outcome.moneyChange;
                    break;
                case StatType.CommunityTrust:
                    playerStats.currentCommunityTrust = Mathf.Clamp(playerStats.currentCommunityTrust + (int)outcome.amountToChange, 0, 100);
                    break;
                default:
                    Debug.LogWarning("Unknown stat type in outcome.");
                    break;
            }

            // Status Effect Management
            AddStatusEffect(outcome.statusEffectToAdd);
            RemoveStatusEffect(outcome.statusEffectToRemove);
            // Skill Gain Management
            if (!string.IsNullOrEmpty(outcome.skillToGain) && !playerStats.acquiredSkills.Contains(outcome.skillToGain))
            {
                playerStats.acquiredSkills.Add(outcome.skillToGain);
                Debug.Log($"Skill Gained: {outcome.skillToGain}");
            }
        }
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
