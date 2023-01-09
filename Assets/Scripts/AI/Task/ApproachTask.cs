using System.Collections.Generic;
using UnityEngine;

public class ApproachTask : Task
{

    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => true;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.ConversationDistance = 2;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        Debug.Log(actor.Stats.Name + " Approach");
        yield return new ApproachAction(actor, actor.Pawn.Social.Conversation);
    }

    public override float Time(WorldState worldState)
    {
        return Mathf.Abs(worldState.ConversationDistance - 2) * 2 / worldState.PrimaryActor.Speed;
    }

    public override float Utility(WorldState worldState)
    {
        return -5;
    }
}
