using System.Collections.Generic;

public class RestTask : Task
{
    Task sleep;

    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public RestTask(WorldState worldState)
    {
        sleep = new SleepTask();
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        return sleep.ChangeWorldState(worldState);
    }

    public override bool ConditionsMet(WorldState worldState)
    {
        return sleep.ConditionsMet(worldState);
    }

    public override float Time(WorldState worldState)
    {
        return sleep.Time(worldState);
    }

    public override float Utility(WorldState worldState)
    {
        return sleep.Utility(worldState);
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        foreach (TaskAction action in sleep.GetActions(actor))
            yield return action;
    }
}
