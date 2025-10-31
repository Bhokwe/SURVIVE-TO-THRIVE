using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Button, Slider, Image
using TMPro; // Required for TextMeshPro text (Recommended)
using System.Text; // Required for the StringBuilder

/// <summary>
/// This is System 4: The UI Manager.
/// It manages all UI elements, displaying stats, events, and consequences.
/// It gets its information from other managers and sends player input
/// (like button clicks) back to them.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("System References")]
    // We can get these from the GameManager, or drag them in.
    // Let's get them from the GameManager to keep it clean.
    private PlayerStats playerStats;
    

    [Header("Stat Panel UI")]
    [Tooltip("Text to display the current day")]
    public TextMeshProUGUI dayText;
    [Tooltip("Text to display the current money")]
    public TextMeshProUGUI moneyText;
    [Tooltip("Slider or Text for Health")]
    public TextMeshProUGUI healthText; // You can swap this for a Slider
    [Tooltip("Slider or Text for Hope")]
    public TextMeshProUGUI hopeText;
    [Tooltip("Slider or Text for Community Trust")]
    public TextMeshProUGUI communityTrustText;

    [Header("Status Effect Panel UI")]
    [Tooltip("The parent object (with a Layout Group) to hold icons")]
    public Transform statusEffectContainer;
    [Tooltip("The prefab for a single status effect icon (e.g., a Text or Image)")]
    public GameObject statusEffectIconPrefab;

    [Header("Event Panel UI")]
    [Tooltip("The entire event panel object to show/hide")]
    public GameObject eventPanel;
    [Tooltip("Text for the event's title")]
    public TextMeshProUGUI eventTitleText;
    [Tooltip("Text for the event's main description")]
    public TextMeshProUGUI eventDescriptionText;
    [Tooltip("The parent object (with a Layout Group) to hold the choice buttons")]
    public Transform choiceButtonContainer;
    [Tooltip("The prefab for a single choice button")]
    public GameObject choiceButtonPrefab;

    [Header("Consequence Modal UI")]
    [Tooltip("The entire modal panel object to show/hide")]
    public GameObject consequenceModalPanel;
    [Tooltip("Text to list all the outcomes (e.g., '+10 Health')")]
    public TextMeshProUGUI consequenceDetailsText;
    [Tooltip("Text to show the GDD's 'educationalText'")]
    public TextMeshProUGUI educationalTextDisplay;
    [Tooltip("The 'Continue' button on the modal")]
    public Button consequenceContinueButton;


    void Start()
    {
        // Get references from the Singleton
        playerStats = GameManager.Instance.playerStats;
       

        // Make sure the UI starts in a clean state
        eventPanel.SetActive(false);
        consequenceModalPanel.SetActive(false);

        // Wire up the 'Continue' button on the modal
        // This button will call OnConsequenceContinueClicked when pressed
        consequenceContinueButton.onClick.AddListener(OnConsequenceContinueClicked);

        // Update all UI elements with starting values
        UpdateAllUI();
    }

    /// <summary>
    /// A single function to refresh all UI elements.
    /// The GameManager can call this at the start of a new day.
    /// </summary>
    public void UpdateAllUI()
    {
        UpdateStatUI();
        UpdateStatusEffectsUI();
    }

    /// <summary>
    /// GDD Function: Updates the text/sliders for all Core Stats.
    /// </summary>
    public void UpdateStatUI()
    {
        if (playerStats == null) return; // Safety check

        dayText.text = $"Day: {playerStats.currentDay}";
        // "F2" formats the float as currency (e.g., 20.50)
        moneyText.text = $"R {playerStats.currentMoney:F2}";
        healthText.text = $"Health: {playerStats.currentHealth}/100";
        hopeText.text = $"Hope: {playerStats.currentHope}/100";
        communityTrustText.text = $"Trust: {playerStats.currentCommunityTrust}/100";
    }

    /// <summary>
    /// GDD Function: Shows icons for all activeStatusEffects.
    /// </summary>
    public void UpdateStatusEffectsUI()
    {
        // 1. Clear all old status icons
        foreach (Transform child in statusEffectContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Add new icons for each active effect
        // This is simple, but not optimized (object pooling is better)
        // For a beginner, Instantiate/Destroy is easiest to understand.
        foreach (string effect in playerStats.activeStatusEffects)
        {
            GameObject icon = Instantiate(statusEffectIconPrefab, statusEffectContainer);
            // Assuming the prefab has a TextMeshPro component to set
            TextMeshProUGUI iconText = icon.GetComponentInChildren<TextMeshProUGUI>();
            if (iconText != null)
            {
                iconText.text = effect;
            }
        }
    }

    /// <summary>
    /// GDD Function: Populates the UI with the event's data
    /// and dynamically creates buttons for each choice.
    /// </summary>
    /// <param name="eventData">The event to display</param>
    public void DisplayEvent(EventData eventData)
    {
        // 1. Turn on the panel
        eventPanel.SetActive(true);
        consequenceModalPanel.SetActive(false); // Just in case

        // 2. Set the text
        eventTitleText.text = eventData.eventTitle;
        eventDescriptionText.text = eventData.eventDescription;

        // 3. Clear any old choice buttons
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 4. Create new buttons for each choice
        for (int i = 0; i < eventData.choices.Count; i++)
        {
            // This is a common "gotcha" in loops. We copy 'i' to a local
            // variable 'choiceIndex' so the button's click listener
            // captures the correct number.
            int choiceIndex = i;

            // Create the button from our prefab
            GameObject buttonGO = Instantiate(choiceButtonPrefab, choiceButtonContainer);

            // Set the button's text
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = eventData.choices[i].choiceDescription;

            // Add the click listener
            Button button = buttonGO.GetComponent<Button>();
            button.onClick.AddListener(() => { OnChoiceButtonClicked(choiceIndex); });
        }
    }

    /// <summary>
    /// A helper function called by the dynamic choice buttons.
    /// </summary>
    /// <param name="index">The index of the choice that was clicked.</param>
    private void OnChoiceButtonClicked(int index)
    {
        // 1. Hide the event panel
        EventData currentEvent = GameManager.Instance.currentEvent;
        EventChoice chosenChoice = currentEvent.choices[index];

        // --- FIX: CALL THE CORRECT MANAGER ---
        GameManager.Instance.MakeChoice(chosenChoice);
    }


    /// <summary>
    /// GDD Function: Shows the modal with outcomes and educational text.
    /// </summary>
    public void ShowConsequenceModal(List<EventOutcome> outcomes, string educationalText)
    {
        // 1. Show the modal
        consequenceModalPanel.SetActive(true);

        // 2. Build the outcome description string
        // Using a StringBuilder is much more efficient than (string + string)
        StringBuilder sb = new StringBuilder();
        foreach (EventOutcome outcome in outcomes)
        { 
            // Show stat changes
            if (outcome.amountToChange != 0)
            {
            string sign = (outcome.amountToChange > 0) ? "+" : "";
            sb.AppendLine($"{outcome.statToChange}: {sign}{outcome.amountToChange}");
            }


            // Show status effect changes
            if (!string.IsNullOrEmpty(outcome.statusEffectToAdd))
            {
                sb.AppendLine($"Gained: {outcome.statusEffectToAdd}");
            }
            if (!string.IsNullOrEmpty(outcome.statusEffectToRemove))
            {
                sb.AppendLine($"Lost: {outcome.statusEffectToRemove}");
            }
            // Show skills gained
            if (!string.IsNullOrEmpty(outcome.skillToGain))
            {
                sb.AppendLine($"Skill Gained: {outcome.skillToGain}");
            }
        }
            // If no outcomes, just say "You continue..."
            if (sb.Length == 0)
            {
                sb.AppendLine("You reflect on your choice.");
            }
            consequenceDetailsText.text = sb.ToString();

            // 3. Set the educational text
            educationalTextDisplay.text = educationalText;

            // 4. (Optional) Hide educational text if it's empty
            educationalTextDisplay.gameObject.SetActive(!string.IsNullOrEmpty(educationalText));
     }

    /// <summary>
    /// This is called by the 'consequenceContinueButton'
    /// </summary>
    private void OnConsequenceContinueClicked()
    {
    // 1. Hide this modal
    consequenceModalPanel.SetActive(false);

    // 2. Tell the EventManager it's safe to proceed
    eventManager.ContinueAfterModal();
    }
}