using System.Collections.Generic;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.AI
{
    public interface IGoal
    {
        IEnumerable<RoomNode> Endpoints { get; }

        float Heuristic(RoomNode start);

        int IsComplete(RoomNode position);
    }
}
