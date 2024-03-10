using System.Collections.Generic;
using Assets.Scripts.AI.Action;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation.Destination;
using Assets.Scripts.AI.Planning;
using Assets.Scripts.Map.Node;
using UnityEngine;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="WanderTask"/> class is a <see cref="Task"/> for causing a <see cref="AdventurerPawn"/> wandering aimlessly.
    /// </summary>
    public class WanderTask : Task
    {
        private RoomNode _node;

        /// <summary>
        /// Initializes a new instance of the <see cref="WanderTask"/> class.
        /// </summary>
        public WanderTask() : base(null, true, null, false) { }

        /// <inheritdoc/>
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            if (_node == null)
            {
                do
                {
                    _node = Map.Map.Instance[Random.Range(0, Map.Map.Instance.MapWidth), Random.Range(0, Map.Map.Instance.MapLength), 0, Random.Range(0, 2)];
                } while (!_node.Traversable);
            }
            worldState.PrimaryActor.Position = _node.WorldPosition;
            return worldState;
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor.Actor actor)
        {
            //yield return new TravelAction(new PawnDestination(GameManager.Instance.Player, 2), actor.Pawn);
            yield return new TravelAction(new TargetDestination(_node), actor.Pawn);
        }

        /// <inheritdoc/>
        public override float Time(WorldState worldState)
        {
            return 10;
        }

        /// <inheritdoc/>
        public override float Utility(WorldState worldState)
        {
            return 10;
        }
    }
}
