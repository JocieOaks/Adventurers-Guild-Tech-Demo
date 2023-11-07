using System;
using System.Collections.Generic;
using Assets.Scripts.AI.Navigation.Destination;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.AI.Navigation.Destination
{
    public class MovingEventArgs : EventArgs
    {
        public MovingEventArgs(IEnumerable<RoomNode> previousEndpoints)
        {
            PreviousEndpoints = previousEndpoints;
        }

        public IEnumerable<RoomNode> PreviousEndpoints { get; }

    }

    public interface IMovingDestination : IDestination
    {
        event EventHandler<MovingEventArgs> DestinationMoved;
    }
}
