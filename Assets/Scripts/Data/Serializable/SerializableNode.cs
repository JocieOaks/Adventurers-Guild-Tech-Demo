using UnityEngine;
using Newtonsoft.Json;
/// <summary>
/// The <see cref="SerializableNode"/> class is a serializable version of the <see cref="RoomNode"/> class used for data persistence.
/// </summary>
[System.Serializable]
public struct SerializableNode
{
    [JsonProperty] readonly int _floorIndex;
    [JsonProperty] readonly NodeType _south;
    [JsonProperty] readonly NodeType _west;
    [JsonProperty] readonly int _z;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableNode"/> based on a <see cref="RoomNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="RoomNode"/> being serialized.</param>
    /// <param name="checkSouth">An out boolean indicating whether there is a <see cref="DoorConnector"/> to the <see cref="Direction.South"/> that needs to be serialized.</param>
    /// <param name="checkWest">An out boolean indicating whether there is a <see cref="DoorConnector"/> to the <see cref="Direction.West"/> that needs to be serialized.</param>
    public SerializableNode(RoomNode node, out bool checkSouth, out bool checkWest)
    {
        checkSouth = false;
        checkWest = false;

        if (node == RoomNode.Undefined)
        {
            IsUndefined = true;
            _floorIndex = -1;
            _z = -1;
            _south = NodeType.Null;
            _west = NodeType.Null;
            return;
        }
        else
        {
            IsUndefined = false;
        }

        if (node.Floor.Enabled)
            _floorIndex = node.Floor.SpriteIndex;
        else
            _floorIndex = -1;
        _z = node.RoomPosition.z;


        if (node.TryGetNodeAs<RoomNode>(Direction.South))
        {
            _south = NodeType.RoomNode;
        }
        else if (node.TryGetNodeAs<DoorConnector>(Direction.South))
        {
            _south = NodeType.Wall;
            checkSouth = true;
        }
        else if (node.TryGetNodeAs<WallBlocker>(Direction.South))
        {
            _south = NodeType.Wall;
        }
        else
        {
            _south = NodeType.Null;
        }

        if (node.TryGetNodeAs<RoomNode>(Direction.West))
        {
            _west = NodeType.RoomNode;
        }
        else if (node.TryGetNodeAs<DoorConnector>(Direction.West))
        {
            _west = NodeType.Wall;
            checkWest = true;
        }
        else if (node.TryGetNodeAs<WallBlocker>(Direction.West))
        {
            _west = NodeType.Wall;
        }
        else
        {
            _west = NodeType.Null;
        }
    }

    /// <summary>
    /// Indicates if the <see cref="RoomNode"/> is <see cref="RoomNode.Undefined"/> and thus does not need to initialize a new <see cref="RoomNode"/> when the save data is loaded.
    /// </summary>
    public bool IsUndefined { get; private set; }

    /// <summary>
    /// Assigns the serialized data to a new <see cref="RoomNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="RoomNode"/> that is going to copy the data from <see cref="SerializableNode"/>.</param>
    public void SetNode(RoomNode node)
    {
        if (_floorIndex != -1)
            node.Floor.SpriteIndex = _floorIndex;
        else if (node != RoomNode.Undefined && (node.WorldPosition == new Vector3Int(20, 24, 6) || node.WorldPosition == new Vector3Int(20, 23, 6) || node.WorldPosition == new Vector3Int(20, 22, 6)))
            node.Floor.SpriteIndex = 0;

        node.SetZ(_z);
        switch (_south)
        {
            case NodeType.Wall:
                WallBlocker wall = new(node.WorldPosition, MapAlignment.XEdge);
                node.SetNode(Direction.South, wall);
                break;
        }

        switch (_west)
        {
            case NodeType.Wall:
                WallBlocker wall = new(node.WorldPosition, MapAlignment.YEdge);
                node.SetNode(Direction.West, wall);
                break;
        }
    }
}
