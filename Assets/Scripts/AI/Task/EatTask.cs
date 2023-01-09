using System.Collections.Generic;
using UnityEngine;

class EatTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Hunger = 10;
        worldState.PrimaryActor.HasFood = false;
        return worldState;
    }

    public override bool ConditionsMet(WorldState worldState)
    {
        return worldState.PrimaryActor.HasFood;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Eat " + actor.Stats.Hunger);
        yield return new EatAction(actor);
    }

    public override float Time(WorldState worldState)
    {
        return Mathf.Max(5, (10 - worldState.PrimaryActor.Hunger) * 1.5f);
    }

    public override float Utility(WorldState worldState)
    {
        if (worldState.PrimaryActor.Stance == Stance.Stand)
            return -5 * Time(worldState);
        else if (worldState.PrimaryActor.Stance == Stance.Lay)
            return -10 * Time(worldState);
        return 0;
    }
}
