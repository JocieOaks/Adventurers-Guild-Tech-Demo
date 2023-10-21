using System.Collections.Generic;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.AI
{
    public class DestinationGoal :IGoal
    {
        private readonly RoomNode _destination;

        public DestinationGoal(RoomNode destination)
        {
            _destination = destination;
        }

        public IEnumerable<RoomNode> Endpoints
        {
            get
            {
                yield return _destination;
            }
        }

        public float Heuristic(RoomNode start)
        {
            return Map.Map.EstimateDistance(start, _destination);
        }

        public int IsComplete(RoomNode position)
        {
            return position == _destination ? 1 : position.Room != _destination.Room ? -1 : 0;
        }
    }
}
