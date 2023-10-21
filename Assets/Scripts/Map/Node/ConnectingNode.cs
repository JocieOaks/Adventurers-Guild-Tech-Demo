using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Map.Node
{
    /// <summary>
    /// Class <see cref="ConnectingNode"/> is a <see cref="IDividerNode"/> that connects two adjoining <see cref="Room"/>s.
    /// </summary>
    public abstract class ConnectingNode : IDividerNode
    {
        //Dictionary containing the distance from a ConnectionNode to every other ConnectionNode in an adjoining room, as well as the Path to the INode.
        //Used for navigation by pre-calculating paths.
        private readonly Dictionary<ConnectingNode, (float, IEnumerable<RoomNode>)> _adjoiningConnectionsDictionary;

        /// <summary>
        /// Initializes a new reference of the <see cref="ConnectingNode"/> class.
        /// </summary>
        /// <param name="connection1">The first of two <see cref="RoomNode"/> that is adjacent to the <see cref="ConnectingNode"/></param>
        /// <param name="connection2">The second of two <see cref="RoomNode"/> that is adjacent to the <see cref="ConnectingNode"/></param>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the <see cref="ConnectingNode"/>.</param>
        protected ConnectingNode(RoomNode connection1, RoomNode connection2, Vector3Int worldPosition)
        {
            FirstNode = connection1;
            SecondNode = connection2;

            Alignment = connection1.WorldPosition.x == connection2.WorldPosition.x
                ? MapAlignment.XEdge
                : MapAlignment.YEdge;

            _adjoiningConnectionsDictionary = new Dictionary<ConnectingNode, (float, IEnumerable<RoomNode>)>();
            WorldPosition = worldPosition;

            GameManager.MapChangingSecond += RegisterRooms;
        }

        /// <inheritdoc/>
        public IEnumerable<INode> AdjacentNodes
        {
            get
            {
                yield return FirstNode;
                yield return SecondNode;
            }
        }

        /// <inheritdoc/>
        public MapAlignment Alignment { get; }

        /// <value>Property <c>ConnectionNodes</c> represents the <see cref="List{ConnectionNode}"/> of <see cref="ConnectingNode"/>s that share an adjacent room with the <see cref="ConnectingNode"/>.</value>
        public List<ConnectingNode> ConnectionNodes => new(_adjoiningConnectionsDictionary.Keys);

        /// <inheritdoc/>
        public RoomNode FirstNode { get; }

        /// <value>Returns true if both <see cref="RoomNode"/>'s bordering the <see cref="ConnectingNode"/> are within the same <see cref="Scripts.Map.Room"/>.</value>
        public bool IsWithinSingleRoom => (FirstNode.Room == SecondNode.Room);

        /// <inheritdoc/>
        public Vector3Int NearestCornerPosition => WorldPosition;

        /// <inheritdoc/>
        public INode Node => this;

        /// <inheritdoc/>
        public Room Room
        {
            get
            {
                if (IsWithinSingleRoom)
                    return FirstNode.Room;
                return null;
            }
        }

        /// <inheritdoc/>
        public RoomNode SecondNode { get; }

        /// <inheritdoc/>
        public abstract bool Obstructed { get; }

        /// <inheritdoc/>
        public Vector3Int WorldPosition { get; protected set; }

        /// <inheritdoc/>
        public abstract Vector3Int Dimensions { get; }

        /// <summary>
        /// Appends the list of <see cref="ConnectingNode"/>s that share a bordering <see cref="Scripts.Map.Room"/>. Used for quickly navigating paths that pass through multiple <see cref="Scripts.Map.Room"/>s.
        /// </summary>
        /// <param name="connection">The <see cref="ConnectingNode"/> that share's an adjacent <see cref="Scripts.Map.Room"/>.</param>
        /// <param name="distance">The path length distance between this <see cref="ConnectingNode"/> and <c>connection</c>.</param>
        /// <param name="path">The path </param>
        public void AddAdjoiningConnection(ConnectingNode connection, float distance, IEnumerable<RoomNode> path)
        {
            if (_adjoiningConnectionsDictionary.TryGetValue(connection, out (float distance, IEnumerable<RoomNode> path) info))
                if (distance > info.distance && RoomNode.VerifyPath(info.path))
                    return;
            _adjoiningConnectionsDictionary[connection] = (distance, path);
        }

        /// <inheritdoc/>
        public bool AdjacentToRoom(Room room)
        {
            return FirstNode.Room == room || SecondNode.Room == room;
        }

        /// <summary>
        /// Called when the <see cref="ConnectingNode"/> is removed from connecting two <see cref="RoomNode"/>s.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Gives the room that is connected to a given room by the <see cref="ConnectingNode"/>.
        /// </summary>
        /// <param name="startingRoom">The initial room that is connected to the <see cref="ConnectingNode"/>.</param>
        /// <returns>Returns the other <see cref="Room"/> adjacent to the <see cref="ConnectingNode"/>.</returns>
        /// <exception cref="System.ArgumentException">Throws this exception if startingRoom is not connected to the <see cref="ConnectingNode"/>.</exception>
        [UsedImplicitly]
        public Room GetConnectedRoom(Room startingRoom)
        {
            if (FirstNode.Room == startingRoom)
                return SecondNode.Room;
            if (SecondNode.Room == startingRoom)
                return FirstNode.Room;

            throw new System.ArgumentException();
        }

        /// <summary>
        /// Gives the path distance from the <see cref="ConnectingNode"/> to nextConnection through the <see cref="Room"/> that both <see cref="INode"/>s are adjacent to.
        /// Used for navigation, by pre-calculating path lengths.
        /// </summary>
        /// <param name="nextConnection">The <see cref="ConnectingNode"/> that is being traversed to, from the current <see cref="INode"/>.</param>
        /// <returns>Returns the path length distance between the two <see cref="INode"/>s.</returns>
        /// <exception cref="System.ArgumentException">Throws exception if the <see cref="ConnectingNode"/> and nextConnection do not share an adjoining room.</exception>
        public float GetDistance(ConnectingNode nextConnection)
        {
            if (_adjoiningConnectionsDictionary.TryGetValue(nextConnection, out (float distance, IEnumerable<RoomNode> path) info))
                return info.distance;
            else
                throw new System.ArgumentException();
        }

        /// <summary>
        /// Gives the path from the <see cref="ConnectingNode"/> to nextConnection through the <see cref="Room"/> that both <see cref="INode"/>s are adjacent to.
        /// Used for navigation, by pre-calculating navigation paths.
        /// </summary>
        /// <param name="nextConnection">The <see cref="ConnectingNode"/> that is being traversed to, from the current <see cref="INode"/>.</param>
        /// <returns>Returns the path between the two <see cref="INode"/>s.</returns>
        /// <exception cref="System.ArgumentException">Throws exception if the <see cref="ConnectingNode"/> and nextConnection do not share an adjoining room.</exception>
        public IEnumerable<RoomNode> GetPath(ConnectingNode nextConnection)
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

        /// <inheritdoc/>
        public virtual RoomNode GetOppositeRoomNode(RoomNode entrance)
        {
            if (entrance == FirstNode)
                return SecondNode;
            else if (entrance == SecondNode)
                return FirstNode;
            else
            {
                throw new System.ArgumentException();
            }
        }

        /// <summary>
        /// Gives the <see cref="RoomNode"/> adjacent to the <see cref="ConnectingNode"/> in the given <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> where the <see cref="RoomNode"/> should be found.</param>
        /// <returns>Returns the <see cref="RoomNode"/> adjacent to the <see cref="ConnectingNode"/> in room.</returns>
        /// <exception cref="System.ArgumentException">Throws exception if the <see cref="ConnectingNode"/> is not adjacent to the given <see cref="Room"/>.</exception>
        public RoomNode GetRoomNode(Room room)
        {
            if (FirstNode.Room == room)
            {
                return FirstNode;
            }
            else if (SecondNode.Room == room)
                return SecondNode;
            else
                throw new System.ArgumentException();
        }

        /// <inheritdoc/>
        public bool HasNavigatedTo(RoomNode node)
        {
            return node == FirstNode || node == SecondNode;
        }

        /// <summary>
        /// Adds this <see cref="ConnectingNode"/> to the <see cref="Scripts.Map.Room.Connections"/> of it's adjacent <see cref="Scripts.Map.Room"/>s.
        /// </summary>
        public virtual void RegisterRooms()
        {
            FirstNode.Room.AddConnection(this);
            if (SecondNode.Room != FirstNode.Room)
                SecondNode.Room.AddConnection(this);

            GameManager.MapChangingSecond -= RegisterRooms;
        }

        /// <summary>
        /// Removes a <see cref="ConnectingNode"/> from the list of nodes that share and adjoining <see cref="Scripts.Map.Room"/> with this <see cref="ConnectingNode"/>.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveAdjoiningConnection(ConnectingNode node)
        {
            _adjoiningConnectionsDictionary.Remove(node);
        }
    }
}