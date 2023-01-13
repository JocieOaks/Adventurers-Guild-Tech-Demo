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
        if (!_pawn.IsInConversation)
            return -1;
        return _complete ? 1 : 0;
    }

    public override void Initialize()
    {
    }

    public override void Perform()
    {
        if (_pawn.CurrentStep.IsComplete())
        {
            (float value, Vector3Int position) best = (_conversation.PositionUtility(_pawn.WorldPosition), _pawn.WorldPosition);

            foreach((RoomNode node, float) node in _pawn.CurrentNode.NextNodes)
            {
                if (!node.node.Reserved)
                {
                    float value = _conversation.PositionUtility(node.node.WorldPosition);
                    if (value < best.value)
                        best = (value, node.node.WorldPosition);
                }
            }
            

            if (best.position == _pawn.WorldPosition)
            {
                _complete = true;
                _pawn.CurrentStep = new WaitStep(_pawn, Map.VectorToDir(_conversation.Nexus - best.position), true);
            }
            else
            {
                _pawn.CurrentStep = new WalkStep(best.position, _pawn);
            }
        }
    }
}
