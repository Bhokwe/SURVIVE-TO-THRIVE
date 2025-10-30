using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Group 1: Core Stats (The 4 Resources)")]

    public float currentMoney; 


    public int currentHealth;
    public int currentHope;
    public int currentCommunityTrust;

    [Header("Group 2: Progression Flags (Tracking Growth)")]

    // what day the player is on
    public int currentDay;

    //tracking the players major milestones
    public bool hasStableHousing;
    public bool hasNPOContact;

    //skills tracking
    public List<string> acquiredSkills;

    public string currentJob;

    [Header("Group 3: Status Effects (Temporary modifiers)")]

    //tracking of temporary status effects
    public List<string> activeStatusEffects;

    //public void AddMoney(float amount)
    //{
    //    currentMoney += amount;
    //}

}
