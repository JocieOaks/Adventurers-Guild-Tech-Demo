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
    public class NavigateRoom : DLite<RoomNode>
    {
        private (float gScore, float rhs, IReference reference)[,] _nodes;
        private Room _room;

        public NavigateRoom(Pawn pawn) : base(pawn)
        {
            _room = pawn.Room;
        }

        protected override (float gScore, float rhs, IReference reference) NodeValues(RoomNode node)
        {
            Vector3Int position = node.RoomPosition;
            return _nodes[position.x, position.y];
        }

        protected override IEnumerable<(RoomNode, float)> Successors(RoomNode node)
        {
            return node.NextNodes;
        }

        protected override void SetRHS(RoomNode node, float value)
        {
            Vector3Int position = node.RoomPosition;
            _nodes[position.x, position.y].rhs = value;
        }

        protected override void SetGScore(RoomNode node, float value)
        {
            Vector3Int position = node.RoomPosition;
            _nodes[position.x, position.y].gScore = value;
        }

        public override bool IsGoalReachable(RoomNode node)
        {
            return base.IsGoalReachable(node) && node.Room == _room;
        }

        protected override void SetElement(RoomNode node, IReference value)
        {
            Vector3Int position = node.RoomPosition;
            _nodes[position.x, position.y].reference = value;
        }

        protected override float Heuristic(RoomNode node)
        {
            return Goal.Heuristic(node);
        }

        public override void SetGoal(IGoal goal)
        {
            _room = Pawn.Room;
            base.SetGoal(goal);
        }

        protected override void InitializeGraph()
        {
            _nodes = new (float, float, IReference)[_room.Width, _room.Length];

            for (int i = 0; i < _room.Width; i++)
            {
                for (int j = 0; j < _room.Length; j++)
                {
                    _nodes[i, j] = (float.PositiveInfinity, float.PositiveInfinity, null);
                }
            }
        }

        protected override IEnumerable<RoomNode> Endpoints()
        {
            return Goal.Endpoints.Where(node => node.Room == _room);
        }

        public override void UpdateStart()
        {
            if (Start != null)
                PriorityAdjustment += Map.Map.EstimateDistance(Start, Pawn.CurrentNode);
            Start = Pawn.CurrentNode;
            EstablishPathing();
        }
    }
}
