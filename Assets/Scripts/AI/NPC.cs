using UnityEngine;

/// <summary>
/// The <see cref="NPC"/> class is a child of the <see cref="AdventurerPawn"/> class where character data is all predetermined in the inspector panel.
/// Used for predefined characters that appear in the game.
/// </summary>
public class NPC : AdventurerPawn
{
    /// <value>The character's name.</value>
    [field: SerializeField] public string CharacterName { get; private set; }

    /// <value>The character's class.</value>
    [field: SerializeField] public Class CharacterClass { get; private set; }

    /// <value>The character's race.</value>
    [field: SerializeField] public Race Race { get; private set; }

    /// <value>The character's strength stat.</value>
    [field: SerializeField] public int Strength { get; private set; }

    /// <value>The character's dexterity stat.</value>
    [field: SerializeField] public int Dexterity { get; private set; }

    /// <value>The character's charisma stat.</value>
    [field: SerializeField] public int Charisma { get; private set; }
    
    /// <value>The character's intelligence stat.</value>
    [field: SerializeField] public int Intelligence { get; private set; }

    /// <summary>
    /// Called when the game object is created.
    /// </summary>
    protected override void Start()
    {
        Actor = new Actor(this);
        base.Start();
    }
}
