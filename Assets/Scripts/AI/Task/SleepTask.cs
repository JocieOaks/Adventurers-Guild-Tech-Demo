using System.Collections.Generic;
using UnityEngine;

class SleepTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => false;

    protected override bool? _laying => null;

    protected override bool? _conversing => false;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Sleep = 10f;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Sleep " + actor.Stats.Sleep);
        yield return new SleepAction(actor);
    }
    public override float Time(WorldState worldState)
    {
        return Mathf.Max(20, (10 - worldState.PrimaryActor.Sleep) * 5);
    }

    public override float Utility(WorldState worldState)
    {
        if (worldState.PrimaryActor.Stance == Stance.Sit)
            return -5 * Time(worldState);
        return 0;
    }
}
