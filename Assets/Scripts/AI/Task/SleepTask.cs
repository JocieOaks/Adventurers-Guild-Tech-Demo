using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The <see cref="SleepTask"/> class is a <see cref="Task"/> for a <see cref="AdventurerPawn"/> to sleep.
/// </summary>
class SleepTask : Task
{
    /// <summary>
    /// Initializes a new instance of <see cref="SleepTask"/>.
    /// </summary>
    public SleepTask() : base(null, false, null, false) { }

    /// <inheritdoc/>
    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Sleep = 10f;
        return worldState;
    }

    /// <inheritdoc/>
    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Sleep " + actor.Stats.Sleep);
        yield return new SleepAction(actor);
    }

    /// <inheritdoc/>
    public override float Time(WorldState worldState)
    {
        return Mathf.Max(20, (10 - worldState.PrimaryActor.Sleep) * 5);
    }

    /// <inheritdoc/>
    public override float Utility(WorldState worldState)
    {
        if (worldState.PrimaryActor.Stance == Stance.Sit)
            return -5 * Time(worldState);
        return 0;
    }
}
