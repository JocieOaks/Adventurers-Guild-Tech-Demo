using UnityEngine;

/// <summary>
/// The <see cref="ApproachAction"/> is a <see cref="TaskAction"/> for a <see cref="Pawn"/> to reposition during a <see cref="Conversation"/> so that it is in a better talking position.
/// </summary>
public class ApproachAction : TaskAction
{
    bool _complete = false;
    Conversation _conversation;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApproachAction"/> class.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> that is repositioning.</param>
    /// <param name="conversation">The <see cref="Conversation"/> that the <see cref="Actor"/> is in.</param>
    public ApproachAction(Actor actor, Conversation conversation) : base(actor)
    {
        _conversation = conversation;
    }

    /// <inheritdoc/>
    public override bool CanListen => true;

    /// <inheritdoc/>
    public override bool CanSpeak => true;

    /// <inheritdoc/>
    public override int Complete()
    {
        if (!_pawn.IsInConversation)
            return -1;
        return _complete ? 1 : 0;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    /// <inheritdoc/>
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
