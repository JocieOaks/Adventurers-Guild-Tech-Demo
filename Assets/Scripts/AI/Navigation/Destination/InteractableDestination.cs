using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;

namespace Assets.Scripts.AI.Navigation.Destination
{
    /// <summary>
    /// The <see cref="TargetDestination"/> class is an <see cref="IDestination"/> for traveling to a specified <see cref="IInteractable"/>.
    /// </summary>
    public class InteractableDestination : IDestination
    {
        private readonly IInteractable _interactable;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetDestination"/> class.
        /// </summary>
        /// <param name="interactable">The interactable trying to be reached.</param>
        public InteractableDestination(IInteractable interactable)
        {
            _interactable = interactable;
        }

        /// <inheritdoc/>
        public IEnumerable<RoomNode> Endpoints => _interactable.InteractionPoints;

        /// <inheritdoc/>
        public IEnumerable<Room> EndRooms
        {
            get
            {
                yield return _interactable.Room;
            }
        }

        /// <inheritdoc/>
        public float Heuristic(RoomNode start)
        {
            return Map.Map.EstimateDistance(start, _interactable.Node);
        }

        /// <inheritdoc/>
        public bool IsComplete(RoomNode position)
        {
            return _interactable.InteractionPoints.Any(node => node == position);
        }
    }
}
