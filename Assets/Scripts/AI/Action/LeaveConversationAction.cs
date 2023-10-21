using Assets.Scripts.AI.Step;
using UnityEngine;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="LeaveConversationAction"/> class is a <see cref="TaskAction"/> for a <see cref="AdventurerPawn"/> to leave a <see cref="Conversation"/>.
    /// </summary>
    public class LeaveConversationAction : ActorAction
    {
        private const float DELAY = 1.0f;
        private float _period;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaveConversationAction"/> class.
        /// </summary>
        /// <param name="actor">The <see cref="Actor"/> leaving the <see cref="Conversation"/>.</param>
        public LeaveConversationAction(Actor actor) : base(actor) { }

        /// <inheritdoc/>
        public override bool CanListen => true;

        /// <inheritdoc/>
        public override bool CanSpeak => false;

        /// <inheritdoc/>
        public override int Complete()
        {
            return _period > DELAY ? 1 : 0;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            Pawn.Social.EndConversation();
            Pawn.CurrentStep = new WaitStep(Pawn, Pawn.CurrentStep, false);
        }

        /// <inheritdoc/>
        public override void Perform()
        {
            _period += Time.deltaTime;
        }
    }
}
