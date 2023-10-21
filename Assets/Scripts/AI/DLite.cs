using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Utility;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.AI
{
    public class DLite
    {
        private Room _room;
        private IGoal _goal;
        private RoomNode _start;

        private PriorityQueue<RoomNode, (float, float)> _nodeQueue;

        private (float gScore, float rhs, IReference reference)[,] _nodes;
        private float _priorityAdjustment;

        public DLite(Pawn pawn)
        {
            Pawn = pawn;
            _room = pawn.Room;
        }

        ///<value>The <see cref="Assets.Scripts.AI.Pawn"/> whose path is being evaluated by <see cref="DLite"/>.</value>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private Pawn Pawn { get; }

        /// <summary>
        /// Indexer that returns the gScore, rhs and reference associated with a particular <see cref="RoomNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="RoomNode"/> being indexed.</param>
        /// <returns>Returns the <see cref="RoomNode"/>'s gScore, rhs and reference.</returns>
        private (float gScore, float rhs, IReference reference) this[RoomNode node]
        {
            get
            {
                Vector3Int position = node.RoomPosition;
                return _nodes[position.x, position.y];
            }
        }

        public bool IsGoalReachable(RoomNode node)
        {
            return !float.IsPositiveInfinity(this[node].gScore);
        }

        /// <summary>
        /// Finds the optimal <see cref="RoomNode"/> to traverse to reach the goal destination.
        /// </summary>
        /// <param name="node">The starting <see cref="RoomNode"/> for the path.</param>
        /// <returns>Returns the next <see cref="RoomNode"/> on the path.</returns>
        // ReSharper disable once UnusedMember.Global
        public RoomNode GetNext(RoomNode node)
        {
            RoomNode next = null;
            float min = float.PositiveInfinity;
            foreach ((RoomNode node, float distance) successor in node.NextNodes)
            {
                float value = successor.distance + this[successor.node].gScore;
                if (value < min)
                {
                    min = value;
                    next = successor.node;
                }
            }

            return next;
        }

        /// <summary>
        /// Sets the rhs value for a particular <see cref="RoomNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="RoomNode"/> whose value is being set.</param>
        /// <param name="value">The value the rhs is being set to.</param>
        private void SetRHS(RoomNode node, float value)
        {
            Vector3Int position = node.RoomPosition;
            _nodes[position.x, position.y].rhs = value;
        }

        /// <summary>
        /// Sets the gScore for a particular <see cref="RoomNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="RoomNode"/> whose value is being set.</param>
        /// <param name="value">The value the gScore is being set to.</param>
        private void SetGScore(RoomNode node, float value)
        {
            Vector3Int position = node.RoomPosition;
            _nodes[position.x, position.y].gScore = value;
        }

        /// <summary>
        /// Sets the reference for a particular <see cref="RoomNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="RoomNode"/> whose value is being set.</param>
        /// <param name="value">The reference associated with <paramref name="node"/>.</param>
        private void SetElement(RoomNode node, IReference value)
        {
            Vector3Int position = node.RoomPosition;
            _nodes[position.x, position.y].reference = value;
        }



        /// <summary>
        /// Calculates the priority of <see cref="RoomNode"/>s used by <see cref="PriorityQueue{T1, T2}"/>.
        /// </summary>
        /// <param name="node">The <see cref="RoomNode"/>.</param>
        /// <returns>Returns the priority of <paramref name="node"/>.</returns>
        private (float, float) CalculatePriority(RoomNode node)
        {
            (float gScore, float rhs, IReference _) = this[node];

            float min = Mathf.Min(gScore, rhs);
            return (min + _goal.Heuristic(node) + _priorityAdjustment, min);
        }

        /// <summary>
        /// The <see cref="PriorityComparer"/> class is an <see cref="IComparer"/> that selects priority based on two float keys, used by the <see cref="DLite"/> class.
        /// </summary>
        private class PriorityComparer : IComparer
        {
            public static PriorityComparer Instance { get; } = new();

            /// <inheritdoc/>
            public int Compare(object x, object y)
            {
                (float x1, float x2) = ((float, float))x!;
                (float y1, float y2) = ((float, float))y!;

                if (Math.Abs(x1 - y1) < Utility.Utility.TOLERANCE)
                {
                    if (Math.Abs(x2 - y2) < Utility.Utility.TOLERANCE)
                        return 0;
                    return x2 < y2 ? 1 : -1;
                }

                return x1 < y1 ? 1 : -1;
            }
        }

        public void SetGoal(IGoal goal)
        {
            _goal = goal;
            Initialize();
        }

        /// <summary>
        /// Initializes <see cref="DLite"/> for the current <see cref="IGoal"/>.
        /// </summary>
        private void Initialize()
        {
            _nodeQueue = new(PriorityComparer.Instance);
            _priorityAdjustment = 0;
            _nodes = new (float, float, IReference)[_room.Width, _room.Length];

            for (int i = 0; i < _room.Width; i++)
            {
                for (int j = 0; j < _room.Length; j++)
                {
                    _nodes[i, j] = (float.PositiveInfinity, float.PositiveInfinity, null);
                }
            }

            foreach (RoomNode node in _goal.Endpoints)
            {
                SetRHS(node, 0);

                SetElement(node, _nodeQueue.Push(node, CalculatePriority(node)));
            }

            _start = Pawn.CurrentNode;
        }

        private void UpdateVertex(RoomNode node)
        {
            if (_goal.Endpoints.All(x => x != node))
            {
                float min = float.PositiveInfinity;
                foreach ((RoomNode node, float distance) successor in node.NextNodes)
                {
                    min = MathF.Min(min, this[successor.node].gScore + successor.distance);
                }

                SetRHS(node, min);
            }


            (float gScore, float rhs, IReference reference) = this[node];
            if(Math.Abs(gScore - rhs) < Utility.Utility.TOLERANCE) return;
            if (reference != null)
            {
                _nodeQueue.ChangePriority(reference, CalculatePriority(node));
            }
            else
            {
                SetElement(node, _nodeQueue.Push(node, CalculatePriority(node)));
            }
        }

        // ReSharper disable once UnusedMember.Local
        /*private void UpdateGoal(RoomNode newNode)
    {
        SetRHS(newNode, 0);
        UpdateVertex(newNode);
        UpdateVertex(_goal);
        _goal = newNode;
        }*/

        // ReSharper disable once UnusedMember.Local
        private void UpdateStart(RoomNode newNode)
        {
            _start = newNode;
            if (newNode.Room == _room)
            {
                _priorityAdjustment += Map.Map.EstimateDistance(_start, newNode);
            }
            else
            {
                _room = newNode.Room;
                Initialize();
            }
        }

        [UsedImplicitly]
        public void EstablishPathing()
        {
            (float gScore, float rhs, IReference _) = this[_start];
            while (_nodeQueue.Count > 0 &&
                   (PriorityComparer.Instance.Compare(_nodeQueue.TopPriority, CalculatePriority(_start)) == 1 ||
                    Math.Abs(gScore - rhs) > Utility.Utility.TOLERANCE))
            {
                (float, float) oldPriority = _nodeQueue.TopPriority;
                RoomNode node = _nodeQueue.Pop();
                (float, float) newPriority = CalculatePriority(node);
                (gScore, rhs, _) = this[node];
                if (PriorityComparer.Instance.Compare(oldPriority, newPriority) == 1)
                {
                    SetElement(node, _nodeQueue.Push(node, newPriority));
                }
                else
                {
                    if (gScore > rhs)
                    {
                        SetGScore(node, rhs);

                    }
                    else
                    {
                        SetGScore(node, float.PositiveInfinity);
                        UpdateVertex(node);
                    }

                    foreach ((RoomNode node, float distance) predecessor in node.NextNodes)
                    {
                        UpdateVertex(predecessor.node);
                    }
                }

                (gScore, rhs, _) = this[_start];
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static void ConstructPrototype(RoomNode goal, out float[,] gScore)
        {
            gScore = null;
        }
    }
}
