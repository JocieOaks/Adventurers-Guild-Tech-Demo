using System.Collections.Generic;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.AI.Navigation.Goal
{
    /// <summary>
    /// The <see cref="DestinationGoal"/> class is an <see cref="IGoal"/> for traveling to a specified <see cref="RoomNode"/>.
    /// </summary>
    public class DestinationGoal :IGoal
    {
        private readonly RoomNode _destination;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationGoal"/> class.
        /// </summary>
        /// <param name="destination">The destination trying to be reached.</param>
        public DestinationGoal(RoomNode destination)
        {
            _destination = destination;
        }

        /// <inheritdoc/>
        public IEnumerable<RoomNode> Endpoints
        {
            get
            {
                yield return _destination;
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
            return position == _destination;
        }
    }
}
