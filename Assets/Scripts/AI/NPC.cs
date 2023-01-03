using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Pawn
{
    [SerializeField]
    string characterName;
    [SerializeField]
    Class characterClass;
    [SerializeField]
    Race race;
    [SerializeField]
    int strength, dexterity, charisma, intelligence;
    protected override void Start()
    {
        Actor = new Actor(this, characterClass, race, name, strength, dexterity, charisma, intelligence);
        base.Start();
    }
}
