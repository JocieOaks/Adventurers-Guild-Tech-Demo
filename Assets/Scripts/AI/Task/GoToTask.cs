using System.Collections.Generic;
using UnityEngine;

/*
class GoToTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => false;

    Vector3Int _destination;
    SpriteObject _targetObject;
    public GoToTask(Vector3Int destination)
    {
        _destination = destination;
    }

    public GoToTask(SpriteObject target)
    {
        _destination = target.WorldPosition;
        _targetObject = target;
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Position = _destination;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return (new TravelAction(_destination, actor));
    }

    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {
        yield break;
    }

    public override float Time(WorldState worldState)
    {
        return Map.Instance.ApproximateDistance(worldState.PrimaryActor.Position, _destination) / worldState.PrimaryActor.Speed;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }
}
*/