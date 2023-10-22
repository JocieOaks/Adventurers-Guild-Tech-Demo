using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;

namespace Assets.Scripts.AI
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
