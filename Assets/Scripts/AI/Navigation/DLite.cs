using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Navigation.Destination;
using Assets.Scripts.Map;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.AI.Navigation
{
    /// <summary>
    /// The <see cref="DLite{T}"/> class is an abstract class for performing the D*Lite search algorithm to determine the optimal path to reach a <see cref="IDestination"/>.
    /// </summary>
    /// <typeparam name="T">The type of the nodes that the algorithm is searching through.</typeparam>
    public abstract class DLite<T> where T : IWorldPosition
    {
        private readonly PriorityQueue<T, (float, float)> _nodeQueue = new(PriorityComparer.Instance);

        /// <value>The <see cref="IDestination"/> that is being navigated towards.</value>
        protected IDestination Destination { get; set; }

        /// <value>Adjust value added to priority to account for the position of <see cref="Start"/> changing.</value>
        protected float PriorityAdjustment { get; set; }

        /// <value>The current starting node - either the node the navigating pawn is currently at or the most recent node the pawn was at.</value>
        protected abstract T Start { get; set; }
        /// <summary>
        /// Finds the optimal node to traverse to reach the destination destination.
        /// </summary>
        /// <returns>Returns the next node on the path.</returns>
        public T GetNext()
        {
            T node = Start;
            T next = Start;
            float min = float.PositiveInfinity;
            foreach ((T node, float distance) successor in Successors(node))
            {
                float value = successor.distance + NodeValues(successor.node).gScore;
                if (value < min)
                {
                    min = value;
                    next = successor.node;
                }
            }

            return next;
        }

        /// <summary>
        /// Determines if the current <see cref="IDestination"/> can be reached from the current node.
        /// </summary>
        /// <returns>Returns false if there is no path from the current node to the destination.</returns>
        public virtual bool IsGoalReachable()
        {
            return !float.IsPositiveInfinity(NodeValues(Start).gScore);
        }

        /// <summary>
        /// Gets the gScore for a particular node.
        /// </summary>
        /// <param name="node">The node whose gScore is desired.</param>
        /// <returns>Returns <paramref name="node"/>'s gScore.
        /// Will return <see cref="float.PositiveInfinity"/> if there is no known path to <paramref name="node"/>.</returns>
        public float Score(T node)
        {
            return NodeValues(node).gScore;
        }

        /// <summary>
        /// Sets the current <see cref="IDestination"/> to navigate to.
        /// </summary>
        /// <param name="destination">The new <see cref="IDestination"/></param>
        public virtual void SetGoal(IDestination destination)
        {
            if (Destination is IMovingDestination movingDestination)
            {
                movingDestination.DestinationMoved -= WhenDestinationMoved;
            }

            Destination = destination;

            if (Destination is IMovingDestination newMovingDestination)
            {
                newMovingDestination.DestinationMoved += WhenDestinationMoved;
            }

            Initialize();
        }

        /// <summary>
        /// Updates the current start node.
        /// </summary>
        /// <param name="node"></param>
        public abstract void UpdateStart(T node);

        /// <summary>
        /// Calculates the priority of nodes used by <see cref="PriorityQueue{T1, T2}"/>.
        /// </summary>
        /// <param name="node">The node being evaluated.</param>
        /// <returns>Returns the priority of <paramref name="node"/>.</returns>
        protected virtual (float, float) CalculatePriority(T node)
        {
            (float gScore, float rhs, IReference _) = NodeValues(node);

            float min = Mathf.Min(gScore, rhs);
            return (min + Heuristic(node) + PriorityAdjustment, min);
        }

        /// <summary>
        /// An iterable list of all the valid nodes that will result in completing the <see cref="IDestination"/>.
        /// </summary>
        /// <returns>Returns all the <see cref="IDestination"/>'s endpoints.</returns>
        protected abstract IEnumerable<T> Endpoints();

        /// <summary>
        /// Calculates the path length from nodes to the <see cref="IDestination"/> to determine the optimal path.
        /// </summary>
        protected void EstablishPathing()
        {
            (float gScore, float rhs, IReference _) = NodeValues(Start);
            while (!_nodeQueue.Empty &&
                   (PriorityComparer.Instance.Compare(_nodeQueue.TopPriority, CalculatePriority(Start)) == 1 ||
                    Math.Abs(gScore - rhs) > Utility.Utility.TOLERANCE))
            {


                (float, float) oldPriority = _nodeQueue.TopPriority;
                T node = _nodeQueue.Pop();
                (float, float) newPriority = CalculatePriority(node);
                (float gScore1, float rhs1, IReference _) = NodeValues(node);
                if (PriorityComparer.Instance.Compare(oldPriority, newPriority) == 1)
                {
                    SetElement(node, _nodeQueue.Push(node, newPriority));
                }
                else
                {
                    if (gScore1 > rhs1)
                    {
                        SetGScore(node, rhs1);

                    }
                    else
                    {
                        SetGScore(node, float.PositiveInfinity);
                        UpdateNode(node);
                    }

                    foreach ((T node, float distance) predecessor in Successors(node))
                    {
                        UpdateNode(predecessor.node);
                    }
                }

                (gScore, rhs, _) = NodeValues(Start);
            }
        }

        /// <summary>
        /// Initializes the graph of all nodes that can be traversed through.
        /// </summary>
        protected abstract void InitializeGraph();

        /// <summary>
        /// Gives the gScore, rhs and an <see cref="IReference"/> for the <see cref="PriorityQueue{T1,T2}"/> associated with the given node.
        /// </summary>
        /// <param name="node">The node whose values are being requested.</param>
        /// <returns>Returns the values associated with <paramref name="node"/>.</returns>
        protected abstract (float gScore, float rhs, IReference reference) NodeValues(T node);

        /// <summary>
        /// Sets the reference for a particular node.
        /// </summary>
        /// <param name="node">The node whose value is being set.</param>
        /// <param name="value">The reference associated with <paramref name="node"/>.</param>
        protected abstract void SetElement(T node, IReference value);

        /// <summary>
        /// Sets the gScore for a particular node.
        /// </summary>
        /// <param name="node">The node whose value is being set.</param>
        /// <param name="value">The value the gScore is being set to.</param>
        protected abstract void SetGScore(T node, float value);

        /// <summary>
        /// Sets the rhs value for a particular node.
        /// </summary>
        /// <param name="node">The node whose value is being set.</param>
        /// <param name="value">The value the rhs is being set to.</param>
        protected abstract void SetRHS(T node, float value);

        /// <summary>
        /// Finds all the nodes that can be traversed to directly from the given node.
        /// </summary>
        /// <param name="node">The node whose successors are being evaluated.</param>
        /// <returns>Returns an iterable list of all of <paramref name="node"/>'s successors.</returns>
        protected abstract IEnumerable<(T, float)> Successors(T node);

        /// <summary>
        /// Sets the nodes rhs value, and determines if it matches the node's gScore.
        /// </summary>
        /// <param name="node">The node being updated.</param>
        protected void UpdateNode(T node)
        {
            if (Endpoints().All(x => !x.Equals(node)))
            {
                float min = float.PositiveInfinity;
                foreach ((T node, float distance) successor in Successors(node))
                {
                    min = MathF.Min(min, NodeValues(successor.node).gScore + successor.distance);
                }

                SetRHS(node, min);
            }


            (float gScore, float rhs, IReference reference) = NodeValues(node);
            if (Math.Abs(gScore - rhs) < Utility.Utility.TOLERANCE || (float.IsPositiveInfinity(gScore) && float.IsPositiveInfinity(rhs))) return;
            if (reference != null)
            {
                _nodeQueue.ChangePriority(reference, CalculatePriority(node));
            }
            else
            {
                SetElement(node, _nodeQueue.Push(node, CalculatePriority(node)));
            }
        }

        /// <summary>
        /// An admissible heuristic to estimate the distance from a node to the <see cref="IDestination"/>.
        /// </summary>
        /// <param name="node">The node being evaluated.</param>
        /// <returns>Returns an estimated distance to the <see cref="IDestination"/> that is either less than or equal to the actual distance.</returns>
        private float Heuristic(T node)
        {
            return Map.Map.EstimateDistance(Start, node);
        }

        protected abstract void WhenDestinationMoved(object sender, MovingEventArgs eventArgs);

        /// <summary>
        /// Initializes <see cref="DLite{T}"/> for the current <see cref="IDestination"/>.
        /// </summary>
        private void Initialize()
        {
            _nodeQueue.Clear();
            PriorityAdjustment = 0;

            InitializeGraph();

            InitializeEndpoints();

            UpdateStart(default);
        }

        protected void InitializeEndpoints()
        {
            foreach (T node in Endpoints())
            {
                SetRHS(node, 0);

                SetElement(node, _nodeQueue.Push(node, CalculatePriority(node)));
            }
        }

        /// <summary>
        /// The <see cref="PriorityComparer"/> class is an <see cref="IComparer"/> that selects priority based on two float keys, used by the <see cref="DLite{T}"/> class.
        /// </summary>
        private class PriorityComparer : IComparer
        {
            public static PriorityComparer Instance { get; } = new();

            /// <inheritdoc/>
            public int Compare(object x, object y)
            {
                (float x1, float x2) = ((float, float))x!;
                (float y1, float y2) = ((float, float))y!;

                if (!(Mathf.Abs(x1 - y1) < Utility.Utility.TOLERANCE) && (float.IsFinite(x1) || float.IsFinite(y1))) return x1 < y1 ? 1 : -1;

                if (!(Mathf.Abs(x2 - y2) < Utility.Utility.TOLERANCE) && (float.IsFinite(x2) || float.IsFinite(y2))) return x2 < y2 ? 1 : -1;

                return 0;
            }
        }
    }
}
