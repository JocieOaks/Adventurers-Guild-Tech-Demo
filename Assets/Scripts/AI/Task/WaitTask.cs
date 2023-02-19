using System.Collections.Generic;

/// <summary>
/// The <see cref="WaitTask"/> class is a <see cref="Task"/> for a <see cref="Pawn"/> to wait a specified amount of time.
/// </summary>
public class WaitTask : Task
{
    float _time;

    /// <summary>
    /// Initializes a new instace of the <see cref="WaitTask"/> class.
    /// </summary>
    /// <param name="time">The time in seconds that a <see cref="Pawn"/> should wait.</param>
    public WaitTask(float time) : base(null, null, null, null)
    {
        _time = time;
    }

    /// <inheritdoc/>
    public override WorldState ChangeWorldState(WorldState worldState)
    {
        return worldState;
    }

    /// <inheritdoc/>
    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new WaitAction(actor, _time);
    }

    /// <inheritdoc/>
    public override float Time(WorldState worldState)
    {
        return _time;
    }

    /// <inheritdoc/>
    public override float Utility(WorldState worldState)
    {
        return 0;
    }
}
