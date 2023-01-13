using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class <c>Layer</c> is a <see cref="Room"/> that contains other Rooms inside it.
/// All Rooms within a layer are at the same z elevation, but multiple Layers might be at the same z elevation.
/// A Layer's primary purpose is to be able to hold references to every <see cref="RoomNode"/> contained in that layer.
/// </summary>
public class Layer : Room
{
    int _layerID;

    public Layer(int x, int y, Vector3Int originPosition, int layerID) : base(x, y, originPosition)
    {
        for (int i = 0; i < x; i++)
            for (int j = 0; j < y; j++)
            {
                _nodes[i, j] = RoomNode.Undefined;
            }
        _layerID = layerID;
    }

    public Layer(RoomNode[,] nodes, Vector3Int originPosition, int layerID) : base(nodes, originPosition)
    {
        _layerID = layerID;
    }

    public int LayerID => _layerID;

    public override RoomNode this[int x, int y]
    {
        get
        {
            int xOffset = x - Origin.x;
            int yOffset = y - Origin.y;
            return base[xOffset, yOffset];
        }
    }
    /// <summary>
    /// Creates a new <see cref="RoomNode"/> at a given position, and adds it to the _nodes array.
    /// </summary>
    /// <param name="x">X position of new RoomNode.</param>
    /// <param name="y">Y position of new RoomNode.</param>
    public void InstantiateRoomNode(int x, int y)
    {
        _nodes[x, y] = new RoomNode(this, x, y);
        _nodes[x, y].SetNode(Direction.North, _nodes[x, y + 1]);
        _nodes[x, y].SetNode(Direction.South, _nodes[x, y - 1]);
        _nodes[x, y].SetNode(Direction.East, _nodes[x + 1, y]);
        _nodes[x, y].SetNode(Direction.West, _nodes[x - 1, y]);
    }

    protected override Room CutRoom(int[,] roomDesignation, int flag, int originX, int originY, int endX, int endY)
    {
        RoomNode[,] nodes = new RoomNode[endX - originX, endY - originY];

        Room newRoom = new Room(nodes, Origin + new Vector3Int(originX, originY));

        for (int i = originX; i < endX; i++)
            for (int j = originY; j < endY; j++)
            {
                if (roomDesignation[i, j] == flag)
                {
                    nodes[i - originX, j - originY] = _nodes[i, j];
                    _nodes[i, j].Reassign(newRoom, i - originX, j - originY);
                }
            }

        return newRoom;
    }

    protected override Room SplitOffRooms(RoomNode a, RoomNode b)
    {
        int[,] roomDesignation = new int[Width, Length];
        int layerFlag = 0;

        void DoNext(Queue<RoomNode> queue, int flag)
        {
            RoomNode current = queue.Dequeue();

            foreach (INode node in current.AdjacentNodes)
            {
                if (node == RoomNode.Undefined)
                {
                    layerFlag = flag;
                    continue;
                }
                if (node != null && node is RoomNode next)
                {
                    (int x, int y) = next.Coords;
                    if (roomDesignation[x, y] == 0)
                    {
                        roomDesignation[x, y] = flag;
                        queue.Enqueue(next);
                    }
                    else if (roomDesignation[x, y] != flag)
                    {
                        int newFlag = roomDesignation[x, y];
                        for (int i = 0; i < Width; i++)
                            for (int j = 0; j < Length; j++)
                            {
                                if (roomDesignation[i, j] == flag)
                                    roomDesignation[i, j] = newFlag;
                            }
                        (x, y) = current.Coords;
                        roomDesignation[x, y] = newFlag;
                        while (queue.Count > 0)
                        {
                            current = queue.Dequeue();
                            (x, y) = current.Coords;
                            roomDesignation[x, y] = newFlag;
                        }
                        return;
                    }
                }
            }
        }

        int flag1 = 1, flag2 = 2;
        int size1 = 0, size2 = 0;

        Queue<RoomNode> queue1 = new Queue<RoomNode>(), queue2 = new Queue<RoomNode>();

        queue1.Enqueue(a);
        (int x, int y) = a.Coords;
        roomDesignation[x, y] = flag1;

        queue2.Enqueue(b);
        (x, y) = b.Coords;
        roomDesignation[x, y] = flag2;

        while ((queue1.Count > 0 || layerFlag == flag1) && (queue2.Count > 0 || layerFlag == flag2))
        {
            if ((size1 <= size2 || queue2.Count == 0) && queue1.Count > 0)
            {
                size1++;
                DoNext(queue1, flag1);
            }

            if ((size2 <= size1 || queue1.Count == 0) && queue2.Count > 0)
            {
                size2++;
                DoNext(queue2, flag2);
            }

        }

        int newRoomFlag;
        if (layerFlag != 0)
            newRoomFlag = layerFlag == 1 ? 2 : 1;
        else if (queue1.Count == 0)
            newRoomFlag = flag1;
        else
            newRoomFlag = flag2;

        (int x1, int y1, int x2, int y2) coordinates = default;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Length; j++)
            {
                if (roomDesignation[i, j] == newRoomFlag)
                {
                    if (coordinates == default)
                    {
                        coordinates = (i, j, i, j);
                    }
                    else
                    {
                        coordinates = (Mathf.Min(i, coordinates.x1), Mathf.Min(j, coordinates.y1), Mathf.Max(i, coordinates.x2), Mathf.Max(j, coordinates.y2));
                    }
                }
            }
        }

        Room newRoom = CutRoom(roomDesignation, newRoomFlag, coordinates.x1, coordinates.y1, coordinates.x2 + 1, coordinates.y2 + 1);

        RegisterForUpdate();

        return newRoom;
    }
}
