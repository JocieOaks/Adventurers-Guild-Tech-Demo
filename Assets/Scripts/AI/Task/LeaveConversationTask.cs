using System.Collections.Generic;
using UnityEngine;

public class LeaveConversationTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => true;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.Conversation = null;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        Debug.Log(actor.Stats.Name + " Leaving Conversation");
        yield return new LeaveConversationAction(actor);
    }

    public override float Time(WorldState worldState)
    {
        return 0.5f;
    }

    public override float Utility(WorldState worldState)
    {
        return -Mathf.Exp( 100 - worldState.Conversation.Duration) - 10;
    }
}
