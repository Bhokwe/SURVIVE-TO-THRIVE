using UnityEngine;

[System.Serializable]
public class EventOutcome
{
    public StatToChange statToChange;

    public float amountToChange;

    [Tooltip("The Status Effect to ADD (eg. sick).")]
    public string statusEffectToAdd;

    [Tooltip("The Status Effect to REMOVE (eg. hungry).")]
    public string statusEffectToRemove;

    [Tooltip("The item to ADD to skills.")]
    public string skillToGain;

    [Tooltip("The event to TRIGGER next.")]
    public EventData nextEvent;
}
