using System.Collections.Generic;
using UnityEngine;

public class WanderTask : Task
{
    RoomNode node;

    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => false;
    public WanderTask()
    {
        do
        {
            node = Map.Instance[Random.Range(0, Map.Instance.MapWidth), Random.Range(0, Map.Instance.MapLength), 0, Random.Range(0, 2)];
        } while (!node.Traversible);
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Position = node.WorldPosition;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new TravelAction(node.WorldPosition, actor);
    }

    public override float Time(WorldState worldState)
    {
        return 10;
    }

    public override float Utility(WorldState worldState)
    {
        return 10;
    }
}
