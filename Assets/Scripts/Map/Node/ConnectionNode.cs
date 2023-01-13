using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// A <see cref="INode"/> that connects two adjoining <see cref="Room"/>s.
/// </summary>
public abstract class ConnectionNode : INode
{
    //Dictionary containing the distance from a ConnectionNode to every other ConnectionNode in an adjoining room, as well as the Path to the INode.
    //Used for navigation by precalculating paths.
    Dictionary<ConnectionNode, (float, IEnumerable<RoomNode>)> _adjoiningConnectionsDictionary;

    protected RoomNode _connection1, _connection2;

    public bool Traversible { get; set; }

    /// <summary>
    /// Initializes a new reference of the <see cref="ConnectionNode"/> class.
    /// </summary>
    /// <param name="connection1">The first of two <see cref="RoomNode"/> that is adjacent to the <see cref="ConnectionNode"/></param>
    /// <param name="connection2">The second of two <see cref="RoomNode"/> that is adjacent to the <see cref="ConnectionNode"/></param>
    /// <param name="traversible">Sets whether the <see cref="ConnectionNode"/> can be traversed by a navigating <see cref="Pawn"/>.</param>
    public ConnectionNode(RoomNode connection1, RoomNode connection2, bool traversible, Vector3Int worldPosition)
    {
        _connection1 = connection1;
        _connection2 = connection2;
        Traversible = traversible;
        _adjoiningConnectionsDictionary = new Dictionary<ConnectionNode, (float, IEnumerable<RoomNode>)>();
        WorldPosition = worldPosition;

        GameManager.MapChangingSecond += RegisterRooms;
    }

    /// <value>Property <c>ConnectionNodes</c> represents the <see cref="List{ConnectionNode}"/> of <see cref="ConnectionNode"/>s that share an adjacent room with the <see cref="ConnectionNode"/>.</value>
    public List<ConnectionNode> ConnectionNodes => new List<ConnectionNode>(_adjoiningConnectionsDictionary.Keys);

    public IEnumerable<RoomNode> Nodes
    {
        get
        {
            yield return _connection1;
            yield return _connection2;
        }
    }

    public (Room, Room) Rooms => (_connection1.Room, _connection2.Room);

    public bool SingleRoom => (_connection1.Room == _connection2.Room);

    /// <value>Property <c>WorldPosition</c> represents the coordinates of the <see cref="ConnectionNode"/> within a <see cref="Map"/>.</value>
    public Vector3Int WorldPosition { get; protected set; }

    public Room Room => null;

    public INode Node => this;

    public void AddAdjoiningConnection(ConnectionNode connection, float distance, IEnumerable<RoomNode> path)
    {
        if (_adjoiningConnectionsDictionary.TryGetValue(connection, out (float distance, IEnumerable<RoomNode> path) info))
            if (distance > info.distance && VerifyPath(info.path))
                return;
        _adjoiningConnectionsDictionary[connection] = (distance, path);
    }

    public bool ConnectedToRoom(Room room)
    {
        return _connection1.Room == room || _connection2.Room == room;
    }

    public abstract void Disconnect();

    /// <summary>
    /// Gives the path distance from the <see cref="ConnectionNode"/> to nextConnection through the <see cref="Room"/> that both <see cref="INode"/>s are adjacent to.
    /// Used for navigation, by precalculating path lengths.
    /// </summary>
    /// <param name="nextConnection">The <see cref="ConnectionNode"/> that is being traversed to, from the current <see cref="INode"/>.</param>
    /// <returns>Returns the path length distance between the two <see cref="INode"/>s.</returns>
    /// <exception cref="System.ArgumentException">Throws exception if the <see cref="ConnectionNode"/> and nextConnection do not share an adjoining room.</exception>
    public float GetDistance(ConnectionNode nextConnection)
    {
        if (_adjoiningConnectionsDictionary.TryGetValue(nextConnection, out (float distance, IEnumerable<RoomNode> path) info))
            return info.distance;
        else
            throw new System.ArgumentException();
    }

    /// <summary>
    /// Gives the path from the <see cref="ConnectionNode"/> to nextConnection through the <see cref="Room"/> that both <see cref="INode"/>s are adjacent to.
    /// Used for navigation, by precalculating navigation paths.
    /// </summary>
    /// <param name="nextConnection">The <see cref="ConnectionNode"/> that is being traversed to, from the current <see cref="INode"/>.</param>
    /// <returns>Returns the path between the two <see cref="INode"/>s.</returns>
    /// <exception cref="System.ArgumentException">Throws exception if the <see cref="ConnectionNode"/> and nextConnection do not share an adjoining room.</exception>
    public IEnumerable<RoomNode> GetPath(ConnectionNode nextConnection)
    {
        if (_adjoiningConnectionsDictionary.TryGetValue(nextConnection, out (float distance, IEnumerable<RoomNode> path) info))
        {
            foreach (RoomNode node in info.path)
            {
                yield return node;
            }
        }
        else
        {
            RegisterRooms();
            nextConnection.RegisterRooms();
            if (_adjoiningConnectionsDictionary.TryGetValue(nextConnection, out info))
            {
                foreach (RoomNode node in info.path)
                {
                    yield return node;
                }
            }
        }
    }

    /// <summary>
    /// Gives the <see cref="RoomNode"/> that is connected to the entrance <see cref="RoomNode"/> by the <see cref="ConnectionNode"/>.
    /// </summary>
    /// <param name="entrance"><see cref="RoomNode"/> that is where an <see cref="Pawn"/> is entering.</param>
    /// <returns>Returns the <see cref="RoomNode"/> that the <see cref="Pawn"/> exits when entering the <see cref="ConnectionNode"/> from entrance.</returns>
    /// <exception cref="System.ArgumentException">Throws this error if the given <see cref="RoomNode"/> is not one of the two <see cref="RoomNode"/>s connected by the <see cref="ConnectionNode"/>.</exception>
    public virtual RoomNode GetRoomNode(RoomNode entrance)
    {
        if (entrance == _connection1)
            return _connection2;
        else if (entrance == _connection2)
            return _connection1;
        else
        {
            throw new System.ArgumentException();
        }
    }

    /// <summary>
    /// Gives the <see cref="RoomNode"/> adjacent to the <see cref="ConnectionNode"/> in the given <see cref="Room"/>.
    /// </summary>
    /// <param name="room">The <see cref="Room"/> where the <see cref="RoomNode"/> should be found.</param>
    /// <returns>Returns the <see cref="RoomNode"/> adjacent to the <see cref="ConnectionNode"/> in room.</returns>
    /// <exception cref="System.ArgumentException">Throws exception if the <see cref="ConnectionNode"/> is not adjacent to the given <see cref="Room"/>.</exception>
    public RoomNode GetRoomNode(Room room)
    {
        if (_connection1.Room == room)
        {
            return _connection1;
        }
        else if (_connection2.Room == room)
            return _connection2;
        else
            throw new System.ArgumentException();
    }

    public virtual void RegisterRooms()
    {
        _connection1.Room.AddConnection(this);
        if (_connection2.Room != _connection1.Room)
            _connection2.Room.AddConnection(this);

        GameManager.MapChangingSecond -= RegisterRooms;
    }

    public void RemoveAdjoiningConnection(ConnectionNode node)
    {
        _adjoiningConnectionsDictionary.Remove(node);
    }

    bool VerifyPath(IEnumerable<RoomNode> path)
    {
        RoomNode previous = path.First();
        (int prevX, int prevY) = previous.Coords;
        foreach (RoomNode nextNode in path.Skip(1))
        {
            (int nextX, int nextY) = nextNode.Coords;
            switch ((nextX - prevX, nextY - prevY))
            {
                case (0, 1):
                    if (previous.GetNode(Direction.North) != nextNode)
                        return false;
                    break;
                case (0, -1):
                    if (previous.GetNode(Direction.South) != nextNode)
                        return false;
                    break;
                case (1, 0):
                    if (previous.GetNode(Direction.East) != nextNode)
                        return false;
                    break;
                case (-1, 0):
                    if (previous.GetNode(Direction.West) != nextNode)
                        return false;
                    break;
                default:
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gives the room that is connected to a given room by the <see cref="ConnectionNode"/>.
    /// </summary>
    /// <param name="startingRoom">The initial room that is connected to the <see cref="ConnectionNode"/>.</param>
    /// <returns>Returns the other <see cref="Room"/> adjacent to the <see cref="ConnectionNode"/>.</returns>
    /// <exception cref="System.ArgumentException">Throws this exception if startingRoom is not connected to the <see cref="ConnectionNode"/>.</exception>
    public Room GetConnectedRoom(Room startingRoom)
    {
        if (_connection1.Room == startingRoom)
            return _connection2.Room;
        if (_connection2.Room == startingRoom)
            return _connection1.Room;

        throw new System.ArgumentException();
    }

    public bool HasNavigatedTo(RoomNode node)
    {
        return node == _connection1 || node == _connection2;
    }
}