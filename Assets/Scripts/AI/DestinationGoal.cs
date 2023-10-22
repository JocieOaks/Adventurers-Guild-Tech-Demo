using System.Collections.Generic;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.AI
{
    public class DestinationGoal :IGoal
    {
        public RoomNode Destination { get; }

        public DestinationGoal(RoomNode destination)
        {
            Destination = destination;
        }

        public IEnumerable<RoomNode> Endpoints
        {
            get
            {
                yield return Destination;
            }
        }

        public float Heuristic(RoomNode start)
        {
            return Map.Map.EstimateDistance(start, Destination);
        }

        public int IsComplete(RoomNode position)
        {
            return position == Destination ? 1 : position.Room != Destination.Room ? -1 : 0;
        }
    }
}
