using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// The <see cref="NavigateJob"/> struct is an <see cref="IJob"/> that finds the best path for a <see cref="Pawn"/> 
/// to travel to get to a specified destination, without blocking the main thread.
/// </summary>
public struct NavigateJob : IJob
{

    readonly Vector3Int _end;
    readonly Vector3Int _start;
    NativeArray<(bool isDoor, Vector3Int)> _walkingPath;

    /// <summary>
    /// Initializes a new <see cref="NavigateJob"/> struct.
    /// </summary>
    /// <param name="startPosition">The starting position of the <see cref="Pawn"/>.</param>
    /// <param name="endPosition">The destination of the <see cref="Pawn"/>.</param>
    /// <param name="walkingPath">The path from <c>startPosition</c> to <c>walkingPath</c> as a list of <see cref="Map"/> coordinates.</param>
    public NavigateJob(Vector3Int startPosition, Vector3Int endPosition, NativeArray<(bool isDoor, Vector3Int)> walkingPath)
    {
        _start = startPosition;
        _end = endPosition;
        _walkingPath = walkingPath;
    }

    /// <inheritdoc/>
    public void Execute()
    {
        RoomNode endNode = Map.Instance[_end];
        RoomNode startNode = Map.Instance[_start];
        Stack<INode> nodes = new();
        IEnumerator navigationIter;

        if (!endNode.Empty)
        {
            navigationIter = Map.Instance.NavigateBetweenRooms(startNode, endNode.Occupant);
        }
        else
        {
            if (!endNode.Traversable)
            {
                foreach (RoomNode node in new List<RoomNode> { endNode.GetNodeAs<RoomNode>(Direction.North), endNode.GetNodeAs<RoomNode>(Direction.South), endNode.GetNodeAs<RoomNode>(Direction.West), endNode.GetNodeAs<RoomNode>(Direction.East), endNode.NorthEast, endNode.NorthWest, endNode.SouthEast, endNode.SouthWest })
                {
                    if (node != null && node.Traversable)
                    {
                        endNode = node;
                        break;
                    }
                }
                if (!endNode.Traversable)
                    return;
            }

            navigationIter = Map.Instance.NavigateBetweenRooms(startNode, endNode);
        }

        navigationIter.MoveNext();
        if ((float)navigationIter.Current != float.PositiveInfinity)
        {
            while (navigationIter.MoveNext())
            {
                    nodes.Push(navigationIter.Current as INode);
            }
        }
        else
        {
            Debug.Log("Cannot Reach Location");
            return;
        }

        int pathLength = nodes.Count;

        for (int i = 0; i < pathLength; i++)
        {
            INode node = nodes.Pop();
            _walkingPath[i] = (node is ConnectingNode, node.WorldPosition);
        }
    }
}
