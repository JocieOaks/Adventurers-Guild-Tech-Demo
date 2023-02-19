using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// The <see cref="QuestTask"/> class is a <see cref="Task"/> for having a <see cref="Pawn"/> leave the <see cref="Map"/> to go on a <see cref="Quest"/>.
/// </summary>
public class QuestTask : Task
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuestTask"/> class.
    /// </summary>
    public QuestTask() : base(null, null, null, null) { }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Position = Vector3Int.one;
        return worldState;
    }

    /// <inheritdoc/>
    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new TravelAction(Vector3Int.one, actor);
        yield return new QuestingAction(actor);
    }

    /// <inheritdoc/>
    public override float Time(WorldState worldState)
    {
        return 0;
    }

    /// <inheritdoc/>
    public override float Utility(WorldState worldState)
    {
        return 0;
    }
}