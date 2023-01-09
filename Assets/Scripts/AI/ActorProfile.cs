using UnityEngine;

public struct ActorProfile
{
    public Vector3Int Position { get; set; }
    public string Name { get; }
    public Class Class { get; }

    float _hunger, _sleep, _social;
    public float Hunger
    {
        get => _hunger;
        set
        {
            _hunger = value > 0 ? value : 0;
        }

    }
    public float Sleep
    {
        get => _sleep;
        set
        {
            _sleep = value > 0 ? value : 0;
            _sleep = _sleep < 10 ? _sleep : 10;
        }
    }
    public float Social
    {
        get => _social;
        set
        {
            _social = value > 0 ? value : 0;
            _social = _social < 10 ? _social : 10;
        }
    }
    public int[] Abilities { get; set; }
    public float Experience { get; set; }
    public int Level { get; set; }
    public float Speed { get; }
    public Stance Stance { get; set; }
    public bool HasFood { get; set; }

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
}
//Timescale: 1 FrameTick == 10 seconds.
//Time value is measured in frame ticks.
