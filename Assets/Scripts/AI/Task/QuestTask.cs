using System.Collections.Generic;
using UnityEngine;

public class QuestTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Position = Vector3Int.one;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new TravelAction(Vector3Int.one, actor);
        yield return new QuestingAction(actor);
    }

    public override float Time(WorldState worldState)
    {
        return 0;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }
}