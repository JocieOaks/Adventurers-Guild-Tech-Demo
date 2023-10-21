using Assets.Scripts.AI.Task;
using UnityEngine;

namespace Assets.Scripts.AI
{
    /// <summary>
    /// The <see cref="WorldState"/> struct is a reflection of the current state of the game for use by <see cref="Planner"/>, or a prediction of a future state of the game.
    /// <see cref="WorldState"/> is mutable because it is intended to hold a temporary prediction from a series of changes from performing a <see cref="Task"/>.
    /// </summary>
    public struct WorldState
    {
        public Conversation Conversation;
        public Task.Task PreviousTask;
        public ActorProfile PrimaryActor;
        private float? _distance;

        /// <summary>
        /// Initializes a new instance of <see cref="WorldState"/>.
        /// </summary>
        /// <param name="actor">The primary <see cref="Actor"/> whose <see cref="Task"/>s the <see cref="WorldState"/> is intended to reflect.</param>
        public WorldState(Actor actor)
        {
            PrimaryActor = actor.Stats;
            Conversation = actor.Pawn.Social.Conversation;
            _distance = null;
            PreviousTask = null;
        }

        /// <value>Gives the distance of the <see cref="Actor"/> from the <see cref="Conversation.Nexus"/> if it is in a <see cref="AI.Conversation"/>.</value>
        public float ConversationDistance
        {
            set => _distance = value;
            get
            {
                if (_distance == null)
                {
                    if (Conversation != null)
                        _distance = Vector3.Distance(Conversation.Nexus, PrimaryActor.Position);
                    else
                        return 0;
                }
                return _distance.Value;
            }
        }
    }
}
