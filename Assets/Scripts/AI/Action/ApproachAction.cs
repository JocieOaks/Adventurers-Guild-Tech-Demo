using UnityEngine;

public class ApproachAction : TaskAction
{
    public override bool CanSpeak => true;

    public override bool CanListen => true;

    Conversation _conversation;

    bool _complete = false;

    public ApproachAction(Actor actor, Conversation conversation) : base(actor) 
    {
        _conversation = conversation;
    }

    public override int Complete()
    {
        if (!_actor.Pawn.IsInConversation)
            return -1;
        return _complete ? 1 : 0;
    }

    public override void Initialize()
    {
    }

    public override void Perform()
    {
        Pawn pawn = _actor.Pawn;

        if (pawn.CurrentStep.IsComplete)
        {
            (float value, Vector3Int position) best = (_conversation.PositionUtility(pawn.WorldPosition), pawn.WorldPosition);

            foreach((RoomNode node, float distance) node in pawn.CurrentNode.NextNodes)
            {
                float value = _conversation.PositionUtility(node.node.WorldPosition);
                if (value < best.value)
                    best = (value, node.node.WorldPosition);
            }
            

            if (best.position == pawn.WorldPosition)
            {
                _complete = true;
                pawn.CurrentStep = new WaitStep(_actor.Pawn, Map.VectorToDir(_conversation.Nexus - best.position));
            }
            else
            {
                pawn.CurrentStep = new WalkStep(best.position, pawn);
            }
        }
    }
}
