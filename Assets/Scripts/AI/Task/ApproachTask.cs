using System.Collections.Generic;
using Assets.Scripts.AI.Action;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Planning;
using Assets.Scripts.AI.Social;
using UnityEngine;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="ApproachTask"/> class is a <see cref="Task"/> for having a <see cref="AdventurerPawn"/> adjust their position when having a <see cref="Conversation"/>.
    /// </summary>
    public class ApproachTask : Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApproachTask"/> class.
        /// </summary>
        public ApproachTask() : base (null, true, null, true) { }

        /// <inheritdoc/>
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            worldState.ConversationDistance = 2;
            return worldState;
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor.Actor actor)
        {
            Debug.Log(actor.Stats.Name + " Approach");
            yield return new ApproachAction(actor, actor.Pawn.Social.Conversation);
        }

        /// <inheritdoc/>
        public override float Time(WorldState worldState)
        {
            return Mathf.Abs(worldState.ConversationDistance - 2) * 2 / worldState.PrimaryActor.Speed;
        }

        /// <inheritdoc/>
        public override float Utility(WorldState worldState)
        {
            return -5;
        }
    }
}
