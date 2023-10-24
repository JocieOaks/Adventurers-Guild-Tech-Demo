using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation.Goal;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.AI.Navigation
{
    public abstract class DLite<T>
    {
        protected IGoal Goal;
        protected T Start;

        private PriorityQueue<T, (float, float)> _nodeQueue;

       
        protected float PriorityAdjustment;

        protected DLite(Pawn pawn)
        {
            Pawn = pawn;
        }

        protected abstract (float gScore, float rhs, IReference reference) NodeValues(T node);

        protected abstract IEnumerable<(T, float)> Successors(T node);

        protected Pawn Pawn;

        public virtual bool IsGoalReachable(T node)
        {
            return !float.IsPositiveInfinity(NodeValues(node).gScore);
        }

        /// <summary>
        /// Finds the optimal <see cref="T"/> to traverse to reach the goal destination.
        /// </summary>
        /// <param name="node">The starting <see cref="T"/> for the path.</param>
        /// <returns>Returns the next <see cref="T"/> on the path.</returns>
        // ReSharper disable once UnusedMember.Global
        public T GetNext(T node)
        {
            T next = default;
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
        /// Sets the rhs value for a particular <see cref="T"/>.
        /// </summary>
        /// <param name="node">The <see cref="T"/> whose value is being set.</param>
        /// <param name="value">The value the rhs is being set to.</param>
        protected abstract void SetRHS(T node, float value);

        /// <summary>
        /// Sets the gScore for a particular <see cref="T"/>.
        /// </summary>
        /// <param name="node">The <see cref="T"/> whose value is being set.</param>
        /// <param name="value">The value the gScore is being set to.</param>
        protected abstract void SetGScore(T node, float value);

        /// <summary>
        /// Sets the reference for a particular <see cref="T"/>.
        /// </summary>
        /// <param name="node">The <see cref="T"/> whose value is being set.</param>
        /// <param name="value">The reference associated with <paramref name="node"/>.</param>
        protected abstract void SetElement(T node, IReference value);

        protected abstract float Heuristic(T node);

        /// <summary>
        /// Calculates the priority of <see cref="T"/>s used by <see cref="PriorityQueue{T1, T2}"/>.
        /// </summary>
        /// <param name="node">The <see cref="T"/>.</param>
        /// <returns>Returns the priority of <paramref name="node"/>.</returns>
        private (float, float) CalculatePriority(T node)
        {
            (float gScore, float rhs, IReference _) = NodeValues(node);

            float min = Mathf.Min(gScore, rhs);
            return (min + Heuristic(node) + PriorityAdjustment, min);
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

                if (Math.Abs(x1 - y1) < Utility.Utility.TOLERANCE)
                {
                    if (Math.Abs(x2 - y2) < Utility.Utility.TOLERANCE)
                        return 0;
                    return x2 < y2 ? 1 : -1;
                }

                return x1 < y1 ? 1 : -1;
            }
        }

        public virtual void SetGoal(IGoal goal)
        {
            Goal = goal;
            Initialize();
        }

        protected abstract void InitializeGraph();

        protected abstract IEnumerable<T> Endpoints(IGoal goal);

        /// <summary>
        /// Initializes <see cref="DLite{T}"/> for the current <see cref="IGoal"/>.
        /// </summary>
        private void Initialize()
        {
            _nodeQueue = new(PriorityComparer.Instance);
            PriorityAdjustment = 0;

            InitializeGraph();

            foreach (T node in Endpoints(Goal))
            {
                SetRHS(node, 0);

                SetElement(node, _nodeQueue.Push(node, CalculatePriority(node)));
            }

            UpdateStart();
        }

        private void UpdateVertex(T node)
        {
            if (Endpoints(Goal).All(x => !x.Equals(node)))
            {
                float min = float.PositiveInfinity;
                foreach ((T node, float distance) successor in Successors(node))
                {
                    min = MathF.Min(min, NodeValues(successor.node).gScore + successor.distance);
                }

                SetRHS(node, min);
            }


            (float gScore, float rhs, IReference reference) = NodeValues(node);
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
        /*private void UpdateGoal(T newNode)
        {
        SetRHS(newNode, 0);
        UpdateVertex(newNode);
        UpdateVertex(_goal);
        _goal = newNode;
        }*/

        // ReSharper disable once UnusedMember.Local
        public abstract void UpdateStart();

        protected void EstablishPathing()
        {
            (float gScore, float rhs, IReference _) = NodeValues(Start);
            while (_nodeQueue.Count > 0 &&
                   (PriorityComparer.Instance.Compare(_nodeQueue.TopPriority, CalculatePriority(Start)) == 1 ||
                    Math.Abs(gScore - rhs) > Utility.Utility.TOLERANCE))
            {
                (float, float) oldPriority = _nodeQueue.TopPriority;
                T node = _nodeQueue.Pop();
                (float, float) newPriority = CalculatePriority(node);
                (gScore, rhs, _) = NodeValues(node);
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

                    foreach ((T node, float distance) predecessor in Successors(node))
                    {
                        UpdateVertex(predecessor.node);
                    }
                }

                (gScore, rhs, _) = NodeValues(Start);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static void ConstructPrototype(T goal, out float[,] gScore)
        {
            gScore = null;
        }
    }
}
