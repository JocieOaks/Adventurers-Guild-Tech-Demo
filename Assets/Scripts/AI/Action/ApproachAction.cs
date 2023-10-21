using Assets.Scripts.AI.Step;
using Assets.Scripts.Map.Node;
using UnityEngine;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="ApproachAction"/> is a <see cref="TaskAction"/> for a <see cref="AdventurerPawn"/> to reposition during a <see cref="Conversation"/> so that it is in a better talking position.
    /// </summary>
    public class ApproachAction : ActorAction
    {
        private bool _complete;
        private readonly Conversation _conversation;

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
            if (!Pawn.IsInConversation)
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
            if (Pawn.CurrentStep.IsComplete())
            {
                (float value, Vector3Int position) best = (_conversation.PositionUtility(Pawn.WorldPosition), Pawn.WorldPosition);

                foreach((RoomNode node, float) node in Pawn.CurrentNode.NextNodes)
                {
                    if (!node.node.Reserved)
                    {
                        float value = _conversation.PositionUtility(node.node.WorldPosition);
                        if (value < best.value)
                            best = (value, node.node.WorldPosition);
                    }
                }
            

                if (best.position == Pawn.WorldPosition)
                {
                    _complete = true;
                    Pawn.CurrentStep = new WaitStep(Pawn, Utility.Utility.VectorToDirection(_conversation.Nexus - best.position), true);
                }
                else
                {
                    Pawn.CurrentStep = new WalkStep(best.position, Pawn);
                }
            }
        }
    }
}
