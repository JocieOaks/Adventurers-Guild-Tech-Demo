using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.AI.Navigation.Destination
{
    public class PawnDestination: IMovingDestination
    {

        private readonly Pawn _pawn;
        private readonly float _radius;

        public PawnDestination(Pawn pawn, float radius)
        {
            _pawn = pawn;
            _radius = radius;
        }


        /// <inheritdoc />
        public IEnumerable<RoomNode> Endpoints
        {
            get { yield return _pawn.CurrentNode; }
        }

        /// <inheritdoc />
        public IEnumerable<Room> EndRooms
        {
            get { yield return _pawn.Room; }
        }

        /// <inheritdoc />
        public float Heuristic(RoomNode start)
        {
            return Map.Map.EstimateDistance(start, _pawn);
        }

        /// <inheritdoc />
        public bool IsComplete(RoomNode position)
        {
            return Map.Map.EstimateDistance(position, _pawn) < _radius;

        }

        /// <inheritdoc />
        public event EventHandler<MovingEventArgs> DestinationMoved;

        private void OnDestinationMoved()
        {
            DestinationMoved?.Invoke(this, new MovingEventArgs(Endpoints.ToList()));
        }
    }
}
