using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public struct NavigateJob : IJob
{

    Vector3Int end;
    Vector3Int start;
    NativeArray<(bool isDoor, Vector3Int)> walkingPath;

    /// <inheritdoc/>
    public void Execute()
    {
        RoomNode endNode = Map.Instance[end];
        RoomNode startNode = Map.Instance[start];
        Stack<INode> nodes = new();
        IEnumerator navigationIter;

        navigationIter = Map.Instance.NavigateBetweenRooms(startNode.Empty ? startNode : startNode.Occupant, endNode.Empty ? endNode : endNode.Occupant);

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
            walkingPath[i] = (node is ConnectingNode, node.WorldPosition);
        }
    }

    public NavigateJob(Vector3Int startPosition, Vector3Int endPosition, NativeArray<(bool isDoor, Vector3Int)> walkingPath)
    {
        start = startPosition;
        end = endPosition;
        this.walkingPath = walkingPath;
    }
}
