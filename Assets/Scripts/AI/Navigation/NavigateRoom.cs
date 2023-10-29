using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation.Goal;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AI.Navigation
{
    /// <summary>
    /// The <see cref="NavigateRoom"/> class is a child of <see cref="DLite{T}"/> that finds the optimal path within a <see cref="Scripts.Map.Room"/> to reach a <see cref="IDestination"/>.
    /// </summary>
    public class NavigateRoom : DLite<RoomNode>
    {
        /// <value>A 2D array containing the associated value for every node within the current room.</value>
        protected (float gScore, float rhs, IReference reference)[,] Nodes { get; set; }

        /// <value>The current room being navigated through.</value>
        protected Room Room { get; set; }

        private readonly Pawn _pawn;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigateRoom"/> algorithm.
        /// </summary>
        /// <param name="pawn">The <see cref="_pawn"/> who's path is being navigated.</param>
        public NavigateRoom(Pawn pawn)
        {
            Room = pawn.Room;
            _pawn = pawn;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigateRoom"/> algorithm, without a corresponding <see cref="Pawn"/>.
        /// Used to calculate pathing to points of interest within a <see cref="Scripts.Map.Room"/>.
        /// </summary>
        /// <param name="room"></param>
        protected NavigateRoom(Room room)
        {
            Room = room;
        }

        /// <inheritdoc/>
        protected override (float gScore, float rhs, IReference reference) NodeValues(RoomNode node)
        {
            if (_pawn.Room != Room)
            {
                SetGoal(Destination);
            }

            Vector3Int position = node.RoomPosition;
            return Nodes[position.x, position.y];
        }

        /// <inheritdoc/>
        protected override IEnumerable<(RoomNode, float)> Successors(RoomNode node)
        {
            return node.NextNodes;
        }

        /// <inheritdoc/>
        protected override void SetRHS(RoomNode node, float value)
        {
            Vector3Int position = node.RoomPosition;
            Nodes[position.x, position.y].rhs = value;
        }

        /// <inheritdoc/>
        protected override void SetGScore(RoomNode node, float value)
        {
            Vector3Int position = node.RoomPosition;
            Nodes[position.x, position.y].gScore = value;
        }

        /// <inheritdoc />
        protected override RoomNode Start { get; set; }

        /// <inheritdoc/>
        public override bool IsGoalReachable()
        {
            return Destination.EndRooms.Contains(Room) && base.IsGoalReachable();
        }

        /// <inheritdoc/>
        protected override void SetElement(RoomNode node, IReference value)
        {
            Vector3Int position = node.RoomPosition;
            Nodes[position.x, position.y].reference = value;
        }

        /// <inheritdoc/>
        public override void SetGoal(IDestination destination)
        {
            Room = _pawn?.Room ?? Room;
            base.SetGoal(destination);
        }

        /// <inheritdoc/>
        protected override void InitializeGraph()
        {
            Start = _pawn.CurrentNode;

            if (Nodes == null || Nodes.GetLength(0) != Room.Width || Nodes.GetLength(0) != Room.Length)
            {
                Nodes = new (float, float, IReference)[Room.Width, Room.Length];
            }
            for (var i = 0; i < Room.Width; i++)
            {
                for (var j = 0; j < Room.Length; j++)
                {
                    Nodes[i, j] = (float.PositiveInfinity, float.PositiveInfinity, null);
                }
            }
        }

        /// <inheritdoc/>
        protected override IEnumerable<RoomNode> Endpoints()
        {
            return Destination.Endpoints.Where(node => node.Room == Room);
        }

        /// <param name="node"></param>
        /// <inheritdoc/>
        public override void UpdateStart(RoomNode node)
        {
            node ??= _pawn.CurrentNode;

            if (Start != null)
                PriorityAdjustment += Map.Map.EstimateDistance(Start, node);
            Start = node;
            EstablishPathing();
        }
    }
}
