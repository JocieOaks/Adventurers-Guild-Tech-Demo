using System.Collections.Generic;
using Assets.Scripts.AI.Action;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation.Goal;
using Assets.Scripts.AI.Planning;
using Assets.Scripts.Map;
using UnityEngine;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="QuestTask"/> class is a <see cref="Task"/> for having a <see cref="AdventurerPawn"/> leave the <see cref="Map"/> to go on a <see cref="Quest"/>.
    /// </summary>
    public class QuestTask : Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestTask"/> class.
        /// </summary>
        public QuestTask() : base(null, null, null, null) { }

        /// <inheritdoc />
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            worldState.PrimaryActor.Position = Vector3Int.one;
            return worldState;
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor.Actor actor)
        {
            yield return new TravelAction(new TargetDestination(Map.Map.Instance[Vector3Int.one]), actor.Pawn);
            yield return new QuestingAction(actor);
        }

        /// <inheritdoc/>
        public override float Time(WorldState worldState)
        {
            return 0;
        }

        /// <inheritdoc/>
        public override float Utility(WorldState worldState)
        {
            return 0;
        }
    }
}