using System.Collections.Generic;

public class WaitTask : Task
{
    float _time;

    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public WaitTask(float time)
    {
        _time = time;
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new WaitAction(actor, _time);
    }

    public override float Time(WorldState worldState)
    {
        return _time;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }
}
