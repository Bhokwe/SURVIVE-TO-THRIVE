using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class EventChoice
{
    // Description of the choice
    public string choiceDescription;
    [TextArea(3, 5)]
    public string educationalText;

    // Outcomes associated with this choice
    public List<EventOutcome> outcomes;

}

