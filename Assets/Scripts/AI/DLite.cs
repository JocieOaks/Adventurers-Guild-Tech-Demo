using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public class DLite
{
    private Room _room;
    RoomNode _goal;
    RoomNode _start;

    private PriorityQueue<RoomNode, (float,float)> _nodeQueue;

    private (float gScore, float rhs, IReference reference)[,] _nodes;
    private float _priorityAdjustment;

    ///<value>The <see cref="global::Pawn"/> whose path is being evaluated by <see cref="DLite"/>.</value>
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

    /// <summary>
    /// Finds the optimal <see cref="RoomNode"/> to traverse to reach the goal destination.
    /// </summary>
    /// <param name="node">The starting <see cref="RoomNode"/> for the path.</param>
    /// <returns>Returns the next <see cref="RoomNode"/> on the path.</returns>
    public RoomNode GetNext(RoomNode node)
    {
        RoomNode next = null;
        float min = float.PositiveInfinity;
        foreach((RoomNode node, float distance) successor in node.NextNodes)
        {
            float value = successor.distance + this[successor.node].gScore;
            if(value < min)
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
        return (min + Map.EstimateDistance(node, _goal) + _priorityAdjustment, min);
    }

    /// <summary>
    /// The <see cref="PriorityComparer"/> class is an <see cref="IComparer"/> that selects priority based on two float keys, used by the <see cref="DLite"/> class.
    /// </summary>
    private class PriorityComparer : IComparer
    {
        public static PriorityComparer Instance { get; } = new PriorityComparer();

        /// <inheritdoc/>
        public int Compare(object x, object y)
        {
            (float x1, float x2) = ((float, float))x;
            (float y1, float y2) = ((float, float))y;

            if(x1 == x2)
            {
                if(y1 == y2)
                    return 0;
                return y1 < y2 ? 1 : -1;
            }

            return x1 < x2 ? -1 : 1;
        }
    }

    private void Initialize()
    {
        _nodeQueue = new(PriorityComparer.Instance);
        _priorityAdjustment = 0;
        _nodes = new(float, float, IReference)[_room.Width, _room.Length];

        for (int i = 0; i < _room.Width; i++)
        {
            for (int j = 0; j < _room.Length; j++)
            {
                _nodes[i, j] = (float.PositiveInfinity, float.PositiveInfinity, null);
            }
        }
        RoomNode node = _goal.Node as RoomNode;
        SetRHS(node, 0);

        SetElement(node, _nodeQueue.Push(node, CalculatePriority(node)));
    }

    private void UpdateVertex(RoomNode node)
    {
        if (node != _goal.Node)
        {
            float min = float.PositiveInfinity;
            foreach ((RoomNode node, float distance) successor in node.NextNodes)
            {
                min = MathF.Min(min, this[successor.node].gScore + successor.distance);
            }
            SetRHS(node, min);
        }

        IReference reference = this[node].reference;
        if (reference != null)
        {
            _nodeQueue.ChangePriority(reference, CalculatePriority(node));
        }
        else
        {
            SetElement(node, _nodeQueue.Push(node, CalculatePriority(node)));
        }
    }

    private void UpdateGoal(RoomNode newNode)
    {
        SetRHS(newNode, 0);
        UpdateVertex(newNode);
        UpdateVertex(_goal);
        _goal = newNode;
    }

    private void UpdateStart(RoomNode newNode)
    {
        _priorityAdjustment += Map.EstimateDistance(_start, newNode);
        _start = newNode;
    }

    private void EstablishPathing()
    {
        (float gScore, float rhs, IReference _) = this[_start];
        while (PriorityComparer.Instance.Compare(_nodeQueue.TopPriority, CalculatePriority(_start)) == 1 || gScore != rhs)
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
        }
    }

    public static void ConstructPrototype(RoomNode goal, float[,] gScore)
    {

    }
}
