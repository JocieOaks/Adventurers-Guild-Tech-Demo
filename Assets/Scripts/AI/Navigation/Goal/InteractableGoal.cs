using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;

namespace Assets.Scripts.AI.Navigation.Goal
{
    public class InteractableGoal : IGoal
    {
        private readonly IInteractable _interactable;
        public InteractableGoal(IInteractable interactable)
        {
            _interactable = interactable;
        }

        public IEnumerable<RoomNode> Endpoints => _interactable.InteractionPoints;
        public float Heuristic(RoomNode start)
        {
            return Map.Map.EstimateDistance(start, _interactable.Node as RoomNode);
        }

        public int IsComplete(RoomNode position)
        {
            if(position.Room != _interactable.Room) return -1;

            return _interactable.InteractionPoints.Any(node => node == position) ? 1 : 0;
        }
    }
}
