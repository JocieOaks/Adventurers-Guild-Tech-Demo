using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WanderTask : Task
{
    RoomNode _node;

    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => false;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        Sector sector = Map.Instance[worldState.PrimaryActor.Position].Sector;

        if (_node == null)
        {
            do
            {
                _node = Map.Instance[Random.Range(0, Map.Instance.MapWidth), Random.Range(0, Map.Instance.MapLength), 0, Random.Range(0, 2)];
            } while (!_node.Traversable || _node.Sector != sector);
        }
        worldState.PrimaryActor.Position = _node.WorldPosition;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new TravelAction(_node.WorldPosition, actor);
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
