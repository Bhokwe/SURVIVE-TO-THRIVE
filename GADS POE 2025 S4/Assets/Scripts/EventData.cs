using System.Collections.Generic; 
using UnityEngine;


[CreateAssetMenu(fileName = "New Event", menuName = "Game/New Event")]
public class EventData : ScriptableObject
{
    // The title of the event (e.g., "A Tough Morning")
    public string eventTitle;

    // The main story text for the event, shown to the player.
    [TextArea(3, 10)]
    public string eventDescription;

    // This uses our GamePhase enum 
    public GamePhase eventPhase;

    
    public List<EventChoice> choices;
}