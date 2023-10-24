using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;

namespace Assets.Scripts.AI.Navigation.Goal
{
    /// <summary>
    /// The <see cref="DestinationGoal"/> class is an <see cref="IGoal"/> for traveling to a specified <see cref="IInteractable"/>.
    /// </summary>
    public class InteractableGoal : IGoal
    {
        private readonly IInteractable _interactable;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationGoal"/> class.
        /// </summary>
        /// <param name="interactable">The interactable trying to be reached.</param>
        public InteractableGoal(IInteractable interactable)
        {
            _interactable = interactable;
        }

        /// <inheritdoc/>
        public IEnumerable<RoomNode> Endpoints => _interactable.InteractionPoints;

        /// <inheritdoc/>
        public float Heuristic(RoomNode start)
        {
            return Map.Map.EstimateDistance(start, _interactable.Node as RoomNode);
        }

        /// <inheritdoc/>
        public bool IsComplete(RoomNode position)
        {
            return _interactable.InteractionPoints.Any(node => node == position);
        }
    }
}
