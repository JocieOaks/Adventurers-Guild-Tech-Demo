using UnityEngine;


/// <summary>
/// A shortlived reflection of an <see cref="Actor"/> used to test hypothetical scenarios with a <see cref="Planner"/>.
/// <see cref="ActorProfile"/> is a mutable struct, so that it can be modified to see the effect of performing <see cref="Task"/>s.
/// </summary>
public struct ActorProfile
{
    float _hunger, _sleep, _social;

    /// <summary>
    /// Initialize a new <see cref="ActorProfile"/> based off of an <see cref="Actor"/>.
    /// </summary>
    /// <param name="actor"></param>
    public ActorProfile(Actor actor)
    {
        Position = actor.Pawn?.CurrentNode.WorldPosition ?? Vector3Int.zero;
        Name = actor.Name;
        Class = actor.Class;
        _hunger = actor.Hunger;
        _sleep = actor.Sleep;
        _social = actor.Social;
        Abilities = new int[] { actor.Strength, actor.Dexterity, actor.Charisma, actor.Intelligence };
        Experience = actor.Experience;
        Level = actor.Level;
        Speed = actor.Pawn?.Speed ?? default;
        Stance = actor.Pawn?.Stance ?? default;
        HasFood = actor.HasFood;
    }
    /// <value>An array containing the <see cref="Actor"/>'s ability scores in the order, Strength, Dexterity, Charisma, Intelligence.</value>
    public int[] Abilities { get; set; }

    /// <value>The <see cref="Actor"/>'s class.</value>
    public Class Class { get; }

    /// <value>The amount of experience the <see cref="Actor"/> has.</value>
    public float Experience { get; set; }

    /// <value>Whether the <see cref="Actor"/> currently possesses food.</value>
    public bool HasFood { get; set; }

    /// <value>The <see cref="Actor"/>'s level of hunger.</value>
    public float Hunger
    {
        get => _hunger;
        set
        {
            _hunger = value > 0 ? value : 0;
        }

    }

    /// <value>The <see cref="Actor"/>'s level.</value>
    public int Level { get; set; }

    /// <value>The <see cref="Actor"/>'s name.</value>
    public string Name { get; }

    /// <value>The map position of the <see cref="Pawn"/> corresponding to the <see cref="Actor"/>.</value>
    public Vector3Int Position { get; set; }

    /// <value>The <see cref="Actor"/>'s level of tiredness and energy.</value>
    public float Sleep
    {
        get => _sleep;
        set
        {
            _sleep = value > 0 ? value : 0;
            _sleep = _sleep < 10 ? _sleep : 10;
        }
    }

    /// <value>The <see cref="Actor"/>'s level of socialization and loneliness.</value>
    public float Social
    {
        get => _social;
        set
        {
            _social = value > 0 ? value : 0;
            _social = _social < 10 ? _social : 10;
        }
    }

    /// <value>The speed of the <see cref="Actor"/>'s corresponding <see cref="Pawn"/>.</value>
    public float Speed { get; }

    /// <value>The <see cref="Stance"/> of the <see cref="Actor"/>'s corresponding <see cref="Pawn"/>.</value>
    public Stance Stance { get; set; }
}
