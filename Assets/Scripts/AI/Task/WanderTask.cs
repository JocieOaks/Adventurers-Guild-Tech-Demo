using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The <see cref="WanderTask"/> class is a <see cref="Task"/> for causing a <see cref="AdventurerPawn"/> wandering aimlessly.
/// </summary>
public class WanderTask : Task
{
    RoomNode _node;

    /// <summary>
    /// Initializes a new instance of the <see cref="WanderTask"/> class.
    /// </summary>
    public WanderTask() : base(null, true, null, false) { }

    /// <inheritdoc/>
    public override WorldState ChangeWorldState(WorldState worldState)
    {
        if (_node == null)
        {
            do
            {
                _node = Map.Instance[Random.Range(0, Map.Instance.MapWidth), Random.Range(0, Map.Instance.MapLength), 0, Random.Range(0, 2)];
            } while (!_node.Traversable);
        }
        worldState.PrimaryActor.Position = _node.WorldPosition;
        return worldState;
    }

    /// <inheritdoc/>
    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new TravelAction(_node.WorldPosition, actor);
    }

    /// <inheritdoc/>
    public override float Time(WorldState worldState)
    {
        return 10;
    }

    /// <inheritdoc/>
    public override float Utility(WorldState worldState)
    {
        return 10;
    }
}
