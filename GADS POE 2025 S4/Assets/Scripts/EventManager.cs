using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for .Where() and .ToList()
using UnityEngine;


public class EventManager : MonoBehaviour
{
    [Header("System References")]

    // We'll get these from the GameManager 
    private GameManager gameManager;
    private PlayerStats playerStats;
    private UIManager uiManager;

    [Header("Event Database")]
    [Tooltip("Drag all of your EventData ScriptableObjects here!")]
    public List<EventData> allEvents; // Our "database" of all possible events

    // Internal tracker for the event currently being shown
    private EventData currentEvent;

 

    void Start()
    { 
        // Get our references from the GameManager Singleton

        gameManager = GameManager.Instance;
        playerStats = gameManager.playerStats;
        uiManager = gameManager.uiManager; 
    }

    public void TriggerEvent(GamePhase phase)
    {
        //Find all events in our database that match the current phase
        List<EventData> validEvents = allEvents.Where(e => e.eventPhase == phase).ToList();

        // TODO: Add more complex filtering here in the future.
        // For example:
        // - Filter out events that have already been seen.
        // - Find events that REQUIRE a specific status effect (e.g., "Sick")
        // - Find events that REQUIRE a skill (e.g., "Baking")

        //Check if we found any valid events
        if (validEvents.Count > 0)
        {
            // 3. Pick a random event from the valid list
            EventData eventToTrigger = validEvents[Random.Range(0, validEvents.Count)];

            // 4. Call our function to show this event
            ShowEvent(eventToTrigger);
        }
        else
        {
            // 5. If no events are found for this phase, log an error
            // and advance the game state so it doesn't get stuck.
            Debug.LogWarning($"No events found for phase: {phase}. Advancing phase.");
            AdvanceGamePhase(); // Move on to prevent getting stuck
        }
    }

    
    public void ShowEvent(EventData eventToShow)
    {
        if (eventToShow == null)
        {
            Debug.LogError("ShowEvent was called with a null event!");
            return;
        }

        //Store this event as the "current" one
        currentEvent = eventToShow;

        //Tell the UIManager to display it
        uiManager.DisplayEvent(currentEvent);
    }


    public void MakeChoice(int choiceIndex, EventData chainedEvent)
    {
        //Get the EventChoice object the player selected
        EventChoice choice = currentEvent.choices[choiceIndex];

        //Reset any pending chained event
        //chainedEvent = null;

        //Process all the outcomes for that choice
        //EventData chainedEvent = null; // To store a potential follow-up event
        foreach (EventOutcome outcome in choice.outcomes)
        {
            ProcessOutcome(outcome);

            // Check if this outcome has a follow-up event
            if (outcome.nextEvent != null)
            {
                chainedEvent = outcome.nextEvent;
            }
        }

        //Tell the UIManager to show the Consequence Modal
        uiManager.ShowConsequenceModal(choice.outcomes, choice.educationalText);
    }

    private void ProcessOutcome(EventOutcome outcome)
    {
        //Change the correct stat based on the enum
        switch (outcome.statToChange)
        {
            case StatToChange.currentMoney:
                playerStats.currentMoney += outcome.amountToChange;
                break;

            // We use Mathf.Clamp to keep stats between 0 and 100
            case StatToChange.currentHealth:
                playerStats.currentHealth = Mathf.Clamp(playerStats.currentHealth + (int)outcome.amountToChange, 0, 100);
                break;
            case StatToChange.currentHope:
                playerStats.currentHope = Mathf.Clamp(playerStats.currentHope + (int)outcome.amountToChange, 0, 100);
                break;
            case StatToChange.currentCommunityTrust:
                playerStats.currentCommunityTrust = Mathf.Clamp(playerStats.currentCommunityTrust + (int)outcome.amountToChange, 0, 100);
                break;
            case StatToChange.currentDay:
                playerStats.currentDay += (int)outcome.amountToChange;
                break;
        }

        // Add/Remove Status Effects (using the GameManager's functions)
        if (!string.IsNullOrEmpty(outcome.statusEffectToAdd))
        {
            gameManager.AddStatusEffect(outcome.statusEffectToAdd);
        }
        if (!string.IsNullOrEmpty(outcome.statusEffectToRemove))
        {
            gameManager.RemoveStatusEffect(outcome.statusEffectToRemove);
        }

        //Add Skills (checking for duplicates)
        if (!string.IsNullOrEmpty(outcome.skillToGain))
        {
            if (!playerStats.acquiredSkills.Contains(outcome.skillToGain))
            {
                playerStats.acquiredSkills.Add(outcome.skillToGain);
                Debug.Log($"Skill Gained: {outcome.skillToGain}");
            }
        }
    }

    public void ContinueAfterModal()
    {
        //Update all the UI stats
        uiManager.UpdateAllUI();

        //Decide what to do next
        //if (pendingChainedEvent != null)
        //{
        //    // If the choice leads to a follow-up event, show it
        //    ShowEvent(pendingChainedEvent);
        //}
        //else
        //{
        //    // If there's no follow-up event, advance the game phase
        //    AdvanceGamePhase();
        //}
    }

    public void AdvanceGamePhase()
    {
        // Check the current phase and call the correct advance function
        switch (gameManager.currentPhase)
        {
            case GamePhase.Morning:
                gameManager.AdvanceToAfternoon();
                break;
            case GamePhase.Afternoon:
                gameManager.AdvanceToEvening();
                break;
            case GamePhase.Evening:
 
                Debug.Log("Evening event complete. Awaiting budgeting.");
                break;
        }
    }
}