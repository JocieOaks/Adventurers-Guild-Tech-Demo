using System.Collections.Generic;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.AI.Navigation.Destination
{
    /// <summary>
    /// The <see cref="TargetDestination"/> class is an <see cref="IDestination"/> for traveling to a specified <see cref="RoomNode"/>.
    /// </summary>
    public class TargetDestination :IDestination
    {
        private readonly INode _destination;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetDestination"/> class.
        /// </summary>
        /// <param name="destination">The destination trying to be reached.</param>
        public TargetDestination(INode destination)
        {
            _destination = destination;
        }

        /// <inheritdoc/>
        public IEnumerable<RoomNode> Endpoints
        {
            get
            {
                yield return _destination.Node;
                if (_destination is ConnectingNode connection)
                {
                    yield return connection.SecondNode;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Room> EndRooms
        {
            get
            {
                if (_destination is ConnectingNode { IsWithinSingleRoom: false } connection)
                {
                    yield return connection.FirstNode.Room;
                    yield return connection.SecondNode.Room;
                }
                else
                    yield return _destination.Room;
            }
        }

        /// <inheritdoc/>
        public float Heuristic(RoomNode start)
        {
            return Map.Map.EstimateDistance(start, _destination);
        }

        /// <inheritdoc/>
        public bool IsComplete(RoomNode position)
        {
            if (_destination is ConnectingNode connection)
            {
                return position == connection.FirstNode || position == connection.SecondNode;
            }
            return position == _destination.Node;
        }
    }
}
