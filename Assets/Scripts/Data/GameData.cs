using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class GameData
{
    public int MapWidth;
    public int MapLength;
    public int MapHeight;
    public int Layers;

    public List<SerializableDoor> Doors;
    public List<SerializableStair> Stairs;
    public List<SpriteObject> SpriteObjects;

    [JsonIgnore] public List<string> Names;

    [JsonProperty] SerializableNode[] _mapBacking;

    public SerializableNode[] Map 
    { 
        get => _mapBacking;
        set => _mapBacking = value; 
    }
}

[System.Serializable]
public struct SerializableDoor
{
    public Vector3Int Position;
    public MapAlignment Alignment;

    public SerializableDoor(Door door)
    {
        (Position, Alignment) = door.Wall.WallSprite.GetPosition;
    }
}

[System.Serializable]
public struct SerializableStair
{
    public Vector3Int Position;
    public Direction Direction;

    public SerializableStair(Stair stair)
    {
        Position = stair.WorldPosition;
        Direction = stair.Direction;
    }
}


enum NodeType
{
    Null,
    RoomNode,
    Wall
}

[System.Serializable]
public struct SerializableNode
{
    [JsonProperty] int _floorIndex;
    [JsonProperty] int _z;
    [JsonProperty] NodeType _south;
    [JsonProperty] NodeType _west;
    public bool IsUndefined { get; private set; }

    //WallMaterial _southWall;
    //WallMaterial _westWall;

    public SerializableNode(RoomNode node, ref bool checkSouth, ref bool checkWest)
    {
        if(node == RoomNode.Undefined)
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
        else if (node.TryGetNodeAs<Door>(Direction.South))
        {
            _south = NodeType.Wall;
            checkSouth = true;
        }
        else if (node.TryGetNodeAs<Wall>(Direction.South))
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
        else if (node.TryGetNodeAs<Door>(Direction.West))
        {
            _west = NodeType.Wall;
            checkWest = true;
        }
        else if (node.TryGetNodeAs<Wall>(Direction.West))
        {
            _west = NodeType.Wall;
        }
        else
        {
            _west = NodeType.Null;
        }
    }

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
                Wall wall = new Wall(node.WorldPosition, MapAlignment.XEdge);
                node.SetNode(Direction.South, wall);
                break;
            case NodeType.Null:
                /*if(node.GetNodeAs<RoomNode>(Direction.South) != null)
                    node.GetNodeAs<RoomNode>(Direction.South).SetNode(Direction.North,null;
                node.South = null;*/
                break;
        }

        switch (_west)
        {
            case NodeType.Wall:
                Wall wall = new Wall(node.WorldPosition, MapAlignment.YEdge);
                node.SetNode(Direction.West,wall);
                break;
            case NodeType.Null:
                /*if(node.GetNodeAs<RoomNode>(Direction.West) != null)
                    node.GetNodeAs<RoomNode>(Direction.West).SetNode(Direction.East,null;
                node.SetNode(Direction.West,null;*/
                break;
        }
    }
}