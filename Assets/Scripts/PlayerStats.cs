using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private int _playerLevel;
    private float _playerXp;

    // We can change the names of these later, depending on what we want
    private int _playerStrength;
    private int _playerDefense;
    private int _playerIntelligence;
    
    public event EventHandler OnXpGained;

    // TODO: invoke xpgained when colliding with xp 'orb'
    // let player choose between str, def, int on level up, then update stored stats and modify
    // gameplay accordingly
}
