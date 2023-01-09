using System.Collections.Generic;

public class StanceStand : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => false;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public override bool ConditionsMet(WorldState worldState)
    {
        return base.ConditionsMet(worldState) && worldState.PreviousTask is not StanceSit && worldState.PreviousTask is not StanceLay;
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Stance = Stance.Stand;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Stand");
        yield return new StandUpAction(actor);
    }

    public override float Time(WorldState worldState)
    {
        return 1;
    }

    public override float Utility(WorldState worldState)
    {
        if (worldState.PrimaryActor.Stance == Stance.Lay)
            return -1 * (10 -worldState.PrimaryActor.Sleep);
        else
            return -0.5f * (10 - worldState.PrimaryActor.Sleep);
    }
}
