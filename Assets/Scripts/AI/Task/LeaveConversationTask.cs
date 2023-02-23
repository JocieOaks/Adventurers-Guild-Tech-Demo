using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The <see cref="LeaveConversationTask"/> class is a <see cref="Task"/> for ending a <see cref="Conversation"/>.
/// </summary>
public class LeaveConversationTask : Task, INestingTask
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LeaveConversationTask"/>.
    /// </summary>
    public LeaveConversationTask() : base(null, null, null, true) { }

    /// <inheritdoc/>
    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.Conversation = null;
        return worldState;
    }

    /// <inheritdoc/>
    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        Debug.Log(actor.Stats.Name + " Leaving Conversation");
        yield return new LeaveConversationAction(actor);
    }

    /// <inheritdoc/>
    public override float Time(WorldState worldState)
    {
        return 0.5f;
    }

    /// <inheritdoc/>
    public override float Utility(WorldState worldState)
    {
        return -Mathf.Exp( 100 - worldState.Conversation.Duration) - 10;
    }
}
